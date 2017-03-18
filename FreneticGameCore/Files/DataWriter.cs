using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FreneticGameCore.Files
{
    /// <summary>
    /// Helper for writing data to a stream.
    /// </summary>
    public class DataWriter
    {
        /// <summary>
        /// The internal stream.
        /// </summary>
        public Stream Internal;

        /// <summary>
        /// Constructs the data writer.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        public DataWriter(Stream stream)
        {
            Internal = stream;
        }

        /// <summary>
        /// Write a location object (24 bytes).
        /// </summary>
        /// <param name="loc">The data.</param>
        public void WriteLocation(Location loc)
        {
            Internal.Write(loc.ToDoubleBytes(), 0, 24);
        }

        /// <summary>
        /// Write a byte.
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteByte(byte x)
        {
            Internal.WriteByte(x);
        }

        /// <summary>
        /// Write an unsigned short integer (2 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteUShort(ushort x)
        {
            Internal.Write(Utilities.UshortToBytes(x), 0, 2);
        }

        /// <summary>
        /// Write an integer (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteInt(int x)
        {
            Internal.Write(Utilities.IntToBytes(x), 0, 4);
        }

        /// <summary>
        /// Write a float (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteFloat(float x)
        {
            Internal.Write(Utilities.FloatToBytes(x), 0, 4);
        }

        /// <summary>
        /// Write a double (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteDouble(double x)
        {
            Internal.Write(Utilities.DoubleToBytes(x), 0, 8);
        }

        /// <summary>
        /// Write a long integer (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteLong(long x)
        {
            Internal.Write(Utilities.LongToBytes(x), 0, 8);
        }

        /// <summary>
        /// Write a set of bytes directly to the stream.
        /// </summary>
        /// <param name="bits">The bytes.</param>
        public void WriteBytes(byte[] bits)
        {
            Internal.Write(bits, 0, bits.Length);
        }

        /// <summary>
        /// Write a "full set" of bytes to the stream: prefixing the bytes with a 4-byte length indicator.
        /// </summary>
        /// <param name="data">The data.</param>
        public void WriteFullBytes(byte[] data)
        {
            WriteInt(data.Length);
            WriteBytes(data);
        }

        /// <summary>
        /// Write a "full" string to the stream: prefixing the string with a 4-byte length indicator.
        /// </summary>
        /// <param name="str">The data.</param>
        public void WriteFullString(string str)
        {
            byte[] data = FileHandler.encoding.GetBytes(str);
            WriteInt(data.Length);
            WriteBytes(data);
        }
    }
}
