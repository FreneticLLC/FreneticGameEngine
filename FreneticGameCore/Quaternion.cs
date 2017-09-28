using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using FreneticGameCore.Collision;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a 3D rotation, using 4 double-precision floating-point coordinates.
    /// Occupies 32 bytes, calculated as 8 * 4, as it has 4 fields (X, Y, Z, W) each occupying 8 bytes (a double).
    /// </summary>
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
        public Quaternion Plus(Quaternion b)
        {
            return new Quaternion(X + b.X, Y + b.Y, Z + b.Z, W + b.W);
        }

        /// <summary>
        /// Returns a Quaternion multiplied by another.
        /// Effectively combines two Quaternions.
        /// </summary>
        /// <param name="b">The other.</param>
        /// <returns>The multiplied result.</returns>
        public Quaternion MultipliedBy(Quaternion b)
        {
            return new Quaternion(
                X * b.W + b.X * W + Y * b.Z - Z * b.Y,
                Y * b.W + b.Y * W + Z * b.X - X * b.Z,
                Z * b.W + b.Z + W + X * b.Y - Y * b.X,
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
        public static bool operator==(Quaternion a, Quaternion b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        }

        /// <summary>
        /// Returns whether the two Quaternions are NOT equal.
        /// </summary>
        /// <param name="a">First Quaternion.</param>
        /// <param name="b">Second Quaternion.</param>
        /// <returns>Non-equality.</returns>
        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
        }

        /// <summary>
        /// Returns the Quaternion multiplied by another.
        /// </summary>
        /// <param name="a">First Quaternion.</param>
        /// <param name="b">Second Quaternion.</param>
        /// <returns>Multiplication result.</returns>
        public static Quaternion operator *(Quaternion a, Quaternion b)
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
    }
}
