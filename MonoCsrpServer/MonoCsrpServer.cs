using MonoLibUsb;
using MonoLibUsb.Profile;
using Rapidnack.Net;
using System;
using System.IO;

namespace MonoCsrpServer
{
	public class MonoCsrpServer : SdrServerBase
	{
		private const string firmware = "Spi1FifoIn.iic";

		private MonoUsbSessionHandle session = null;
		private MonoUsbProfileList profileList = null;
		private MonoUsbDeviceHandle connectedDevice = null;


		public MonoCsrpServer(uint fpgaClock, int rate, MonoUsbSessionHandle session, MonoUsbProfileList profileList)
			: base(fpgaClock, rate)
		{
			this.session = session;
			this.profileList = profileList;
		}


		protected override IAvalonPacket CreateAvalonPacket()
		{
			MonoUsbProfile sampleProfile = profileList.GetList().Find(
				p => p.DeviceDescriptor.VendorID == 0x04b4 && p.DeviceDescriptor.ProductID == 0x1004);
			if (sampleProfile == null)
			{
				if (Listener != null)
				{
					Listener.Stop();
				}
				connectedDevice = null;

				MonoUsbProfile noEepromProfile = profileList.GetList().Find(
					p => p.DeviceDescriptor.VendorID == 0x04b4 && (ushort)p.DeviceDescriptor.ProductID == 0x8613);
				if (noEepromProfile == null)
					return null;
				string arg0 = Environment.GetCommandLineArgs()[0];
				string path = Path.Combine(Path.GetDirectoryName(arg0), firmware);
				MonoUsbDeviceHandle noEepromDevice = noEepromProfile.OpenDeviceHandle();
				MonoEzUsbApi.ezusb_load_ram(noEepromDevice, path);

				return null;
			}

			if (connectedDevice != null)
				return null;

			MonoUsbDeviceHandle sampleDevice = sampleProfile.OpenDeviceHandle();
			connectedDevice = sampleDevice;
			return new MonoCsrpAvalonPacket(connectedDevice);
		}

		protected override int ReadIQData(byte[] rxBuf, int length)
		{
			int xferLen;
			MonoUsbApi.BulkTransfer(connectedDevice, 0x82, rxBuf, length, out xferLen, 3000);
			return xferLen;
		}
	}
}
