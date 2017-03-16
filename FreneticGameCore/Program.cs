using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    public abstract class Program
    {
        public static Program Instance;

        public readonly string Name;

        public readonly string Version;

        public static string GameName
        {
            get
            {
                return Instance.Name;
            }
        }

        public static string GameVersion
        {
            get
            {
                return Instance.Version;
            }
        }
    }
}
