using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents the context in which something is being rendered (in 3D).
    /// </summary>
    public class RenderContext
    {
        /// <summary>
        /// The relevant owning game engine (3D).
        /// </summary>
        public GameEngine3D Engine;
    }
}
