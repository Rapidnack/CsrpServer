using CyUSB;
using Rapidnack.Net;
using System;

namespace Fx2lpTest
{
	public class Fx2lpAvalonPacket : AvalonPacketBase
	{
		private const int BUF_SIZE = 64;

		private CyBulkEndPoint outEndpoint = null;
		private CyBulkEndPoint inEndpoint = null;
		private byte[] outData = new byte[BUF_SIZE];
		private byte[] inData = new byte[BUF_SIZE];


		public Fx2lpAvalonPacket(CyBulkEndPoint outEndpoint, CyBulkEndPoint inEndpoint)
			: base()
		{
			this.outEndpoint = outEndpoint;
			this.inEndpoint = inEndpoint;
		}


		protected override void SpiWrite(byte[] txBuf, int length)
		{
			int sentLength = 0;
			while (sentLength < length)
			{
				int size = Math.Min(BUF_SIZE, length - sentLength);
				Buffer.BlockCopy(txBuf, sentLength, outData, 0, size);

				int count = size;
				outEndpoint.XferData(ref outData, ref count);
				if (count > 0)
				{
					inEndpoint.XferData(ref inData, ref count);
				}

				sentLength += size;
			}
		}

		protected override void SpiRead(byte[] rxBuf, int length)
		{
			for (int i = 0; i < outData.Length; i++)
			{
				outData[i] = 0x4a;
			}

			int sentLength = 0;
			while (sentLength < length)
			{
				int size = Math.Min(BUF_SIZE, length - sentLength);

				int count = size;
				outEndpoint.XferData(ref outData, ref count);
				if (count > 0)
				{
					inEndpoint.XferData(ref inData, ref count);
				}

				Buffer.BlockCopy(inData, 0, rxBuf, sentLength, size);
				sentLength += size;
			}
		}
	}
}
