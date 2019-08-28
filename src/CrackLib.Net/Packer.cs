namespace CrackLib.Net
{
    using System;
    using System.IO;
    using static System.String;
    public interface IPacker
    {
        void Put(string s);
        string Get(int num);
        int Find(string s);
        int Size();
        void Close();
    }

    /// <summary>
    /// Manages the packed and indexed dictionary files.
    /// </summary>
    public class Packer : IPacker
    {
        public const int Magic = 0x70775631;
        public const int NumWords = 16;
        public const int MaxWordLength = 32;
        public static readonly int MaxBlockLength = (MaxWordLength * NumWords);
        public const int IntegerSize = 4;
        protected internal RandomAccessFile DataFile; // data file
        protected internal RandomAccessFile IndexFile; // index file
        protected internal RandomAccessFile HashFile; // hash file
        protected internal string Mode;
        protected internal PackerHeader Header;
        protected internal int[] Hwms = new int[256];
        protected internal int Count;
        protected internal string LastWord;
        protected internal string[] Data = new string[NumWords];
        protected internal int Block = -1;

        /// <summary>
        /// Create a new instance. </summary>
        /// <param name="path">physical path where index files are stored</param>
        /// <exception cref="IOException"> if an I/O error occurs. </exception>
        public Packer(string path) : this(path, "words.pack.pwd", "words.pack.pwi", "words.pack.hwm", "r")
        {
        }

        /// <summary>
        /// Create a new instance. </summary>
        /// <param name="path">physical path where index files are stored</param>
        /// <param name="name"> the base name of the packer files, ".pwd", etc. will be appended. </param>
        /// <param name="mode"> "r" or "rw". </param>
        /// <exception cref="IOException"> if an I/O error occurs. </exception>
        public Packer(string path, string name, string mode) : this(path, name + ".pwd", name + ".pwi", name + ".hwm", mode)
        {
        }

        /// <summary>
        /// Create a new instance. </summary>
        /// <param name="path">physical path where index files are stored</param>
        /// <param name="pwd"> the pwd file name. </param>
        /// <param name="pwi"> the pwi file name. </param>
        /// <param name="hwm"> the hwm file name. </param>
        /// <param name="mode"> "r" or "rw". </param>
        public Packer(string path, string pwd, string pwi, string hwm, string mode)
        {
            this.Mode = mode;

            if (!(mode.Equals("rw") || mode.Equals("r")))
            {
                throw new ArgumentException("Mode must be \"rw\" or \"r\"");
            }

            var validPath = (!IsNullOrWhiteSpace(path) && Directory.Exists(path));

            var pwdPath = validPath ? Path.Combine(path, pwd) : pwd;
            var pwiPath = validPath ? Path.Combine(path, pwi) : pwi;
            var hwmPath = validPath ? Path.Combine(path, hwm) : hwm;

            if (mode.Equals("rw"))
            {
                // we have to blow it away on write.
                if (Directory.Exists(pwdPath)) Directory.Delete(pwdPath, true); else File.Delete(pwdPath);
                if (Directory.Exists(pwiPath)) Directory.Delete(pwiPath, true); else File.Delete(pwiPath);
                if (Directory.Exists(hwmPath)) Directory.Delete(hwmPath, true); else File.Delete(hwmPath);
            }

            DataFile = new RandomAccessFile(pwdPath, mode); // data file
            IndexFile = new RandomAccessFile(pwiPath, mode); // index file
            try
            {
                HashFile = new RandomAccessFile(hwmPath, mode); // hash file
            }
            catch (IOException)
            {
                HashFile = null; // hashFile isn't mandatory.
            }
            if (mode.Equals("rw"))
            {
                Header = new PackerHeader
                {
                    Magic = Magic,
                    BlockLen = (short)NumWords,
                    NumWords = 0
                };
                // write the header.
                IndexFile.Write(Header.Bytes);
            }
            else
            {
                Header = PackerHeader.Parse(IndexFile);
                if (Header.Magic != Magic)
                {
                    throw new IOException("Magic Number mismatch");
                }
                else if (Header.BlockLen != NumWords)
                {
                    throw new IOException("Size mismatch");
                }
                // populate the hwms..
                if (HashFile != null)
                {
                    byte[] b = new byte[4];
                    for (var i = 0; i < Hwms.Length; i++)
                    {
                        HashFile.ReadFully(b);
                        Hwms[i] = Util.GetIntLe(b);
                    }
                }
            }
        }

        /// <summary>
        /// Flush the output and close streams
        /// </summary>
        public void Close()
        {
            lock (this)
            {
                if (Mode.Equals("rw"))
                {
                    Flush();
                    IndexFile.Seek(0);
                    IndexFile.Write(Header.Bytes);
                    if (HashFile != null)
                    {
                        // Give non-existent letters decent indices.
                        for (int i = 1; i <= 0xff; i++)
                        {
                            if (Hwms[i] == 0)
                            {
                                Hwms[i] = Hwms[i - 1];
                            }
                        }

                        foreach (var t in Hwms)
                        {
                            HashFile.Write(Util.GetBytesLe(t));
                        }
                    }
                }
                IndexFile.Close();
                DataFile.Close();
                HashFile?.Close();
            }
        }

        /// <summary>
        /// Put a word into dictionary index
        /// </summary>
        /// <param name="s"></param>
        public void Put(string s)
        {
            lock (this)
            {
                if (!Mode.Equals("rw"))
                {
                    throw new IOException("Not opened for write.");
                }
                if (ReferenceEquals(s, null))
                {
                    throw new NullReferenceException();
                }
                if (!ReferenceEquals(LastWord, null) && Compare(LastWord, s, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new ArgumentException("put's must be in alphabetical order!");
                }
                else
                {
                    LastWord = s;
                }
                // truncate if > MaxWordLength (including \0)
                Data[Count] = s.Length > MaxWordLength - 1 ? s.Substring(0, MaxWordLength - 1) : s;
                Hwms[s[0] & 0xff] = Header.NumWords;
                ++Count;
                Header.NumWords = Header.NumWords + 1;
                if (Count >= NumWords)
                {
                    Flush();
                }
            }
        }

        /// <summary>
        /// Flush the output to data index file
        /// </summary>
        private void Flush()
        {
            lock (this)
            {
                if (!Mode.Equals("rw"))
                {
                    throw new IOException("Not opened for write.");
                }
                int index = (int)DataFile.FilePointer;
                // write the pos to the index file.
                IndexFile.Write(Util.GetBytesLe(index));
                DataFile.Write(Data[0].GetBytes()); // write null terminated string.
                DataFile.Write(new byte[1] { 0 });
                string ostr = Data[0];
                for (int i = 1; i < NumWords; i++)
                {
                    string nstr = Data[i];
                    if (!ReferenceEquals(nstr, null))
                    { // (nstr[0])
                        int j = 0;
                        for (j = 0; j < ostr.Length && j < nstr.Length && (ostr[j] == nstr[j]); j++)
                        {
                        }
                        DataFile.Write(new byte[1] { (byte)(j & 0xff) }); // write the index
                        DataFile.Write(nstr.Substring(j).GetBytes()); // write the new
                                                                      // string from j
                                                                      // to end.
                    }
                    DataFile.Write(new byte[1] { 0 }); // write a null;
                    ostr = nstr;
                }
                Data = new string[NumWords];
                Count = 0;
            }
        }

        /// <summary>
        /// Get a word from index
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string Get(int num)
        {
            lock (this)
            {
                if (!Mode.Equals("r"))
                {
                    throw new IOException("Can only get in mode \"r\"");
                }
                if (Header.NumWords <= num)
                { // too big
                    return null;
                }
                byte[] index = new byte[4];
                byte[] index2 = new byte[4];
                int thisblock = num / NumWords;
                if (Block == thisblock)
                {
                    return (Data[num % NumWords]);
                }

                // get the index of this block.
                IndexFile.Seek(PackerHeader.SizeOf() + (thisblock * IntegerSize));
                IndexFile.ReadFully(index, 0, index.Length);
                byte[] buf = null;
                try
                {
                    // get the index of the next block.
                    IndexFile.Seek(IndexFile.FilePointer + IntegerSize);
                    IndexFile.ReadFully(index2, 0, index2.Length);
                    buf = new byte[Util.GetIntLe(index2) - Util.GetIntLe(index)];
                }
                catch (IOException)
                { // EOF
                    buf = new byte[MaxBlockLength];
                }

                // read the data
                DataFile.Seek(Util.GetIntLe(index));
                Util.ReadFullish(DataFile, buf, 0, buf.Length);
                Block = thisblock;
                byte[] strbuf = new byte[MaxWordLength];
                int a = 0;
                int off = 0;
                for (int i = 0; i < NumWords; i++)
                {
                    int b = a;
                    for (; buf[b] != (byte)'\0'; b++)
                    {
                    }

                    if (b == a)
                    { // not more \0's
                        break;
                    }
                    Array.Copy(buf, a, strbuf, off, (b - a));
                    Data[i] = StringHelper.NewString(strbuf, 0, off + (b - a));
                    a = b + 2;
                    off = buf[a - 1];
                }
                return (Data[num % NumWords]);
            }
        }

        /// <summary>
        /// Find a word in index
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int Find(string s)
        {
            if (!Mode.Equals("r"))
            {
                throw new IOException("Can only find in mode \"r\"");
            }
            int index = (int)s[0];
            int lwm = index != 0 ? Hwms[index - 1] : 0;
            int hwm = Hwms[index];
            for (; ; )
            {
                int middle = lwm + ((hwm - lwm + 1) / 2);
                if (middle == hwm)
                {
                    break;
                }
                int cmp = Compare(s, Get(middle), StringComparison.OrdinalIgnoreCase);
                if (cmp < 0)
                {
                    hwm = middle;
                }
                else if (cmp > 0)
                {
                    lwm = middle;
                }
                else
                {
                    return middle;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns total number of words present in index
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return Header.NumWords;
        }
    }
}