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
        /// Converts a BEPU Matrix4 to an OpenTK Matrix4 (Doubles).
        /// </summary>
        /// <param name="mat">The input matrix.</param>
        /// <returns>The output matrix.</returns>
        public static Matrix4d ConvertD(BEPUutilities.Matrix mat)
        {
            return new Matrix4d(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        /// <summary>
        /// Converts an OpenTK Matrix4 to a BEPU Matrix4 (Doubles).
        /// </summary>
        /// <param name="mat">The input matrix.</param>
        /// <returns>The output matrix.</returns>
        public static BEPUutilities.Matrix ConvertD(Matrix4d mat)
        {
            return new BEPUutilities.Matrix(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
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
