using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;
using FreneticGameCore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;
using FreneticGameCore.MathHelpers;

namespace FreneticGameCore.PhysicsSystem
{
    /// <summary>
    /// Utilities related to BEPU physics.
    /// </summary>
    public static class BepuUtilities
    {
        /// <summary>
        /// Converts a quaternion to a byte array.
        /// 16 bytes.
        /// </summary>
        /// <param name="quat">The quaternion.</param>
        /// <returns>The byte array.</returns>
        public static byte[] QuaternionToBytes(BEPUutilities.Quaternion quat)
        {
            byte[] dat = new byte[4 + 4 + 4 + 4];
            QuaternionToBytes(quat, dat, 0);
            return dat;
        }

        /// <summary>
        /// Converts a quaternion to a byte array.
        /// 16 bytes.
        /// </summary>
        /// <param name="quat">The quaternion.</param>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the output bytes.</param>
        /// <returns>The byte array.</returns>
        public static void QuaternionToBytes(BEPUutilities.Quaternion quat, byte[] outputBytes, int offset)
        {
            PrimitiveConversionHelper.Float32ToBytes((float)quat.X, outputBytes, offset);
            PrimitiveConversionHelper.Float32ToBytes((float)quat.Y, outputBytes, offset + 4);
            PrimitiveConversionHelper.Float32ToBytes((float)quat.Z, outputBytes, offset + (4 + 4));
            PrimitiveConversionHelper.Float32ToBytes((float)quat.W, outputBytes, offset + (4 + 4 + 4));
        }

        /// <summary>
        /// Converts a byte array to a quaternion.
        /// </summary>
        /// <param name="dat">The byte array.</param>
        /// <param name="offset">The offset in the array.</param>
        /// <returns>The quaternion.</returns>
        public static BEPUutilities.Quaternion BytesToQuaternion(byte[] dat, int offset)
        {
            return new BEPUutilities.Quaternion(
                PrimitiveConversionHelper.BytesToFloat32(dat, offset),
                PrimitiveConversionHelper.BytesToFloat32(dat, offset + 4),
                PrimitiveConversionHelper.BytesToFloat32(dat, offset + (4 + 4)),
                PrimitiveConversionHelper.BytesToFloat32(dat, offset + (4 + 4 + 4))
                );
        }

        /// <summary>
        /// Creates a Matrix that "looks at" a target from a location, left-hand notation.
        /// </summary>
        /// <param name="start">The starting coordinate.</param>
        /// <param name="end">The end target.</param>
        /// <param name="up">The normalized up vector.</param>
        /// <returns>A matrix.</returns>
        public static Matrix LookAtLH(Location start, Location end, Location up)
        {
            Location zAxis = (end - start).Normalize();
            Location xAxis = up.CrossProduct(zAxis).Normalize();
            Location yAxis = zAxis.CrossProduct(xAxis);
            return new Matrix(xAxis.X, yAxis.X, zAxis.X, 0, xAxis.Y,
                yAxis.Y, zAxis.Y, 0, xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -xAxis.Dot(start), -yAxis.Dot(start), -zAxis.Dot(start), 1);
        }

        /// <summary>
        /// Converts a matrix to Euler angles.
        /// </summary>
        /// <param name="WorldTransform">The matrix.</param>
        /// <returns>The Euler angles.</returns>
        public static Location MatrixToAngles(Matrix WorldTransform)
        {
            Location rot;
            rot.X = Math.Atan2(WorldTransform.M32, WorldTransform.M33) * 180 / Math.PI;
            rot.Y = -Math.Asin(WorldTransform.M31) * 180 / Math.PI;
            rot.Z = Math.Atan2(WorldTransform.M21, WorldTransform.M11) * 180 / Math.PI;
            return rot;
        }

        /// <summary>
        /// Converts Euler angles to a matrix.
        /// </summary>
        /// <param name="rot">The Euler angles.</param>
        /// <returns>The matrix.</returns>
        public static Matrix AnglesToMatrix(Location rot)
        {
            // TODO: better method?
            return Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), (rot.X * Utilities.PI180))
                    * Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (rot.Y * Utilities.PI180))
                    * Matrix.CreateFromAxisAngle(new Vector3(0, 0, 1), (rot.Z * Utilities.PI180));
        }

        /// <summary>
        /// Projects a vector onto another.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The projected vector.</returns>
        public static Vector3 Project(Vector3 a, Vector3 b)
        {
            return b * (Vector3.Dot(a, b) / b.LengthSquared());
        }
    }
}
