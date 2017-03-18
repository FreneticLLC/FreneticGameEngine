using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FreneticGameCore.Files
{
    /// <summary>
    /// Wraps a System.IO.MemoryStream.
    /// </summary>
    public class DataStream : MemoryStream
    {
        /// <summary>
        /// Constructs a data stream with bytes pre-loaded.
        /// </summary>
        /// <param name="bytes">The bytes to pre-load.</param>
        public DataStream(byte[] bytes)
            : base(bytes)
        {
        }

        /// <summary>
        /// Constructs an empty data stream.
        /// </summary>
        public DataStream()
            : base()
        {
        }

        public DataStream(int capacity)
            : base(capacity)
        {
        }
    }
}
