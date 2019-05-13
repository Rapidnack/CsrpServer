using CyUSB;
using Rapidnack.Net;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Fx2lpTest
{
	public partial class Form1 : Form
	{
		private const string firmware = "Spi1FifoIn.iic";

		private USBDeviceList usbDevices = null;
		private CyUSBDevice connectedDevice = null;
		private IAvalonPacket avalonPacket = null;
		private CyBulkEndPoint endpoint2 = null;


		public Form1()
		{
			InitializeComponent();
		}


		public void SetDevice()
		{
			avalonPacket = CreateAvalonPacket();
			if (avalonPacket == null)
				return;

			// blink LED
			for (int i = 0; i < 5; i++)
			{
				var response = avalonPacket.WritePacket(0, 1);
				avalonPacket.PrintBytes("response", response);
				Thread.Sleep(100);
				response = avalonPacket.WritePacket(0, 0);
				avalonPacket.PrintBytes("response", response);
				Thread.Sleep(100);
			}
		}

		private IAvalonPacket CreateAvalonPacket()
		{
			CyUSBDevice sampleDevice = usbDevices[0x04b4, 0x1004] as CyUSBDevice;
			if (sampleDevice == null)
			{
				connectedDevice = null;

				CyFX2Device noEepromDevice = usbDevices[0x04b4, 0x8613] as CyFX2Device;
				if (noEepromDevice == null)
					return null;
				string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), firmware);
				noEepromDevice.LoadExternalRam(path);

				return null;
			}

			if (connectedDevice != null)
				return null;

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
			return new Fx2lpAvalonPacket(outEndpoint, inEndpoint);
		}


		private void Form1_Load(object sender, EventArgs e)
		{
			usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
			usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
			usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);

			SetDevice();
		}

		private void usbDevices_DeviceRemoved(object sender, EventArgs e)
		{
			Console.WriteLine("DeviceRemoved");
			SetDevice();
		}

		private void usbDevices_DeviceAttached(object sender, EventArgs e)
		{
			Console.WriteLine("DeviceAttached");
			SetDevice();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (usbDevices != null)
			{
				usbDevices.Dispose();
			}
		}
	}
}
