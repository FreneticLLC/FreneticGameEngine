using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameGraphics
{
    /// <summary>
    /// Helper class for graphical systems.
    /// </summary>
    public static class GraphicsUtil
    {
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
