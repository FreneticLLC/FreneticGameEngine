using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FreneticGameCore.Files
{
    public class DataReader
    {
        public Stream Internal;

        public DataReader(Stream stream)
        {
            Internal = stream;
        }

        public byte ReadByte()
        {
            int r = Internal.ReadByte();
            if (r < 0)
            {
                throw new EndOfStreamException("Failed to read from stream, " + Internal.Length + " bytes were available (now none)...");
            }
            return (byte)r;
        }

        public int Available
        {
            get
            {
                return (int)(Internal.Length - Internal.Position);
            }
        }

        public byte[] ReadBytes(int count)
        {
            byte[] b = new byte[count];
            for (int i = 0; i < count; i++)
            {
                b[i] = ReadByte();
            }
            return b;
        }

        public Location ReadLocation()
        {
            return Location.FromDoubleBytes(ReadBytes(24), 0);
        }

        public short ReadShort()
        {
            return Utilities.BytesToShort(ReadBytes(2));
        }

        public ushort ReadUShort()
        {
            return Utilities.BytesToUshort(ReadBytes(2));
        }

        public int ReadInt()
        {
            return Utilities.BytesToInt(ReadBytes(4));
        }

        public long ReadLong()
        {
            return Utilities.BytesToLong(ReadBytes(8));
        }

        public float ReadFloat()
        {
            return Utilities.BytesToFloat(ReadBytes(4));
        }

        public double ReadDouble()
        {
            return Utilities.BytesToDouble(ReadBytes(8));
        }

        public string ReadString(int length)
        {
            return FileHandler.encoding.GetString(ReadBytes(length));
        }

        public byte[] ReadFullBytes()
        {
            int len = ReadInt();
            return ReadBytes(len);
        }

        public string ReadFullString()
        {
            int len = ReadInt();
            return ReadString(len);
        }

        public void Close()
        {
            Internal.Close();
        }
    }
}
