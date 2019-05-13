using CyUSB;
using System;
using System.Windows.Forms;

namespace CsrpServer
{
	public partial class Form1 : Form
	{
		private USBDeviceList usbDevices = null;
		CsrpServer server = null;


		public Form1()
		{
			InitializeComponent();
		}


		private void Form1_Load(object sender, EventArgs e)
		{
			usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
			usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
			usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);

			server = new CsrpServer(64000000, 400000, usbDevices);
			if (server != null)
			{
				server.SetDevice();
			}
		}

		private void usbDevices_DeviceRemoved(object sender, EventArgs e)
		{
			if (server != null)
			{
				server.SetDevice();
			}
		}

		private void usbDevices_DeviceAttached(object sender, EventArgs e)
		{
			if (server != null)
			{
				server.SetDevice();
			}
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
