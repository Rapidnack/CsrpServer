using CyUSB;
using Rapidnack.Net;
using System;
using System.IO;
using System.Windows.Forms;

namespace CsrpServer
{
	public class CsrpServer : SdrServerBase
	{
		private const string firmware = "Spi1FifoIn.iic";

		private USBDeviceList usbDevices = null;
		private CyUSBDevice connectedDevice = null;
		private CyBulkEndPoint endpoint2 = null;


		public CsrpServer(uint fpgaClock, int rate, USBDeviceList usbDevices)
			: base(fpgaClock, rate)
		{
			this.usbDevices = usbDevices;
		}


		protected override IAvalonPacket CreateAvalonPacket()
		{
			CyUSBDevice sampleDevice = usbDevices[0x04b4, 0x1004] as CyUSBDevice;
			if (sampleDevice == null)
			{
				if (Listener != null)
				{
					Listener.Stop();
				}
				if (connectedDevice != null)
				{
					//Console.WriteLine("DeviceRemoved");
					connectedDevice = null;
				}

				CyFX2Device noEepromDevice = usbDevices[0x04b4, 0x8613] as CyFX2Device;
				if (noEepromDevice == null)
					return null;
				string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), firmware);				
				noEepromDevice.LoadExternalRam(path);

				return null;
			}

			if (connectedDevice != null)
				return null;

			//Console.WriteLine("DeviceAttached");
			CyBulkEndPoint outEndpoint = sampleDevice.EndPointOf(0x01) as CyBulkEndPoint;
			if (outEndpoint == null)
				return null;
			CyBulkEndPoint inEndpoint = sampleDevice.EndPointOf(0x81) as CyBulkEndPoint;
			if (inEndpoint == null)
				return null;
			endpoint2 = sampleDevice.EndPointOf(0x82) as CyBulkEndPoint;
			if (endpoint2 == null)
				return null;
			connectedDevice = sampleDevice;
			return new CsrpAvalonPacket(outEndpoint, inEndpoint);
		}

		protected override int ReadIQData(byte[] rxBuf, int length)
		{
			int xferLen = rxBuf.Length;
			endpoint2.XferData(ref rxBuf, ref xferLen);
			return xferLen;
		}
	}
}
