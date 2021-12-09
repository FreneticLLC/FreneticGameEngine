//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LZ4;

namespace FGECore.FileSystems
{
    /// <summary>Utilities related to file handling.</summary>
    public static class FileUtilities
    {
        /// <summary>Compresses a byte array using LZ4.</summary>
        /// <param name="input">Non-compressed data.</param>
        /// <returns>Compressed data.</returns>
        public static byte[] Compress(byte[] input)
        {
            using MemoryStream memstream = new();
            using LZ4Stream lzstream = new(memstream, LZ4StreamMode.Compress);
            lzstream.Write(input, 0, input.Length);
            lzstream.Flush();
            return memstream.ToArray();
        }
        /// <summary>Compresses a byte array using LZ4.</summary>
        /// <param name="input">Non-compressed data.</param>
        /// <param name="start">Starting index.</param>
        /// <param name="length">Length to compress.</param>
        /// <returns>Compressed data.</returns>
        public static byte[] CompressPartial(byte[] input, int start, int length)
        {
            using MemoryStream memstream = new();
            using LZ4Stream lzstream = new(memstream, LZ4StreamMode.Compress);
            lzstream.Write(input, start, length);
            lzstream.Flush();
            return memstream.ToArray();
        }

        /// <summary>Decompresses a byte array using LZ4.</summary>
        /// <param name="input">Compressed data.</param>
        /// <returns>Non-compressed data.</returns>
        public static byte[] Decompress(byte[] input)
        {
            using MemoryStream output = new();
            using MemoryStream memstream = new(input);
            using LZ4Stream LZStream = new(memstream, LZ4StreamMode.Decompress);
            LZStream.CopyTo(output);
            LZStream.Flush();
            return output.ToArray();
        }

        /// <summary>Compresses a byte array using the GZip algorithm.</summary>
        /// <param name="input">Non-compressed data.</param>
        /// <returns>Compressed data.</returns>
        public static byte[] GZip(byte[] input)
        {
            using MemoryStream memstream = new();
            using GZipStream GZStream = new(memstream, CompressionMode.Compress);
            GZStream.Write(input, 0, input.Length);
            GZStream.Flush();
            return memstream.ToArray();
        }

        /// <summary>Decompress a byte array using the GZip algorithm.</summary>
        /// <param name="input">Compressed data.</param>
        /// <returns>Non-compressed data.</returns>
        public static byte[] UnGZip(byte[] input)
        {
            using MemoryStream output = new();
            using MemoryStream memstream = new(input);
            using GZipStream GZStream = new(memstream, CompressionMode.Decompress);
            GZStream.CopyTo(output);
            GZStream.Flush();
            return output.ToArray();
        }
    }
}
