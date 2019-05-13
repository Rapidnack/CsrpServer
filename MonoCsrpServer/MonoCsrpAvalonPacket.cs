using MonoLibUsb;
using Rapidnack.Net;
using System;

namespace MonoCsrpServer
{
	public class MonoCsrpAvalonPacket : AvalonPacketBase
	{
		private const int BUF_SIZE = 64;
		private const int TIMEOUT = 3000;

		MonoUsbDeviceHandle deviceHandle = null;
		private byte[] outData = new byte[BUF_SIZE];
		private byte[] inData = new byte[BUF_SIZE];


		public MonoCsrpAvalonPacket(MonoUsbDeviceHandle deviceHandle)
			: base()
		{
			this.deviceHandle = deviceHandle;
		}


		protected override void SpiWrite(byte[] txBuf, int length)
		{
			int sentLength = 0;
			while (sentLength < length)
			{
				int size = Math.Min(BUF_SIZE, length - sentLength);
				Buffer.BlockCopy(txBuf, sentLength, outData, 0, size);

				int transferred;
				MonoUsbApi.BulkTransfer(deviceHandle, 0x01, outData, size, out transferred, TIMEOUT);
				if (transferred > 0)
				{
					MonoUsbApi.BulkTransfer(deviceHandle, 0x81, inData, size, out transferred, TIMEOUT);
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

				int transferred;
				MonoUsbApi.BulkTransfer(deviceHandle, 0x01, outData, size, out transferred, TIMEOUT);
				if (transferred > 0)
				{
					MonoUsbApi.BulkTransfer(deviceHandle, 0x81, inData, transferred, out transferred, TIMEOUT);
				}

				Buffer.BlockCopy(inData, 0, rxBuf, sentLength, size);
				sentLength += size;
			}
		}
	}
}
