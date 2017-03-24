using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics
{
    /// <summary>
    /// Helper class for graphical systems.
    /// </summary>
    public static class GraphicsUtil
    {
        /// <summary>
        /// Converts a Location to a Vector3.
        /// </summary>
        /// <param name="inp">The location.</param>
        /// <returns>The Vector3.</returns>
        public static Vector3 Convert(Location inp)
        {
            return new Vector3((float)inp.X, (float)inp.Y, (float)inp.Z);
        }

        /// <summary>
        /// Checks errors when debug is enabled.
        /// </summary>
        /// <param name="loc">The source calling location.</param>
        public static void CheckError(string loc)
        {
#if DEBUG
            ErrorCode ec = GL.GetError();
            while (ec != ErrorCode.NoError)
            {
                SysConsole.Output(OutputType.ERROR, "OpenGL error [" + loc + "]: " + ec + "\n" + Environment.StackTrace);
                ec = GL.GetError();
            }
#endif
        }

    }
}
