//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics.GraphicsHelpers
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
#if !DEBUG
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
