using MonoLibUsb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoCsrpServer
{
	public static class MonoEzUsbApi
	{
		private enum ram_mode
		{
			_undef = 0,
			internal_only,      /* hardware first-stage loader */
			skip_internal,      /* first phase, second-stage loader */
			skip_external       /* second phase, second-stage loader */
		}

			private class ram_poke_context
		{
			public MonoUsbDeviceHandle device;
			public ram_mode mode;
			public int total;
			public int count;
		};


		private const int EIO = 5;      /* I/O error */
		private const int LIBUSB_ENDPOINT_OUT = 0x00;
		private const int LIBUSB_REQUEST_TYPE_VENDOR = (0x02 << 5);
		private const int LIBUSB_RECIPIENT_DEVICE = 0x00;
		private const int LIBUSB_ERROR_IO = -1;
		private const int LIBUSB_ERROR_TIMEOUT = -7;
		private const int RETRY_LIMIT = 5;
		private const byte RW_INTERNAL = 0xA0;
		private const byte RW_MEMORY = 0xA3;
		private static int verbose = 0;

		private static int ezusb_write(MonoUsbDeviceHandle device, string label,
			byte opcode, int addr, byte[] data, int len)
		{
			int status;

			if (verbose > 1)
				Console.WriteLine("{0}, addr 0x{1:x8} len {2} (0x{3:x4})", label, addr, len, len);
			status = MonoUsbApi.ControlTransfer(device,
				LIBUSB_ENDPOINT_OUT | LIBUSB_REQUEST_TYPE_VENDOR | LIBUSB_RECIPIENT_DEVICE,
				opcode, (short)(addr & 0xFFFF), (short)(addr >> 16),
				data, (short)(len), 1000);
			if (status != len)
			{
				if (status < 0)
					Console.WriteLine("{0}: {1}", label, MonoUsbApi.StrError((MonoUsbError)status));
				else
					Console.WriteLine("{0} ==> {1}", label, status);
			}
			return (status < 0) ? -EIO : 0;
		}

		private static bool ezusb_cpucs(MonoUsbDeviceHandle device, int addr, bool doRun)
		{
			int status;
			byte[] data = new byte[1] { doRun ? (byte)0x00 : (byte)0x01 };

			if (verbose > 0)
				Console.WriteLine("{0}", (data[0] != 0) ? "stop CPU" : "reset CPU");
			status = MonoUsbApi.ControlTransfer(device,
				LIBUSB_ENDPOINT_OUT | LIBUSB_REQUEST_TYPE_VENDOR | LIBUSB_RECIPIENT_DEVICE,
				RW_INTERNAL, (short)(addr & 0xFFFF), (short)(addr >> 16),
				data, 1, 1000);
			if ((status != 1) &&
				/* We may get an I/O error from libusb as the device disappears */
				((!doRun) || (status != LIBUSB_ERROR_IO)))
			{
				string mesg = "can't modify CPUCS";
				if (status < 0)
					Console.WriteLine("{0}: {1}", mesg, MonoUsbApi.StrError((MonoUsbError)status));
				else
					Console.WriteLine("{0}", mesg);
				return false;
			}
			else
				return true;
		}

		private static int parse_iic(string image, ram_poke_context context)
		{
			byte[] iic_header = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
			byte[] bytes;
			byte[] data = new byte[4096];
			byte[] block_header = new byte[4];

			try
			{
				using (FileStream fs = new FileStream(image, FileMode.Open, FileAccess.Read))
				{
					if (fs.Length == 0)
						return -1;
					bytes = new byte[fs.Length];
					fs.Read(bytes, 0, bytes.Length);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				return -1;
			}

			int pos = 0;

			if (pos + iic_header.Length > bytes.Length)
			{
				Console.WriteLine("IIC image does not contain executable code - cannot load to RAM.");
				return -1;
			}
			Buffer.BlockCopy(bytes, pos, iic_header, 0, iic_header.Length); pos += iic_header.Length;
			if (iic_header[0] != 0xC2)
			{
				Console.WriteLine("IIC image does not contain FX2(LP) executable code - cannot load to RAM.");
				return -1;
			}

			for (; ; )
			{
				/* Ignore the trailing reset IIC data (5 bytes) */
				if (pos >= (bytes.Length - 5))
					break;

				if (pos + block_header.Length > bytes.Length)
				{
					Console.WriteLine("unable to read IIC block header");
					return -1;
				}
				Buffer.BlockCopy(bytes, pos, block_header, 0, block_header.Length); pos += block_header.Length;
				int data_len = (block_header[0] << 8) + block_header[1];
				int data_addr = (block_header[2] << 8) + block_header[3];
				if (data_len > data.Length)
				{
					/* If this is ever reported as an error, switch to using malloc/realloc */
					Console.WriteLine("IIC data block too small - please report this error to libusb.info");
					return -1;
				}

				if (pos + data_len > bytes.Length)
				{
					Console.WriteLine("read error");
					return -1;
				}
				Buffer.BlockCopy(bytes, pos, data, 0, (int)data_len); pos += data_len;

				int rc = ram_poke(context, data_addr, false, data, data_len);
				if (rc < 0)
					return -1;
			}
			return 0;

		}

		private static int ram_poke(ram_poke_context ctx, int addr, bool external, byte[] data, int len)
		{
			int rc;
			uint retry = 0;

			ctx.total += len;
			ctx.count++;

			/* Retry this till we get a real error. Control messages are not
			 * NAKed (just dropped) so time out means is a real problem.
			 */
			while ((rc = ezusb_write(ctx.device,
				external? "write external" : "write on-chip",
				external? RW_MEMORY : RW_INTERNAL,
				addr, data, len)) < 0
				&& retry < RETRY_LIMIT) {
				if (rc != LIBUSB_ERROR_TIMEOUT)
					break;
				retry += 1;
			}
			return rc;
		}

		public static int ezusb_load_ram(MonoUsbDeviceHandle device, string path)
		{
			int cpucs_addr = 0xe600;
			ram_poke_context ctx = new ram_poke_context();

			ctx.mode = ram_mode.internal_only;

			/* if required, halt the CPU while we overwrite its code/data */
			if (cpucs_addr != 0 && !ezusb_cpucs(device, cpucs_addr, false))
			{
				return -1;
			}

			ctx.device = device;
			ctx.total = ctx.count = 0;

			int status = parse_iic(path, ctx);
			if (status < 0)
			{
				Console.WriteLine("unable to upload {0}", path);
				return status;
			}

			/* if required, reset the CPU so it runs what we just uploaded */
			if (cpucs_addr != 0 && !ezusb_cpucs(device, cpucs_addr, true))
				return -1;

			return status;
		}
	}
}
