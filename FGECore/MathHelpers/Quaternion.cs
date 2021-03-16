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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using FGECore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;

namespace FGECore.MathHelpers
{
    /// <summary>
    /// Represents a 3D rotation, using 4 double-precision floating-point coordinates.
    /// Occupies 32 bytes, calculated as 8 * 4, as it has 4 fields (X, Y, Z, W) each occupying 8 bytes (a double).
    /// </summary>
    /// <remarks>Based upon BEPU utilities Quaternion.</remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct Quaternion : IEquatable<Quaternion>
    {
        /// <summary>
        /// The identity Quaternion: one with no rotation applied.
        /// </summary>
        public static readonly Quaternion Identity = new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// The X component of this Quaternion.
        /// </summary>
        [FieldOffset(0)]
        public double X;

        /// <summary>
        /// The Y component of this Quaternion.
        /// </summary>
        [FieldOffset(8)]
        public double Y;

        /// <summary>
        /// The Z component of this Quaternion.
        /// </summary>
        [FieldOffset(16)]
        public double Z;

        /// <summary>
        /// The W component of this Quaternion.
        /// </summary>
        [FieldOffset(24)]
        public double W;

        /// <summary>
        /// Returns X as a float.
        /// </summary>
        public float XF
        {
            get
            {
                return (float)X;
            }
        }

        /// <summary>
        /// Returns Y as a float.
        /// </summary>
        public float YF
        {
            get
            {
                return (float)Y;
            }
        }

        /// <summary>
        /// Returns Z as a float.
        /// </summary>
        public float ZF
        {
            get
            {
                return (float)Z;
            }
        }

        /// <summary>
        /// Returns W as a float.
        /// </summary>
        public float WF
        {
            get
            {
                return (float)W;
            }
        }

        /// <summary>
        /// Constructs a Quaternion with specific components.
        /// </summary>
        /// <param name="_x">X.</param>
        /// <param name="_y">Y.</param>
        /// <param name="_z">Z.</param>
        /// <param name="_w">W.</param>
        public Quaternion(double _x, double _y, double _z, double _w)
        {
            X = _x;
            Y = _y;
            Z = _z;
            W = _w;
        }

        /// <summary>
        /// Returns a quaternion plus another. Uses simple addition.
        /// DOES NOT "COMBINE" TWO QUATERNIONS. For that, use multiplication!
        /// </summary>
        /// <param name="b">The other.</param>
        /// <returns>The added result.</returns>
        public Quaternion Plus(in Quaternion b)
        {
            return new Quaternion(X + b.X, Y + b.Y, Z + b.Z, W + b.W);
        }

        /// <summary>
        /// Returns a Quaternion multiplied by another.
        /// Effectively combines two Quaternions.
        /// </summary>
        /// <param name="b">The other.</param>
        /// <returns>The multiplied result.</returns>
        public Quaternion MultipliedBy(in Quaternion b)
        {
            return new Quaternion(
                X * b.W + b.X * W + Y * b.Z - Z * b.Y,
                Y * b.W + b.Y * W + Z * b.X - X * b.Z,
                Z * b.W + b.Z * W + X * b.Y - Y * b.X,
                W * b.W - X * b.X - Y * b.Y - Z * b.Z
                );
        }

        /// <summary>
        /// Returns a quaternion scaled by a factor.
        /// DOES NOT INCREASE ANGLE.
        /// </summary>
        /// <param name="scale">The scaling factor.</param>
        /// <returns>The scaled quaternion.</returns>
        public Quaternion ScaledBy(double scale)
        {
            return new Quaternion(X * scale, Y * scale, Z * scale, W * scale);
        }

        /// <summary>
        /// Returns a normalized version of the Quaternion.
        /// Goes through a slow square root op.
        /// </summary>
        /// <returns>The normalized form.</returns>
        public Quaternion Normalized()
        {
            double len_inv = 1 / Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
            return new Quaternion(X * len_inv, Y * len_inv, Z * len_inv, W * len_inv);
        }

        /// <summary>
        /// Gets the squared length of this Quaternion.
        /// Squared for efficiency.
        /// </summary>
        /// <returns>The length.</returns>
        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z + W * W;
        }

        /// <summary>
        /// Gets the length of this Quaternion.
        /// Non-squared, and thus inefficient.
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        /// <summary>
        /// Returns the intermediate state between two Quaternions.
        /// </summary>
        /// <param name="b">The second Quaternion.</param>
        /// <param name="lerp">The lerp amount.</param>
        /// <returns>The interpolated quaternion.</returns>
        public Quaternion Interpolate(Quaternion b, double lerp)
        {
            double cosHalfAngle = X * b.X + Y * b.Y + Z * b.Z + W * b.W;
            if (cosHalfAngle < 0)
            {
                b = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
                cosHalfAngle = -cosHalfAngle;
            }
            double halfAngle = Math.Acos(cosHalfAngle);
            double sinHalfAngle = Math.Sqrt(1.0 - cosHalfAngle * cosHalfAngle);
            double fracA = Math.Sin(((1 - lerp) * halfAngle)) / sinHalfAngle;
            double fracB = Math.Sin(lerp * halfAngle) / sinHalfAngle;
            return new Quaternion(
                X * fracA + b.X * fracB,
                Y * fracA + b.Y * fracB,
                Z * fracA + b.Z * fracB,
                W * fracA + b.W * fracB
                );
        }

        /// <summary>
        /// Returns the conjugate of this Quaternion.
        /// </summary>
        /// <returns>The conjugate.</returns>
        public Quaternion Conjugate()
        {
            return new Quaternion(-X, -Y, -Z, W);
        }

        /// <summary>
        /// Returns the inverse of this Quaternion.
        /// </summary>
        /// <returns>The inverse.</returns>
        public Quaternion Inverse()
        {
            double len_sq = X * X + Y * Y + Z * Z + W * W;
            return new Quaternion(-X * len_sq, -Y * len_sq, -Z * len_sq, W * len_sq);
        }

        /// <summary>
        /// Returns the negative of this Quaternion.
        /// </summary>
        /// <returns>The negative.</returns>
        public Quaternion Negative()
        {
            return new Quaternion(-X, -Y, -Z, -W);
        }

        /// <summary>
        /// Returns whether the two Quaternions are equal.
        /// </summary>
        /// <param name="a">First Quaternion.</param>
        /// <param name="b">Second Quaternion.</param>
        /// <returns>Equality.</returns>
        public static bool operator ==(in Quaternion a, in Quaternion b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        }

        /// <summary>
        /// Returns whether the two Quaternions are NOT equal.
        /// </summary>
        /// <param name="a">First Quaternion.</param>
        /// <param name="b">Second Quaternion.</param>
        /// <returns>Non-equality.</returns>
        public static bool operator !=(in Quaternion a, in Quaternion b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
        }

        /// <summary>
        /// Returns the Quaternion multiplied by another.
        /// </summary>
        /// <param name="a">First Quaternion.</param>
        /// <param name="b">Second Quaternion.</param>
        /// <returns>Multiplication result.</returns>
        public static Quaternion operator *(in Quaternion a, in Quaternion b)
        {
            return a.MultipliedBy(b);
        }

        /// <summary>
        /// Returns a hash code for this Quaternion.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }

        /// <summary>
        /// Returns whether this Quaternion equals another object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Equality.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Quaternion q)
            {
                return this == q;
            }
            return false;
        }

        /// <summary>
        /// Returns whether this Quaternion equals another Quaternion.
        /// </summary>
        /// <param name="b">The other Quaternion.</param>
        /// <returns>Equality.</returns>
        public bool Equals(Quaternion b)
        {
            return this == b;
        }

        /// <summary>
        /// Returns a simple string of this Quaternion.
        /// </summary>
        /// <returns>The simple string.</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ", " + W + ")";
        }

        /// <summary>
        /// Creates a Quaternion from an axis and an angle around the axis.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="angle">The angle (in radians).</param>
        /// <returns>The resultant Quaternion.</returns>
        public static Quaternion FromAxisAngle(in Location axis, double angle)
        {
            double s = Math.Sin(angle * 0.5);
            return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, Math.Cos(angle * 0.5));
        }

        /// <summary>
        /// Gets the quaternion between two normalized vector locations.
        /// </summary>
        /// <param name="v1">First vector location.</param>
        /// <param name="v2">Second vector location.</param>
        public static Quaternion GetQuaternionBetween(in Location v1, in Location v2)
        {
            double dot = v1.Dot(v2);
            if (dot < -0.9999f)
            {
                double absX = Math.Abs(v1.X);
                double absY = Math.Abs(v1.Y);
                double absZ = Math.Abs(v1.Z);
                if (absX < absY && absX < absZ)
                {
                    return new Quaternion(0, -v1.Z, v1.Y, 0).Normalized();
                }
                else if (absY < absZ)
                {
                    return new Quaternion(-v1.Z, 0, v1.X, 0).Normalized();
                }
                else
                {
                    return new Quaternion(-v1.Y, v1.X, 0, 0).Normalized();
                }
            }
            else
            {
                Location axis = v1.CrossProduct(v2);
                return new Quaternion(axis.X, axis.Y, axis.Z, dot + 1).Normalized();
            }
        }

        /// <summary>
        /// Gets the quaternion between two quaternions.
        /// Equivalent to: <code>a.MultipliedBy(b.Conjugate())</code>
        /// Returns a result such that: <code>b == a.MultipliedBy(result)</code>
        /// </summary>
        /// <param name="a">First quaternion.</param>
        /// <param name="b">Second quaternion.</param>
        public static Quaternion GetQuaternionBetween(in Quaternion a, in Quaternion b)
        {
            return a.MultipliedBy(b.Conjugate());
        }

        /// <summary>
        /// Get the angle around an axis for a specific quaternion, in radians.
        /// </summary>
        /// <param name="axis">The relative axis.</param>
        public double AxisAngleForRadians(in Location axis)
        {
            Location ra = new Location(X, Y, Z);
            Location p = ra.Project(axis);
            Quaternion twist = new Quaternion(p.X, p.Y, p.Z, W).Normalized();
            Location new_forward = twist.TransformX();
            return MathUtilities.VectorToAnglesYawRad(new_forward);
        }

        /// <summary>
        /// Reads the byte array to a Quaternion object.
        /// Expects 16 bytes.
        /// Inverts <see cref="ToFloatBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="bytes">The bytes to read.</param>
        /// <param name="index">The index to start at.</param>
        /// <returns>the location object.</returns>
        public static Quaternion FromFloatBytes(byte[] bytes, int index)
        {
            return new Quaternion(
                PrimitiveConversionHelper.BytesToFloat32(bytes, index),
                PrimitiveConversionHelper.BytesToFloat32(bytes, index + 4),
                PrimitiveConversionHelper.BytesToFloat32(bytes, index + (4 + 4)),
                PrimitiveConversionHelper.BytesToFloat32(bytes, index + (4 + 4 + 4))
                );
        }

        /// <summary>
        /// Copies the quaterion to a byte array.
        /// Contains 16 bytes.
        /// Inverts <see cref="FromFloatBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the output array.</param>
        public void ToFloatBytes(byte[] outputBytes, int offset)
        {
            PrimitiveConversionHelper.Float32ToBytes((float)X, outputBytes, offset);
            PrimitiveConversionHelper.Float32ToBytes((float)Y, outputBytes, offset + 4);
            PrimitiveConversionHelper.Float32ToBytes((float)Z, outputBytes, offset + (4 + 4));
            PrimitiveConversionHelper.Float32ToBytes((float)W, outputBytes, offset + (4 + 4 + 4));
        }

        /// <summary>
        /// Reads the byte array to a Quaternion object.
        /// Expects 32 bytes.
        /// Inverts <see cref="ToDoubleBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="bytes">The bytes to read.</param>
        /// <param name="index">The index to start at.</param>
        /// <returns>the location object.</returns>
        public static Quaternion FromDoubleBytes(byte[] bytes, int index)
        {
            return new Quaternion(
                PrimitiveConversionHelper.BytesToDouble64(bytes, index),
                PrimitiveConversionHelper.BytesToDouble64(bytes, index + 8),
                PrimitiveConversionHelper.BytesToDouble64(bytes, index + (8 + 8)),
                PrimitiveConversionHelper.BytesToDouble64(bytes, index + (8 + 8 + 8))
                );
        }

        /// <summary>
        /// Copies the quaterion to a byte array.
        /// Contains 32 bytes.
        /// Inverts <see cref="FromDoubleBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the output array.</param>
        public void ToDoubleBytes(byte[] outputBytes, int offset)
        {
            PrimitiveConversionHelper.Double64ToBytes(X, outputBytes, offset);
            PrimitiveConversionHelper.Double64ToBytes(Y, outputBytes, offset + 8);
            PrimitiveConversionHelper.Double64ToBytes(Z, outputBytes, offset + (8 + 8));
            PrimitiveConversionHelper.Double64ToBytes(W, outputBytes, offset + (8 + 8 + 8));
        }

        /// <summary>
        /// Converts the Quaternion to a simple byte[] representation.
        /// Contains 32 bytes.
        /// Inverts <see cref="FromDoubleBytes(byte[], int)"/>.
        /// </summary>
        /// <returns>The byte array.</returns>
        public byte[] ToDoubleBytes()
        {
            byte[] toret = new byte[32];
            ToDoubleBytes(toret, 0);
            return toret;
        }

        /// <summary>
        /// Transforms a location vector by this Quaternion and returns the result.
        /// </summary>
        /// <param name="v">The location vector.</param>
        /// <returns>The transformed location vector.</returns>
        public Location Transform(in Location v)
        {
            double x2 = X * 2;
            double y2 = Y * 2;
            double z2 = Z * 2;
            double xx2 = X * x2;
            double xy2 = X * y2;
            double xz2 = X * z2;
            double yy2 = Y * y2;
            double yz2 = Y * z2;
            double zz2 = Z * z2;
            double wx2 = W * x2;
            double wy2 = W * y2;
            double wz2 = W * z2;
            return new Location(v.X * (1f - yy2 - zz2) + v.Y * (xy2 - wz2) + v.Z * (xz2 + wy2),
                v.X * (xy2 + wz2) + v.Y * (1f - xx2 - zz2) + v.Z * (yz2 - wx2),
                v.X * (xz2 - wy2) + v.Y * (yz2 + wx2) + v.Z * (1f - xx2 - yy2));
        }

        /// <summary>
        /// Transform the normal X axis by this Quaternion.
        /// </summary>
        /// <returns>The transformed axis.</returns>
        public Location TransformX()
        {
            double y2 = Y * 2;
            double z2 = Z * 2;
            return new Location(1.0 - (Y * y2) - (Z * z2), (X * y2) + (W * z2), (X * z2) - (W * y2));
        }

        /// <summary>
        /// Gets or sets this Quaternion as a 2D angle.
        /// </summary>
        public double Angle2D
        {
            get
            {
                return AxisAngleForRadians(Location.UnitZ);
            }
            set
            {
                X = 0;
                Y = 0;
                Z = Math.Sin(value * 0.5);
                W = Math.Cos(value * 0.5);
            }
        }

        /// <summary>Gets the angle represented by the Quaternion.</summary>
        public double RepresentedAngle()
        {
            double wAbs = Math.Abs(W);
            if (wAbs > 1)
            {
                return 0;
            }
            return 2 * Math.Acos(wAbs);
        }

        /// <summary>Gets the axis of rotation represented by the Quaternion.</summary>
        public Location RepresentedAxis()
        {
            return (new Location(X, Y, Z) * Math.Sign(W)).Normalize();
        }

        /// <summary>Converts the <see cref="Quaternion"/> value to a floating point <see cref="System.Numerics.Quaternion"/>.</summary>
        public System.Numerics.Quaternion ToNumerics()
        {
            return new System.Numerics.Quaternion(XF, YF, ZF, WF);
        }

        /// <summary>Returns the Spherical Linear Interpolation ('slerp') between this quaternion and another.</summary>
        public Quaternion Slerp(Quaternion end, double interpolationAmount)
        {
            double cosHalfTheta = W * end.W + X * end.X + Y * end.Y + Z * end.Z;
            if (cosHalfTheta < 0)
            {
                end = end.Negative();
                cosHalfTheta = -cosHalfTheta;
            }
            if (cosHalfTheta > (1.0 - 1e-12))
            {
                return this;
            }
            double halfTheta = Math.Acos(cosHalfTheta);
            double sinHalfTheta = Math.Sqrt(1.0 - cosHalfTheta * cosHalfTheta);
            double aFraction = Math.Sin((1 - interpolationAmount) * halfTheta) / sinHalfTheta;
            double bFraction = Math.Sin(interpolationAmount * halfTheta) / sinHalfTheta;
            return new Quaternion(X * aFraction + end.X * bFraction, Y * aFraction + end.Y * bFraction,
                Z * aFraction + end.Z * bFraction, W * aFraction + end.W * bFraction);
        }
    }

    /// <summary>Helper extensions for <see cref="Quaternion"/>.</summary>
    public static class ExtensionsForQuaternion
    {
        /// <summary>Converts a floating point <see cref="System.Numerics.Quaternion"/> to a <see cref="Quaternion"/>.</summary>
        public static Quaternion ToCore(this System.Numerics.Quaternion quat)
        {
            return new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }
    }
}
