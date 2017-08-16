using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents shader target locations.
    /// </summary>
    public static class ShaderLocations
    {
        /// <summary>
        /// Locations shared by most shaders.
        /// </summary>
        public static class Common
        {
            /// <summary>
            /// The general screen projection and view together.
            /// </summary>
            public const int PROJECTION = 1;

            /// <summary>
            /// The general world offset.
            /// </summary>
            public const int WORLD = 2;
        }
    }
}
