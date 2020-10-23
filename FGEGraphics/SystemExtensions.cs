//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;

namespace FGEGraphics
{
    /// <summary>
    /// Helpers for various external classes.
    /// </summary>
    public static class SystemExtensions
    {
        /// <summary>
        /// Converts a Core <see cref="FGECore.MathHelpers.Quaternion"/> to an OpenTK <see cref="OpenTK.Quaternion"/>.
        /// </summary>
        /// <param name="quat">The Core <see cref="FGECore.MathHelpers.Quaternion"/>.</param>
        /// <returns>The OpenTK <see cref="OpenTK.Quaternion"/>.</returns>
        public static OpenTK.Quaternion ToOpenTK(this FGECore.MathHelpers.Quaternion quat)
        {
            return new OpenTK.Quaternion((float)quat.X, (float)quat.Y, (float)quat.Z, (float)quat.W);
        }

        /// <summary>
        /// Converts a Core <see cref="FGECore.MathHelpers.Quaternion"/> to an OpenTK <see cref="Quaterniond"/>.
        /// </summary>
        /// <param name="quat">The Core <see cref="FGECore.MathHelpers.Quaternion"/>.</param>
        /// <returns>The OpenTK <see cref="Quaterniond"/>.</returns>
        public static Quaterniond ToOpenTKDoubles(this FGECore.MathHelpers.Quaternion quat)
        {
            return new Quaterniond(quat.X, quat.Y, quat.Z, quat.W);
        }

        /// <summary>
        /// Converts a BEPU <see cref="BEPUutilities.Quaternion"/> to an OpenTK <see cref="OpenTK.Quaternion"/>.
        /// </summary>
        /// <param name="quat">The BEPU <see cref="BEPUutilities.Quaternion"/>.</param>
        /// <returns>The OpenTK <see cref="OpenTK.Quaternion"/>.</returns>
        public static OpenTK.Quaternion ToOpenTK(this BEPUutilities.Quaternion quat)
        {
            return new OpenTK.Quaternion((float)quat.X, (float)quat.Y, (float)quat.Z, (float)quat.W);
        }

        /// <summary>
        /// Converts an OpenTK <see cref="OpenTK.Quaternion"/> to a Core <see cref="FGECore.MathHelpers.Quaternion"/>.
        /// </summary>
        /// <param name="quat">The OpenTK <see cref="OpenTK.Quaternion"/>.</param>
        /// <returns>The Core <see cref="FGECore.MathHelpers.Quaternion"/>.</returns>
        public static FGECore.MathHelpers.Quaternion ToCore(this OpenTK.Quaternion quat)
        {
            return new FGECore.MathHelpers.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        /// <summary>
        /// Converts an OpenTK <see cref="OpenTK.Quaternion"/> to a BEPU <see cref="BEPUutilities.Quaternion"/>.
        /// </summary>
        /// <param name="quat">The OpenTK <see cref="OpenTK.Quaternion"/>.</param>
        /// <returns>The BEPU <see cref="BEPUutilities.Quaternion"/>.</returns>
        public static BEPUutilities.Quaternion ToBEPU(this OpenTK.Quaternion quat)
        {
            return new BEPUutilities.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        /// <summary>
        /// Converts an OpenTK <see cref="OpenTK.Quaternion"/> to an OpenTK <see cref="Quaterniond"/>.
        /// </summary>
        /// <param name="quat">The OpenTK <see cref="OpenTK.Quaternion"/>.</param>
        /// <returns>The OpenTK <see cref="Quaterniond"/>.</returns>
        public static Quaterniond ToDoubles(this OpenTK.Quaternion quat)
        {
            return new Quaterniond(quat.X, quat.Y, quat.Z, quat.W);
        }

        /// <summary>
        /// Converts an OpenTK <see cref="Vector3"/> to an OpenTK <see cref="Vector3d"/>.
        /// </summary>
        /// <param name="vec">The OpenTK <see cref="Vector3"/>.</param>
        /// <returns>The OpenTK <see cref="Vector3d"/>.</returns>
        public static Vector3d ToDoubles(this Vector3 vec)
        {
            return new Vector3d(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// Converts a <see cref="Location"/> to an OpenTK <see cref="Vector3d"/>.
        /// </summary>
        /// <param name="loc">The <see cref="Location"/>.</param>
        /// <returns>The OpenTK <see cref="Vector3d"/>.</returns>
        public static Vector3d ToOpenTK3D(this Location loc)
        {
            return new Vector3d(loc.X, loc.Y, loc.Z);
        }

        /// <summary>
        /// Converts a <see cref="Color3F"/> to an OpenTK <see cref="Vector3"/> as R,G,B.
        /// </summary>
        /// <param name="color">The <see cref="Color3F"/>.</param>
        /// <returns>The OpenTK <see cref="Vector3"/>.</returns>
        public static Vector3 ToOpenTK(this Color3F color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts a <see cref="Color3F"/> to an OpenTK <see cref="Vector3"/> as R,G,B,A.
        /// </summary>
        /// <param name="color">The <see cref="Color4F"/>.</param>
        /// <returns>The OpenTK <see cref="Vector4"/> as R,G,B,A.</returns>
        public static Vector4 ToOpenTK(this Color4F color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Converts a BEPU <see cref="BEPUutilities.Vector3"/> to an OpenTK <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vec">The BEPU <see cref="BEPUutilities.Vector3"/>.</param>
        /// <returns>The OpenTK <see cref="Vector3"/>.</returns>
        public static Vector3 ToOpenTK(this BEPUutilities.Vector3 vec)
        {
            return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
        }

        /// <summary>
        /// Converts a <see cref="Location"/> to an OpenTK <see cref="Vector3"/>.
        /// </summary>
        /// <param name="loc">The <see cref="Location"/>.</param>
        /// <returns>The OpenTK <see cref="Vector3"/>.</returns>
        public static Vector3 ToOpenTK(this Location loc)
        {
            return new Vector3((float)loc.X, (float)loc.Y, (float)loc.Z);
        }

        /// <summary>
        /// Converts an OpenTK Vector3D to a <see cref="Location"/>.
        /// </summary>
        /// <param name="vec">The OpenTK Vector3D.</param>
        /// <returns>The <see cref="Location"/>.</returns>
        public static Location ToLocation(this Vector3d vec)
        {
            return new Location(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// Converts an OpenTK <see cref="Vector3"/> to a <see cref="Location"/>.
        /// </summary>
        /// <param name="loc">The OpenTK <see cref="Vector3"/>.</param>
        /// <returns>The <see cref="Location"/>.</returns>
        public static Location ToLocation(this Vector3 loc)
        {
            return new Location(loc.X, loc.Y, loc.Z);
        }

        /// <summary>
        /// Converts a BEPUPhysics <see cref="BEPUutilities.Vector3"/> to a <see cref="Location"/>.
        /// </summary>
        /// <param name="loc">The BEPUPhysics <see cref="BEPUutilities.Vector3"/>.</param>
        /// <returns>The <see cref="Location"/>.</returns>
        public static Location ToLocation(this BEPUutilities.Vector3 loc)
        {
            return new Location(loc.X, loc.Y, loc.Z);
        }

        /// <summary>
        /// Converts a <see cref="Location"/> to a BEPUPhysics <see cref="BEPUutilities.Vector3"/>.
        /// </summary>
        /// <param name="loc">The <see cref="Location"/>.</param>
        /// <returns>The BEPUPhysics <see cref="BEPUutilities.Vector3"/>.</returns>
        public static BEPUutilities.Vector3 ToBEPU(this Location loc)
        {
            return new BEPUutilities.Vector3(loc.X, loc.Y, loc.Z);
        }

        /// <summary>
        /// Converts a OpenTK <see cref="Matrix4"/> to an OpenTK <see cref="Matrix4d"/>.
        /// </summary>
        /// <param name="mat">The input <see cref="Matrix4"/>.</param>
        /// <returns>The output <see cref="Matrix4d"/>.</returns>
        public static Matrix4d ConvertToD(this Matrix4 mat)
        {
            return new Matrix4d(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        /// <summary>
        /// Converts a BEPU <see cref="BEPUutilities.Matrix"/> to an OpenTK <see cref="Matrix4"/>.
        /// </summary>
        /// <param name="mat">The input <see cref="BEPUutilities.Matrix"/>.</param>
        /// <returns>The output <see cref="Matrix4"/>.</returns>
        public static Matrix4 Convert(this BEPUutilities.Matrix mat)
        {
            return new Matrix4((float)mat.M11, (float)mat.M12, (float)mat.M13, (float)mat.M14, (float)mat.M21, (float)mat.M22, (float)mat.M23,
               (float)mat.M24, (float)mat.M31, (float)mat.M32, (float)mat.M33, (float)mat.M34, (float)mat.M41, (float)mat.M42, (float)mat.M43, (float)mat.M44);
        }

        /// <summary>
        /// Converts a BEPU <see cref="BEPUutilities.Matrix"/> to an OpenTK <see cref="Matrix4d"/>.
        /// </summary>
        /// <param name="mat">The input <see cref="BEPUutilities.Matrix"/>.</param>
        /// <returns>The output <see cref="Matrix4d"/>.</returns>
        public static Matrix4d ConvertD(this BEPUutilities.Matrix mat)
        {
            return new Matrix4d(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        /// <summary>
        /// Converts an OpenTK <see cref="Matrix4d"/> to a BEPU <see cref="BEPUutilities.Matrix"/>.
        /// </summary>
        /// <param name="mat">The input <see cref="Matrix4d"/>.</param>
        /// <returns>The output <see cref="BEPUutilities.Matrix"/>.</returns>
        public static BEPUutilities.Matrix ConvertD(this Matrix4d mat)
        {
            return new BEPUutilities.Matrix(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        /// <summary>
        /// Converts an OpenTK <see cref="Matrix4"/> to a BEPU <see cref="BEPUutilities.Matrix"/>.
        /// </summary>
        /// <param name="mat">The input <see cref="Matrix4"/>.</param>
        /// <returns>The output <see cref="BEPUutilities.Matrix"/>.</returns>
        public static BEPUutilities.Matrix ConvertD(this Matrix4 mat)
        {
            return new BEPUutilities.Matrix(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }
    }
}
