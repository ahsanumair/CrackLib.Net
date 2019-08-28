namespace CrackLib.Net
{
    using System.IO;
    using System.IO.MemoryMappedFiles;
    public class RandomAccessFile
    {
        private readonly FileMode _mode;
        private readonly MemoryMappedViewStream _viewStream;
        private readonly FileStream _writeStream;
        public long FilePointer { get; private set; }

        public RandomAccessFile(string name, string fileMode)
        {
            if (fileMode == "rw")
            {
                _mode = FileMode.OpenOrCreate;
                _writeStream = File.Create(name);
            }
            else
            {
                _mode = FileMode.Open;
                using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(name, _mode))
                {
                    _viewStream = memoryMappedFile.CreateViewStream();
                }
            }
        }

        public virtual void Write(byte[] b)
        {
            WriteBytes(b, 0, b.Length);
        }

        public virtual void Write(byte[] b, int off, int len)
        {
            WriteBytes(b, off, len);
        }

        public void WriteBytes(string s)
        {
            int len = s.Length;
            byte[] b = s.GetBytes();
            WriteBytes(b, 0, len);
        }

        public void WriteBytes(byte[] buffer, int offset, int length)
        {
            if (_mode == FileMode.OpenOrCreate)
            {
                _writeStream.Seek(FilePointer, SeekOrigin.Begin);
                _writeStream.Write(buffer, offset, length);
                FilePointer = _writeStream.Position;
            }
            else
            {
                _viewStream.Seek(FilePointer, SeekOrigin.Begin);
                _viewStream.Write(buffer, offset, length);
                FilePointer = _viewStream.Position;
            }
        }

        public virtual int Read(byte[] b, int off, int len)
        {
            return ReadBytes(b, off, len);
        }

        public virtual int Read(byte[] b)
        {
            return ReadBytes(b, 0, b.Length);
        }

        public int ReadByte(byte[] buffer, int offset, int length)
        {
            return _viewStream.ReadByte();
        }

        public int ReadBytes(byte[] buffer, int offset, int length)
        {
            int count;

            if (_mode == FileMode.OpenOrCreate)
            {
                _writeStream.Seek(FilePointer, SeekOrigin.Begin);
                count = _writeStream.Read(buffer, offset, length);
                FilePointer = _writeStream.Position;
            }
            else
            {
                _viewStream.Seek(FilePointer, SeekOrigin.Begin);
                count = _viewStream.Read(buffer, offset, length);
                FilePointer = _viewStream.Position;
            }

            return count;
        }

        public void ReadFully(byte[] b)
        {
            ReadFully(b, 0, b.Length);
        }

        public void ReadFully(byte[] b, int off, int len)
        {
            int n = 0;
            do
            {
                var count = Read(b, off + n, len - n);
                if (count < 0)
                {
                    throw new EndOfStreamException();
                }
                n += count;
            } while (n < len);
        }

        public long Seek(long offset)
        {
            FilePointer = _mode == FileMode.OpenOrCreate ? _writeStream.Seek((int) offset, SeekOrigin.Begin) : _viewStream.Seek((int)offset, SeekOrigin.Begin);

            return FilePointer;
        }

        public void Close()
        {
            if (_mode == FileMode.OpenOrCreate)
            {
                _writeStream?.Close();
            }
            else
            {
                _viewStream?.Close();
            }
        }
    }
}