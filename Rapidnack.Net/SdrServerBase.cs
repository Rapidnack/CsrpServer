using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidnack.Net
{
	public abstract class SdrServerBase
	{
		private enum ERunningState
		{
			Stop,
			Start,
			Continued
		}


		private const int NUM_SAMPLES = 1024;

		private const int MIN_RATE = 50000; // 50k/100k/200k/400k/800k/1.6M
		private const int MIN_RATE_MUL = 0; //   0/   1/   2/   3/   4/   5
		private const int MAX_RATE_MUL = 5;

		private const int MIN_FREQ = 0;
		private const int MAX_FREQ = 475000000; // 475MHz

		private const int MIN_GAIN = 0;
		private const int MAX_GAIN = 16;

		private const int DEFAULT_DESTPORT = 28888;

		private const int KEEP_ALIVE_SECONDS = 3;

		private IAvalonPacket avalonPacket = null;
		private uint fpgaClock = 64000000; // 64MHz
		private double periodFactor = 0.9;
		private byte[] inData = new byte[4 * NUM_SAMPLES];
		private byte[] packet = new byte[4 + 4 * NUM_SAMPLES];
		private int sequence;


		protected TcpListener Listener { get; set; } = null;

		private int _rateMul = 3; // 400k
		private int RateMul
		{
			get
			{
				return _rateMul;
			}
			set
			{
				if (value < MIN_RATE_MUL) value = MIN_RATE_MUL;
				if (MAX_RATE_MUL < value) value = MAX_RATE_MUL;
				_rateMul = value;

				avalonPacket?.WritePacket(0x0, (uint)((0 << 17) + (Gain << 12) + (RateMul << 8)));
			}
		}

		private int Rate
		{
			get
			{
				return MIN_RATE << RateMul;
			}
			set
			{
				RateMul = (int)Math.Ceiling(Math.Log(value / MIN_RATE) / Math.Log(2));
			}
		}

		private int _freq = 1000000;
		private int Freq
		{
			get
			{
				return _freq;
			}
			set
			{
				if (value < MIN_FREQ) value = MIN_FREQ;
				if (MAX_FREQ < value) value = MAX_FREQ;
				_freq = value;

				avalonPacket?.WritePacket(0x10, freqToPhaseInc(fpgaClock, Freq));
			}
		}

		private int DDC
		{
			get
			{
				return (int)freqToDDC(fpgaClock, Freq);
			}
		}

		private int _gain = 8;
		private int Gain
		{
			get
			{
				return _gain;
			}
			set
			{
				if (value < MIN_GAIN) value = MIN_GAIN;
				if (MAX_GAIN < value) value = MAX_GAIN;
				_gain = value;

				avalonPacket?.WritePacket(0x0, (uint)((0 << 17) + (Gain << 12) + (RateMul << 8)));
			}
		}

		private string DestAddr { get; set; } = string.Empty;

		private int DestPort { get; set; } = DEFAULT_DESTPORT;

		private bool Header { get; set; } = true;

		private ERunningState RunningState { get; set; } = ERunningState.Stop;


		public SdrServerBase(uint fpgaClock, int rate)
		{
			this.fpgaClock = fpgaClock;
			Rate = rate;
		}


		protected abstract IAvalonPacket CreateAvalonPacket();

		protected abstract int ReadIQData(byte[] rxBuf, int length);


		public void SetDevice()
		{
			avalonPacket = CreateAvalonPacket();
			if (avalonPacket == null)
				return;

			CancellationTokenSource cts = new CancellationTokenSource();
			var ct = cts.Token;
			Task.Run(() =>
			{
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

				UdpClient udp = new UdpClient();
				Listener = new TcpListener(IPAddress.Any, 28888);
				try
				{
					Gain = Gain; // Rate is also set
					Freq = Freq;

					Listener.Start();
					Console.Write("Waiting for connection...");

					CancellationTokenSource serverCts = null;
					while (!ct.IsCancellationRequested)
					{
						TcpClient client = Listener.AcceptTcpClient();
						Console.WriteLine();
						Console.WriteLine("Connection accepted({0}:{1}).",
							((IPEndPoint)client.Client.RemoteEndPoint).Address,
							((IPEndPoint)client.Client.RemoteEndPoint).Port);

						if (serverCts != null)
						{
							serverCts.Cancel();
						}

						serverCts = new CancellationTokenSource();
						var serverCt = serverCts.Token;
						Task.Run(() =>
						{
							Server(udp, client, serverCt);
						}, serverCt);
					}
				}
				catch (SocketException ex)
				{
					if (ex.ErrorCode != 10004)
					{
						Console.WriteLine(ex.Message);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				finally
				{
					Listener.Stop();
					udp.Close();
					Console.WriteLine("stopped.");
				}
			}, ct);
		}

		private void Server(UdpClient udp, TcpClient client, CancellationToken ct)
		{
			try
			{
				Header = true;
				DestAddr = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
				DestPort = DEFAULT_DESTPORT;

				NetworkStream ns = client.GetStream();
				WriteLine(ns, "DEVICE -");

				try
				{
					DateTime lastKeepAlive = DateTime.Now;

					double deltaT = 1.0 / Rate;
					TimeSpan period = TimeSpan.FromSeconds(deltaT * NUM_SAMPLES * periodFactor);
					DateTime lastTime = DateTime.Now;

					while (!ct.IsCancellationRequested)
					{
						if (Header == false) // GNU Radio
						{
							if (DateTime.Now - lastKeepAlive > TimeSpan.FromSeconds(KEEP_ALIVE_SECONDS))
								break;
						}

						if (ns.DataAvailable)
						{
							lastKeepAlive = DateTime.Now;
							processInput(ns);
						}
						else if (RunningState == ERunningState.Stop)
						{
							lastTime = DateTime.Now;
							Thread.Sleep(1);
						}
						else
						{
							int xferLen = ReadIQData(inData, inData.Length);
							if (xferLen != inData.Length)
							{
								Console.WriteLine($"the response size {xferLen / 4} not equal to the requested size {inData.Length / 4}");
								if (xferLen == 0)
									break;
								else
									continue;
							}

							int packetStart = 0;
							int packetLength = 4 * NUM_SAMPLES;
							if (Header)
							{
								packet[0] = (byte)((RunningState == ERunningState.Start) ? 0x10 : 0x00);
								packet[1] = 0;
								packet[2] = (byte)(sequence & 0xff);
								packet[3] = (byte)((sequence >> 8) & 0xff);
								packetStart += 4;
								packetLength += 4;
							}
							RunningState = ERunningState.Continued;

							inData.CopyTo(packet, packetStart);

							udp.Send(packet, packetLength, DestAddr, DestPort);
							sequence++;
							if (sequence == 0x10000)
							{
								sequence = 0;
							}
						}
					}
				}
				catch (ObjectDisposedException)
				{
					// nothing to do
				}
				catch (OperationCanceledException)
				{
					// nothing to do
				}
				finally
				{
					ns.Close();
					client.Close();
					Console.WriteLine("Connection closed.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void processInput(NetworkStream ns)
		{
			string str = ReadLine(ns);
			if (str.Trim() == string.Empty)
				return; // keep alive

			Console.WriteLine(str);

			if (str.StartsWith("DEVICE -", StringComparison.CurrentCultureIgnoreCase) ||
				str.StartsWith("DEVICE 0", StringComparison.CurrentCultureIgnoreCase))
			{
				string s = $"DEVICE CSRP" +
					$"|{MIN_GAIN}.000|{MAX_GAIN}.000|1.000" +
					$"|{MIN_RATE << MAX_RATE_MUL}.000" +
					$"|{NUM_SAMPLES}" +
					$"|RX" +
					$"|00000000" +
					$"|default" +
					$"|default";
				WriteLine(ns, s);
			}
			else if (str.StartsWith("DEVICE !", StringComparison.CurrentCultureIgnoreCase))
			{
				WriteLine(ns, "DEVICE -");
			}
			else if (str.StartsWith("RATE", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					string s = str.Split(new char[] { ' ' }, 2)[1];
					Rate = (int)Math.Round(double.Parse(s));
					if (RunningState == ERunningState.Continued)
					{
						sequence = 0;
						RunningState = ERunningState.Start;
					}
					WriteLine(ns, "RATE OK");
				}
				else
				{
					WriteLine(ns, $"RATE {Rate}.000");
				}
			}
			else if (str.StartsWith("FREQ", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					string s = str.Split(new char[] { ' ' }, 2)[1];
					int value = (int)Math.Round(double.Parse(s));
					Freq = value;
					if (RunningState == ERunningState.Continued)
					{
						sequence = 0;
						RunningState = ERunningState.Start;
					}

					string ret = "OK";
					if (value < MIN_FREQ) ret = "LOW";
					if (MAX_FREQ < value) ret = "HIGH";

					WriteLine(ns, $"FREQ {ret} 0.000 0.000 {DDC}.000 {DDC}.000");
				}
				else
				{
					WriteLine(ns, $"FREQ {Freq}.000");
				}
			}
			else if (str.StartsWith("GAIN", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					string s = str.Split(new char[] { ' ' }, 2)[1];
					Gain = (int)Math.Round(double.Parse(s));
					if (RunningState == ERunningState.Continued)
					{
						sequence = 0;
						RunningState = ERunningState.Start;
					}
					WriteLine(ns, "GAIN OK");
				}
				else
				{
					WriteLine(ns, $"GAIN {Gain}.000");
				}
			}
			else if (str.StartsWith("ANTENNA", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					WriteLine(ns, "ANTENNA OK");
				}
				else
				{
					WriteLine(ns, "ANTENNA RX");
				}
			}
			else if (str.StartsWith("CLOCK_SRC", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					WriteLine(ns, "CLOCK_SRC OK");
				}
				else
				{
					WriteLine(ns, "CLOCK_SRC default");
				}
			}
			else if (str.StartsWith("TIME_SRC", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					WriteLine(ns, "TIME_SRC OK");
				}
				else
				{
					WriteLine(ns, "TIME_SRC default");
				}
			}
			else if (str.StartsWith("DEST", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					string s = str.Split(new char[] { ' ' }, 2)[1];
					string[] sArray = s.Split(new char[] { ':' }, 2);
					try
					{
						DestAddr = sArray[0];
						if (sArray.Length > 1)
						{
							DestPort = int.Parse(sArray[1]);
						}
						WriteLine(ns, $"DEST OK {DestAddr}:{DestPort}");
					}
					catch
					{
						WriteLine(ns, "DEST FAIL Failed to set destination");
					}
				}
				else
				{
					WriteLine(ns, $"DEST {DestAddr}:{DestPort}");
				}
			}
			else if (str.StartsWith("HEADER", StringComparison.CurrentCultureIgnoreCase))
			{
				if (str.Contains(" "))
				{
					string s = str.Split(new char[] { ' ' }, 2)[1];
					Header = (s == "ON");
					WriteLine(ns, "HEADER OK");
				}
				else
				{
					string header = Header ? "ON" : "OFF";
					WriteLine(ns, $"HEADER {header}");
				}
			}
			else if (str.StartsWith("GO", StringComparison.CurrentCultureIgnoreCase))
			{
				sequence = 0;
				RunningState = ERunningState.Start;
				WriteLine(ns, "GO OK");
			}
			else if (str.StartsWith("STOP", StringComparison.CurrentCultureIgnoreCase))
			{
				RunningState = ERunningState.Stop;
				WriteLine(ns, "STOP OK");
			}
			else
			{
				WriteLine(ns, $"{str} UNKNOWN");
			}
		}

		private void WriteLine(NetworkStream ns, string s)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(s + "\n");
			ns.Write(bytes, 0, bytes.Length);
			Console.WriteLine($"{s}");
		}

		private string ReadLine(NetworkStream ns)
		{
			System.Text.Encoding enc = System.Text.Encoding.UTF8;
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			byte[] resBytes = new byte[256];
			int resSize = 0;
			do
			{
				resSize = ns.Read(resBytes, 0, resBytes.Length);
				if (resSize == 0)
				{
					Console.WriteLine("Client disconnected.");
					break;
				}
				ms.Write(resBytes, 0, resSize);
			} while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
			string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
			ms.Close();
			resMsg = resMsg.TrimEnd('\n');

			return resMsg;
		}

		private double freqToDDC(double clk, double freq)
		{ // in Hz
			double clkDiv2 = clk / 2;
			int bank = (int)Math.Floor(freq / clkDiv2);
			double lo = freq - clkDiv2 * bank;
			if ((bank % 2) == 1)
				lo = clkDiv2 - lo;

			return lo;
		}

		private uint freqToPhaseInc(double clk, double freq)
		{ // in Hz
			double phaseInc360 = (double)0x80000000UL * 2; // 32 bits full scale

			return (uint)(phaseInc360 * (freqToDDC(clk, freq) / clk));
		}
	}
}
