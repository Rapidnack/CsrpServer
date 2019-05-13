using System;
using System.Collections.Generic;

namespace Rapidnack.Net
{
	public abstract class AvalonPacketBase : IAvalonPacket
	{
		#region # private field

		private bool byteCh = false;
		private bool bytesEscape = false;
		private bool bitsEscape = false;
		private int timeoutInSec = 3;
		private int waitMs = 100;

		#endregion


		protected abstract void SpiWrite(byte[] txBuf, int length);

		protected abstract void SpiRead(byte[] rxBuf, int length);


		#region # public method

		public void PrintBytes(string title, byte[] bytes)
		{
			Console.Write($"{title}[{bytes.Length}]: ");
			for (int i = 0; i < bytes.Length; i++)
			{
				Console.Write($" {bytes[i]:x2}");
			}
			Console.WriteLine("");
		}

		public byte[] WritePacket(uint addr, uint data)
		{
			uint[] array = new uint[] { data };
			return WritePacket(addr, array, 0, array.Length, false);
		}

		public byte[] WritePacket(uint addr, uint[] array, int start, int length, bool isIncremental = false)
		{
			if (array.Length == 0)
				return new byte[0];
			if (start < 0 || array.Length < start + length)
				return new byte[0];

			int numBytes = 4;
			byte[] bytes = new byte[numBytes * array.Length];
			for (int i = start; i < start + length; i++)
			{
				byte[] tempBytes = BitConverter.GetBytes(array[i]);
				tempBytes.CopyTo(bytes, numBytes * i);
			}
			return WritePacket(addr, bytes, 0, bytes.Length, isIncremental);
		}

		public byte[] WritePacket(uint addr, byte[] array, int start, int length, bool isIncremental = false)
		{
			if (array.Length == 0)
				return new byte[0];
			if (start < 0 || array.Length < start + length)
				return new byte[0];

			byte[] packet = new byte[8 + length];
			byte[] sizeBytes = BitConverter.GetBytes((UInt16)length);
			byte[] addrBytes = BitConverter.GetBytes(addr);
			packet[0] = (byte)(isIncremental ? 0x04 : 0x00);
			packet[1] = 0;
			packet[2] = sizeBytes[1];
			packet[3] = sizeBytes[0];
			packet[4] = addrBytes[3];
			packet[5] = addrBytes[2];
			packet[6] = addrBytes[1];
			packet[7] = addrBytes[0];
			for (int i = start; i < start + length; i++)
			{
				packet[8 + i] = array[i];
			}
			byte[] bits = BytesToBits(PacketToBytes(packet));
			SpiWrite(bits, bits.Length);

			byte[] response = new byte[4];
			ReceiveResponse(response, 0, response.Length);
			return response;
		}

		public uint ReadPacket(uint addr)
		{
			uint[] array = new uint[1];
			ReadPacket(addr, array, 0, array.Length, false);
			return array[0];
		}

		public int ReadPacket(uint addr, uint[] array, int start, int length, bool isIncremental = false)
		{
			if (array.Length == 0)
				return 0;
			if (start < 0 || array.Length < start + length)
				return 0;

			int numBytes = 4;
			byte[] recvPacket = new byte[numBytes * array.Length];
			int numReceivedBytes = ReadPacket(addr, recvPacket, 0, recvPacket.Length, isIncremental);
			byte[] dataBytes = new byte[numBytes];
			for (int i = start; i < start + length; i++)
			{
				for (int b = 0; b < numBytes; b++)
				{
					dataBytes[b] = recvPacket[numBytes * i + b];
				}
				array[i] = BitConverter.ToUInt32(dataBytes, 0);
			}
			return numReceivedBytes / numBytes;
		}

		public int ReadPacket(uint addr, byte[] array, int start, int length, bool isIncremental = false)
		{
			if (array.Length == 0)
				return 0;
			if (start < 0 || array.Length < start + length)
				return 0;

			byte[] packet = new byte[8];
			byte[] sizeBytes = BitConverter.GetBytes((UInt16)length);
			byte[] addrBytes = BitConverter.GetBytes(addr);
			packet[0] = (byte)(isIncremental ? 0x14 : 0x10);
			packet[1] = 0;
			packet[2] = sizeBytes[1];
			packet[3] = sizeBytes[0];
			packet[4] = addrBytes[3];
			packet[5] = addrBytes[2];
			packet[6] = addrBytes[1];
			packet[7] = addrBytes[0];
			byte[] bits = BytesToBits(PacketToBytes(packet));
			SpiWrite(bits, bits.Length);

			return ReceiveResponse(array, start, length);
		}

		#endregion


		#region # private method

		private byte[] PacketToBytes(byte[] packet)
		{
			List<byte> bytes = new List<byte>();
			bytes.Add(0x7c);
			bytes.Add(0);
			bytes.Add(0x7a);
			for (int i = 0; i < packet.Length; i++)
			{
				byte p = packet[i];
				if (0x7a <= p && p <= 0x7d)
				{
					if (i == packet.Length - 1)
					{
						bytes.Add(0x7b);
					}
					bytes.Add(0x7d);
					bytes.Add((byte)(p ^ 0x20));
				}
				else
				{
					if (i == packet.Length - 1)
					{
						bytes.Add(0x7b);
					}
					bytes.Add(p);
				}
			}
			return bytes.ToArray();
		}

		private byte[] BytesToPacket(byte[] bytes)
		{
			List<byte> packet = new List<byte>();
			foreach (byte b in bytes)
			{
				if (b == 0x7a || b == 0x7b)
				{
					// Dropped
				}
				else if (b == 0x7c)
				{
					byteCh = true;
					// Dropped
				}
				else if (b == 0x7d)
				{
					bytesEscape = true;
					// Dropped
				}
				else
				{
					if (byteCh)
					{
						byteCh = false;
						// Dropped
					}
					else if (bytesEscape)
					{
						bytesEscape = false;
						packet.Add((byte)(b ^ 0x20));
					}
					else
					{
						packet.Add(b);
					}
				}
			}
			return packet.ToArray();
		}

		private byte[] BytesToBits(byte[] bytes)
		{
			List<byte> bits = new List<byte>();
			foreach (byte b in bytes)
			{
				if (b == 0x4a || b == 0x4d)
				{
					bits.Add(0x4d);
					bits.Add((byte)(b ^ 0x20));
				}
				else
				{
					bits.Add(b);
				}
			}

			return bits.ToArray();
		}

		private byte[] BitsToBytes(byte[] bits)
		{
			List<byte> bytes = new List<byte>();
			for (int i = 0; i < bits.Length; i++)
			{
				byte b = bits[i];
				if (b == 0x4a)
				{
					// Dropped
				}
				else if (b == 0x4d)
				{
					bitsEscape = true;
					// Dropped
				}
				else
				{
					if (bitsEscape)
					{
						bitsEscape = false;
						bytes.Add((byte)(b ^ 0x20));
					}
					else
					{
						bytes.Add(b);
					}
				}
			}
			return bytes.ToArray();
		}

		private int ReceiveResponse(byte[] array, int start, int length)
		{
			if (start < 0 || array.Length < start + length)
				return 0;

			byteCh = false;
			bytesEscape = false;
			bitsEscape = false;

			int emptyCount = 0;

			int responseLength = 0;
			while (responseLength < length)
			{
				byte[] bits = new byte[(int)((length - responseLength) * 1.1) + 4];
				SpiRead(bits, bits.Length);

				byte[] packet = BytesToPacket(BitsToBytes(bits));

				if (packet.Length > 0)
				{
					int copyLength = Math.Min(length - responseLength, packet.Length);
					Buffer.BlockCopy(packet, 0, array, start + responseLength, copyLength);
					responseLength += packet.Length;

					emptyCount = 0;
				}
				else
				{
					emptyCount += (emptyCount < 10) ? 1 : 10;
					if (emptyCount >= (timeoutInSec * 1000) / waitMs)
						break;
					System.Threading.Thread.Sleep(waitMs * ((emptyCount < 10) ? 1 : 10));
				}
			}

			return responseLength;
		}

		#endregion
	}
}
