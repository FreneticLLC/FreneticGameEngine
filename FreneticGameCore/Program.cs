using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

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
        /// The original system culture.
        /// </summary>
        public static CultureInfo SystemCulture;

        /// <summary>
        /// This method should be called FIRST!
        /// Enforces the correct (Invariant) culture locale setting!
        /// </summary>
        public static void PreInit(Program p)
        {
            Instance = p;
            SystemCulture = CultureInfo.DefaultThreadCurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
        }

        /// <summary>
        /// The name of this program.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The version (number) of this program.
        /// </summary>
        public readonly string Version;

        /// <summary>
        /// The version description of this program.
        /// </summary>
        public readonly string VersionDescription;

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

        /// <summary>
        /// The current program's game version description.
        /// (EG "Release", "Beta", or "Alpha" usually).
        /// </summary>
        public static string GameVersionDescription
        {
            get
            {
                return Instance.VersionDescription;
            }
        }

        /// <summary>
        /// Construct the program descriptor.
        /// </summary>
        /// <param name="_name">Game name.</param>
        /// <param name="_version">Game version.</param>
        /// <param name="_versdesc">Game version descriptor.</param>
        public Program(string _name, string _version, string _versdesc)
        {
            Name = _name;
            Version = _version;
            VersionDescription = _versdesc;
        }
    }
}
