namespace CrackLib.Net
{
    internal static class StringHelper
    {
        public static string NewString(byte[] bytes, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetString(bytes, index, count);
        }

        public static byte[] GetBytes(this string self)
        {
            return GetBytesForEncoding(System.Text.Encoding.UTF8, self);
        }

        private static byte[] GetBytesForEncoding(System.Text.Encoding encoding, string s)
        {
            if (s != null)
            {
                byte[] bytes = new byte[encoding.GetByteCount(s)];
                encoding.GetBytes(s, 0, s.Length, bytes, 0);
                return bytes;
            }

            return new byte[1] {0};
        }
    }
}