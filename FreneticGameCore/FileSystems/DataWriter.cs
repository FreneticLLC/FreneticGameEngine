//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FreneticGameCore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;
using FreneticGameCore.MathHelpers;

namespace FreneticGameCore.FileSystems
{
    /// <summary>
    /// Helper for writing data to a stream.
    /// </summary>
    public class DataWriter
    {
        /// <summary>
        /// The internal stream.
        /// </summary>
        public DataStream Internal;

        private readonly byte[] HelperBytes = new byte[32];

        /// <summary>
        /// Constructs the data writer.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        public DataWriter(DataStream stream)
        {
            Internal = stream;
        }

        /// <summary>
        /// Write a location object (24 bytes).
        /// </summary>
        /// <param name="loc">The data.</param>
        public void WriteLocation(Location loc)
        {
            loc.ToDoubleBytes(HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 24);
        }

        /// <summary>
        /// Write a location object (12 bytes).
        /// </summary>
        /// <param name="loc">The data.</param>
        public void WriteLocationFloat(Location loc)
        {
            WriteFloat(loc.XF);
            WriteFloat(loc.YF);
            WriteFloat(loc.ZF);
        }

        /// <summary>
        /// Write a quaternion object (32 bytes).
        /// </summary>
        /// <param name="quat">The data.</param>
        public void WriteQuaternion(Quaternion quat)
        {
            quat.ToDoubleBytes(HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 32);
        }

        /// <summary>
        /// Write a quaternion object (16 bytes).
        /// </summary>
        /// <param name="quat">The data.</param>
        public void WriteQuaternionFloat(Quaternion quat)
        {
            WriteFloat(quat.XF);
            WriteFloat(quat.YF);
            WriteFloat(quat.ZF);
            WriteFloat(quat.WF);
        }

        /// <summary>
        /// Write a view direction from a location object (4 bytes).
        /// </summary>
        /// <param name="loc">The data.</param>
        public void WriteViewDirection(Location loc)
        {
            float yaw = (float)loc.Yaw;
            while (yaw < 0f)
            {
                yaw += 360f;
            }
            while (yaw > 360f)
            {
                yaw -= 360f;
            }
            WriteUShort((ushort)((ushort.MaxValue / 360f) * yaw));
            float pitch = (float)loc.Pitch;
            if (pitch < -89.999f)
            {
                pitch = -89.999f;
            }
            if (pitch > 89.999f)
            {
                pitch = 89.999f;
            }
            pitch += 90;
            WriteUShort((ushort)((ushort.MaxValue / 180f) * pitch));
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
        /// Write a signed byte.
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteSByte(sbyte x)
        {
            Internal.WriteByte((byte)x);
        }

        /// <summary>
        /// Write a bool.
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteBool(bool x)
        {
            WriteByte((byte)(x ? 1 : 0));
        }

        /// <summary>
        /// Write a character (2 bytes).
        /// </summary>
        /// <param name="x"></param>
        public void WriteChar(char x)
        {
            PrimitiveConversionHelper.UShort16ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 2);
        }

        /// <summary>
        /// Write a short integer (2 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteShort(short x)
        {
            PrimitiveConversionHelper.Short16ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 2);
        }

        /// <summary>
        /// Write an unsigned short integer (2 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteUShort(ushort x)
        {
            PrimitiveConversionHelper.UShort16ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 2);
        }

        /// <summary>
        /// Write an integer (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteInt(int x)
        {
            PrimitiveConversionHelper.Int32ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 4);
        }

        /// <summary>
        /// Write an unsigned integer (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteUInt(uint x)
        {
            PrimitiveConversionHelper.UInt32ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 4);
        }

        /// <summary>
        /// Write a float (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteFloat(float x)
        {
            PrimitiveConversionHelper.Float32ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 4);
        }

        /// <summary>
        /// Write a double (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteDouble(double x)
        {
            PrimitiveConversionHelper.Double64ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 8);
        }

        /// <summary>
        /// Write a long integer (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteLong(long x)
        {
            PrimitiveConversionHelper.Long64ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 8);
        }

        /// <summary>
        /// Write an unsigned long integer (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteULong(ulong x)
        {
            PrimitiveConversionHelper.ULong64ToBytes(x, HelperBytes, 0);
            Internal.Write(HelperBytes, 0, 8);
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
        /// Write a "full set" of bytes to the stream: prefixing the bytes with a var int length indicator.
        /// </summary>
        /// <param name="data">The data.</param>
        public void WriteFullBytesVar(byte[] data)
        {
            WriteVarInt(data.Length);
            WriteBytes(data);
        }

        /// <summary>
        /// Write a "full" string to the stream: prefixing the string with a var int length indicator.
        /// </summary>
        /// <param name="str">The data.</param>
        public void WriteFullStringVar(string str)
        {
            byte[] data = StringConversionHelper.UTF8Encoding.GetBytes(str);
            WriteVarInt(data.Length);
            WriteBytes(data);
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
            byte[] data = StringConversionHelper.UTF8Encoding.GetBytes(str);
            WriteInt(data.Length);
            WriteBytes(data);
        }

        /// <summary>
        /// Writes a variable integer to the stream.
        /// This writes 1 byte at a time, where the final bit signifies whether another byte of data follows.
        /// <para>For example, if we assume (DATA) to be any combination of 7 bits (zero or one, repeated seven times.
        /// Then (DATA)0 is one full VarInt. Additionally, (DATA)1(DATA)0 is as well a single full VarInt. (DATA)1(DATA)1(DATA)1(DATA)0 is as well as singular complete VarInt.</para>
        /// <para>This can theoretically continue on to infinity for massive integers, where every byte has its final bit as a 1, until the very last byte's last bit, which is always a 0.</para>
        /// <para>The first bit is always 0 for positive numbers, and 1 for negative numbers. The remaining bits are as described above.</para>
        /// </summary>
        /// <param name="input">The input integer.</param>
        public void WriteVarInt(long input)
        {
            if (input < 0)
            {
                input = -input;
                input <<= 1;
                input += 1;
            }
            else
            {
                input <<= 1;
            }
            int shifts = 0;
            long lim = 127;
            while (input > lim)
            {
                byte b = (byte)(((input & lim) >> shifts) | 128);
                WriteByte(b);
                shifts += 7;
                lim <<= 7;
            }
            byte lastB = (byte)(((input & lim) >> shifts));
            WriteByte(lastB);
        }
    }
}
