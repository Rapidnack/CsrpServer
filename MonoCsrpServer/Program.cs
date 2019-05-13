using MonoLibUsb;
using MonoLibUsb.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoCsrpServer
{
	class Program
	{
		// The first time the Session property is used it creates a new session
		// handle instance in '__sessionHandle' and returns it. Subsequent 
		// request simply return '__sessionHandle'.
		private static MonoUsbSessionHandle __sessionHandle;
		public static MonoUsbSessionHandle Session
		{
			get
			{
				if (ReferenceEquals(__sessionHandle, null))
					__sessionHandle = new MonoUsbSessionHandle();
				return __sessionHandle;
			}
		}

		static void Main(string[] args)
		{
			int rate = 400000;
			bool err = false;
			foreach (var s in args)
			{
				int i;
				if (int.TryParse(s, out i))
				{
					if (50 <= i && i <= 1600)
					{
						rate = i * 1000;
					}
					else
					{
						err = true;
					}
				}
				else
				{
					err = true;
				}
			}
			if (err)
			{
				string arg0 = Environment.GetCommandLineArgs()[0];
				string name = System.IO.Path.GetFileNameWithoutExtension(arg0).Split('.')[0];
				Console.WriteLine($"sudo mono {name}.exe [rate(kHz):50-1600]");
				return;
			}

			int numDevices = -1;
			MonoUsbProfileList profileList = null;

			// Initialize the context.
			if (Session.IsInvalid)
				throw new Exception("Failed to initialize context.");

			MonoUsbApi.SetDebug(Session, 0);
			// Create a MonoUsbProfileList instance.
			profileList = new MonoUsbProfileList();

			MonoCsrpServer server = new MonoCsrpServer(64000000, rate, Session, profileList);
			if (server == null)
				return;

			try
			{
				while (true)
				{
					// The list is initially empty.
					// Each time refresh is called the list contents are updated. 
					int ret = profileList.Refresh(Session);
					if (ret < 0) throw new Exception("Failed to retrieve device list.");

					if (numDevices != ret)
					{
						// Iterate through the profile list; write the device descriptor to
						// console output.
						//foreach (MonoUsbProfile profile in profileList)
						//	Console.WriteLine(profile.DeviceDescriptor);

						numDevices = ret;
						Console.WriteLine($"{numDevices} device(s) found.");
						server.SetDevice();
					}

					Thread.Sleep(1000);
				}
			}
			finally
			{
				// Since profile list, profiles, and sessions use safe handles the
				// code below is not required but it is considered good programming
				// to explicitly free and close these handle when they are no longer
				// in-use.
				profileList.Close();
				Session.Close();
			}
		}
	}
}
