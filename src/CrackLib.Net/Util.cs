namespace CrackLib.Net
{
	internal class Util
	{
		public static short GetShortLe(byte[] b)
		{
			return (short)(((b[1] & 0xff) << 8) | (b[0] & 0xff));
		}

		public static int GetIntLe(byte[] b)
		{
			return (int)(((b[3] & 0xff) << 24) | ((b[2] & 0xff) << 16) | ((b[1] & 0xff) << 8) | (b[0] & 0xff));
		}

		public static byte[] GetBytesLe(int i)
		{
			byte[] b = new byte[4];
			b[0] = unchecked((byte)(i & 0xff));
			b[1] = unchecked((byte)((i >> 8) & 0xff));
			b[2] = unchecked((byte)((i >> 16) & 0xff));
			b[3] = unchecked((byte)((i >> 24) & 0xff));
			return b;
		}

		public static byte[] GetBytesLe(short s)
		{
			byte[] b = new byte[2];
			b[0] = unchecked((byte)(s & 0xff));
			b[1] = unchecked((byte)((s >> 8) & 0xff));
			return b;
		}

        public static int ReadFullish(RandomAccessFile raf, byte[] b, int off, int len)
		{
			int i = 0;
			int j = 0;
			while ((j = raf.Read(b, off + i, len - i)) != -1 && i != len)
			{
				i += j;
			}
			return i;
		}
	}

}