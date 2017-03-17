using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents the current program abstractly.
    /// </summary>
    public abstract class Program
    {
        /// <summary>
        /// The current program instance.
        /// </summary>
        public static Program Instance;

        /// <summary>
        /// The name of this program.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The version of this program.
        /// </summary>
        public readonly string Version;

        /// <summary>
        /// The current program's game name.
        /// </summary>
        public static string GameName
        {
            get
            {
                return Instance.Name;
            }
        }

        /// <summary>
        /// The current program's game version.
        /// </summary>
        public static string GameVersion
        {
            get
            {
                return Instance.Version;
            }
        }
    }
}
