namespace CrackLib.Net
{
    using System;
    public class PackerHeader
    {
        internal int magic;
        internal int numWords;
        internal short blockLen;
        internal short pad;

        public PackerHeader()
        {
        }

        public PackerHeader(int magic, int numWords, short blockLen, short pad)
        {
            this.magic = magic;
            this.numWords = numWords;
            this.blockLen = blockLen;
            this.pad = pad;
        }

        public virtual int Magic
        {
            get => magic;
            set => magic = value;
        }


        public virtual int NumWords
        {
            get => numWords;
            set => numWords = value;
        }


        public virtual short BlockLen
        {
            get => blockLen;
            set => blockLen = value;
        }


        public virtual short Pad
        {
            get => pad;
            set => pad = value;
        }


        public static int SizeOf()
        {
            return 12;
        }

        public static PackerHeader Parse(RandomAccessFile raf)
        {
            byte[] b = new byte[SizeOf()];
            raf.ReadFully(b);
            return Parse(b);
        }

        public static PackerHeader Parse(byte[] b)
        {
            byte[] b1 = new byte[4]; // magic, numWords
            byte[] b2 = new byte[2]; // blocklen, pad
            Array.Copy(b, 0, b1, 0, 4);
            int magic = Util.GetIntLe(b1);
            Array.Copy(b, 4, b1, 0, 4);
            int numWords = Util.GetIntLe(b1);
            Array.Copy(b, 8, b2, 0, 2);
            short blockLen = Util.GetShortLe(b2);
            Array.Copy(b, 10, b2, 0, 2);
            short pad = Util.GetShortLe(b2);
            return new PackerHeader(magic, numWords, blockLen, pad);
        }

        public virtual byte[] Bytes
        {
            get
            {
                byte[] b = new byte[SizeOf()];
                Array.Copy(Util.GetBytesLe(magic), 0, b, 0, 4);
                Array.Copy(Util.GetBytesLe(numWords), 0, b, 4, 4);
                Array.Copy(Util.GetBytesLe(blockLen), 0, b, 8, 2);
                Array.Copy(Util.GetBytesLe(pad), 0, b, 10, 2);
                return b;
            }
        }

        public override string ToString()
        {
            return "magic=0x" + magic.ToString("x") + ",numWords="
                    + numWords + ",blockLen=" + blockLen + ",pad=" + pad;
        }
    }
}