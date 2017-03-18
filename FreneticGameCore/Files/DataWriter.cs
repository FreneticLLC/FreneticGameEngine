using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FreneticGameCore.Files
{
    public class DataWriter
    {
        public Stream Internal;

        public DataWriter(Stream stream)
        {
            Internal = stream;
        }

        public void WriteLocation(Location loc)
        {
            Internal.Write(loc.ToDoubleBytes(), 0, 24);
        }

        public void WriteByte(byte x)
        {
            Internal.WriteByte(x);
        }

        public void WriteUShort(ushort x)
        {
            Internal.Write(Utilities.UshortToBytes(x), 0, 2);
        }

        public void WriteInt(int x)
        {
            Internal.Write(Utilities.IntToBytes(x), 0, 4);
        }

        public void WriteFloat(float x)
        {
            Internal.Write(Utilities.FloatToBytes(x), 0, 4);
        }

        public void WriteDouble(double x)
        {
            Internal.Write(Utilities.DoubleToBytes(x), 0, 8);
        }

        public void WriteLong(long x)
        {
            Internal.Write(Utilities.LongToBytes(x), 0, 8);
        }

        public void WriteBytes(byte[] bits)
        {
            Internal.Write(bits, 0, bits.Length);
        }

        public void WriteFullBytes(byte[] data)
        {
            WriteInt(data.Length);
            WriteBytes(data);
        }

        public void WriteFullString(string str)
        {
            byte[] data = FileHandler.encoding.GetBytes(str);
            WriteInt(data.Length);
            WriteBytes(data);
        }
    }
}
