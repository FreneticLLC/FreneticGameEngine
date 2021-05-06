//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.UtilitySystems;

namespace FGECore.MathHelpers
{
    /// <summary>
    /// Represents a 3D location, using 3 double-precision floating-point coordinates.
    /// <para>Occupies 24 bytes, calculated as 8 * 3, as it has 3 fields (X, Y, Z) each occupying 8 bytes (a double).</para>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Location : IEquatable<Location>
    {
        #region static reference fields
        /// <summary>
        /// A Location of (0, 0, 0).
        /// </summary>
        public static readonly Location Zero = new Location(0);

        /// <summary>
        /// A Location of (1, 1, 1).
        /// </summary>
        public static readonly Location One = new Location(1);

        /// <summary>
        /// A location of (1, 0, 0).
        /// </summary>
        public static readonly Location UnitX = new Location(1, 0, 0);

        /// <summary>
        /// A location of (0, 1, 0).
        /// </summary>
        public static readonly Location UnitY = new Location(0, 1, 0);

        /// <summary>
        /// A location of (0, 0, 1).
        /// </summary>
        public static readonly Location UnitZ = new Location(0, 0, 1);

        /// <summary>
        /// A location of (NaN, NaN, NaN).
        /// </summary>
        public static readonly Location NaN = new Location(double.NaN, double.NaN, double.NaN);

        /// <summary>
        /// A location of (Infinity, Infinity, Infinity).
        /// </summary>
        public static readonly Location Infinity = new Location(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        #endregion

        #region fields
        /// <summary>
        /// The X coordinate of this location.
        /// </summary>
        [FieldOffset(0)]
        public double X;

        /// <summary>
        /// The Y coordinate of this location.
        /// </summary>
        [FieldOffset(8)]
        public double Y;

        /// <summary>
        /// The Z coordinate of this location.
        /// </summary>
        [FieldOffset(16)]
        public double Z;
        #endregion

        #region coordinate accessors
        /// <summary>
        /// Returns <see cref="X"/> as a <see cref="float"/>.
        /// </summary>
        public float XF
        {
            get
            {
                return (float)X;
            }
        }

        /// <summary>
        /// Returns <see cref="Y"/> as a <see cref="float"/>.
        /// </summary>
        public float YF
        {
            get
            {
                return (float)Y;
            }
        }

        /// <summary>
        /// Returns <see cref="Z"/> as a <see cref="float"/>.
        /// </summary>
        public float ZF
        {
            get
            {
                return (float)Z;
            }
        }

        /// <summary>
        /// If this location is an angle set, returns the Yaw value of this angle set.
        /// Equivalent to <see cref="Z"/>.
        /// </summary>
        public double Yaw
        {
            get
            {
                return Z;
            }
            set
            {
                Z = value;
            }
        }

        /// <summary>
        /// If this location is an angle set, returns the Pitch value of this angle set.
        /// Equivalent to <see cref="Y"/>.
        /// </summary>
        public double Pitch
        {
            get
            {
                return Y;
            }
            set
            {
                Y = value;
            }
        }

        /// <summary>
        /// If this location is an angle set, returns the Roll value of this angle set.
        /// Equivalent to <see cref="X"/>.
        /// </summary>
        public double Roll
        {
            get
            {
                return X;
            }
            set
            {
                X = value;
            }
        }

        /// <summary>
        /// Gets or sets a coordinate from this Location.
        /// This is a slow operation and should be avoided.
        /// X = 0, Y = 1, Z = 2.
        /// </summary>
        public double this[int index]
        {
            get
            {
                return index switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Must be between 0 and 2"),
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), index, "Must be between 0 and 2");
                }
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a Location, with all coordinates individually specified.
        /// </summary>
        public Location(double _X, double _Y, double _Z)
        {
            X = _X;
            Y = _Y;
            Z = _Z;
        }

        /// <summary>
        /// Constructs a Location where all three coordinates are set to a single value.
        /// </summary>
        public Location(double _Point)
        {
            X = _Point;
            Y = _Point;
            Z = _Point;
        }

        /// <summary>
        /// Constructs a Location, with all coordinates individually specified.
        /// </summary>
        public Location(float _X, float _Y, float _Z)
        {
            X = _X;
            Y = _Y;
            Z = _Z;
        }

        /// <summary>
        /// Constructs a Location where all three coordinates are set to a single value.
        /// </summary>
        public Location(float _Point)
        {
            X = _Point;
            Y = _Point;
            Z = _Point;
        }
        #endregion

        #region vector math
        /// <summary>
        /// Returns the flat (X/Y) linear distance of the vector location to another vector location, squared for efficiency.
        /// </summary>
        /// <returns>The squared distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistanceSquared_Flat(in Location two)
        {
            double x1 = X - two.X;
            double y1 = Y - two.Y;
            return x1 * x1 + y1 * y1;
        }

        /// <summary>
        /// Returns the full linear distance of the vector location to another vector location, squared for efficiency.
        /// <para>If squaring is undesirable, use <see cref="Distance(in Location)"/>.</para>
        /// </summary>
        /// <returns>The squared distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistanceSquared(in Location two)
        {
            double x1 = X - two.X;
            double y1 = Y - two.Y;
            double z1 = Z - two.Z;
            return x1 * x1 + y1 * y1 + z1 * z1;
        }

        /// <summary>
        /// Returns the full linear distance of the Location vector to another Location vector, which goes through a square-root operation (inefficient).
        /// <para>Where possible, prefer <see cref="DistanceSquared(in Location)"/>.</para>
        /// </summary>
        /// <returns>The square-rooted distance.</returns>
        public double Distance(in Location two)
        {
            double x1 = X - two.X;
            double y1 = Y - two.Y;
            double z1 = Z - two.Z;
            return Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
        }

        /// <summary>
        /// Caps the location to a specific maximum length, returning the result.
        /// <para>Input length should always be greater than zero.</para>
        /// </summary>
        /// <param name="len">New max length.</param>
        /// <returns>Same or shortened Location vector.</returns>
        public Location WithMaxmimumLength(double len)
        {
            Debug.Assert(len > 0, $"Length input to {nameof(WithMaxmimumLength)} should be greater than 0, but was {len}");
            double lsq = LengthSquared();
            if (lsq > len * len)
            {
                return this * (len / Math.Sqrt(lsq));
            }
            return this;
        }

        /// <summary>
        /// Sets a minimum length for this Location vector.
        /// Meaning, if the vector has a lower length than the input value, the vector will extend to the given length exactly.
        /// <para>Zero locations will result in the default value scaled to the minimum (should be a unit vector).</para>
        /// <para>Input length should always be greater than zero.</para>
        /// </summary>
        /// <param name="len">Minimum length.</param>
        /// <param name="defaultValue">The default value to use instead of zero.</param>
        /// <returns>The new Location vector.</returns>
        public Location WithMinimumLength(double len, Location defaultValue)
        {
            Debug.Assert(len > 0, $"Length input to {nameof(WithMinimumLength)} should be greater than 0, but was {len}");
            double lenSquared = LengthSquared();
            if (lenSquared < len * len)
            {
                if (lenSquared == 0)
                {
                    return defaultValue * len;
                }
                return this * (len / Math.Sqrt(lenSquared));
            }
            return this;
        }

        /// <summary>
        /// Sets a minimum length for this Location vector.
        /// Meaning, if the vector has a lower length than the input value, the vector will extend to the given length exactly.
        /// <para>Input of <see cref="Zero"/> will result in output of <see cref="Zero"/>.</para>
        /// <para>Input length should always be greater than zero.</para>
        /// </summary>
        /// <param name="len">Minimum length.</param>
        /// <returns>The new Location vector.</returns>
        public Location WithMinimumLength(double len)
        {
            return WithMinimumLength(len, Zero);
        }

        /// <summary>
        /// Sets the length of a location.
        /// <para>This will return <see cref="Zero"/> if the Location vector is <see cref="Zero"/>.</para>
        /// <para>Negative input length values will invert the vector, and produce a resultant length matching the absolute value of the input length, or zero.</para>
        /// </summary>
        /// <param name="len">The new length.</param>
        /// <returns>The Location with a length, or XP vector with the length.</returns>
        public Location SetLength(double len)
        {
            return Normalize() * len;
        }

        /// <summary>
        /// Sets the length of a location.
        /// <para>This will return a vector of the specified default value (scaled to the length) if the Location vector is <see cref="Zero"/>.</para>
        /// <para>Negative input length values will invert the vector, and produce a resultant length matching the absolute value of the input length, or zero.</para>
        /// </summary>
        /// <param name="len">The new length.</param>
        /// <param name="defaultValue">The default value to rescale. Must be already a Unit vector.</param>
        /// <returns>The Location with a length, or XP vector with the length.</returns>
        public Location SetLength(double len, Location defaultValue)
        {
            return Normalize(defaultValue) * len;
        }

        /// <summary>
        /// Returns the full linear length of the vector location, squared for efficiency.
        /// </summary>
        /// <returns>The squared length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// Returns the full linear length of the vector location, which goes through a square-root operation (inefficient).
        /// </summary>
        /// <returns>The square-rooted length.</returns>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Returns the dot product of this and another location.
        /// </summary>
        /// <param name="two">The second location.</param>
        /// <returns>The dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Dot(Location two)
        {
            return X * two.X + Y * two.Y + Z * two.Z;
        }

        /// <summary>
        /// Returns a normal form of this location.
        /// <para>Input of <see cref="Zero"/> will become the default vector instead.</para>
        /// </summary>
        /// <param name="defaultValue">The default value to use. Should be already a unit vector.</param>
        /// <returns>A valid normal location.</returns>
        public Location Normalize(Location defaultValue)
        {
            double len = Length();
            if (len == 0.0)
            {
                return defaultValue;
            }
            len = 1.0 / len;
            return new Location(X * len, Y * len, Z * len);
        }

        /// <summary>
        /// Returns a normal form of this location.
        /// <para>Input of <see cref="Zero"/> will output <see cref="Zero"/>.</para>
        /// </summary>
        /// <returns>A valid normal location.</returns>
        public Location Normalize()
        {
            double len = Length();
            if (len == 0.0)
            {
                return Zero;
            }
            len = 1.0 / len;
            return new Location(X * len, Y * len, Z * len);
        }

        /// <summary>
        /// Returns the cross product of this location with another.
        /// </summary>
        /// <param name="two">The second location vector.</param>
        /// <returns>The cross product of the two.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location CrossProduct(in Location two)
        {
            return new Location(Y * two.Z - two.Y * Z, two.X * Z - X * two.Z, X * two.Y - Y * two.X);
        }

        /// <summary>
        /// Reflect a location vector against a normal.
        /// </summary>
        /// <param name="normal">The normal vector.</param>
        /// <returns>The reflected vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location Reflect(in Location normal)
        {
            return this - (2 * Dot(normal) * normal);
        }

        /// <summary>
        /// Projects this location onto another.
        /// </summary>
        /// <param name="b">The other.</param>
        /// <returns>The projection.</returns>
        public Location Project(in Location b)
        {
            return b * (Dot(b) / b.LengthSquared());
        }

        /// <summary>
        /// Rotates this vector by a certain yaw (in radians).
        /// </summary>
        /// <param name="yaw">The yaw to rotate by (in radians).</param>
        /// <returns>The rotated vector.</returns>
        public Location Rotate(double yaw)
        {
            double cos = Math.Cos(yaw);
            double sin = Math.Sin(yaw);
            return new Location((X * cos) - (Y * sin), (X * sin) + (Y * cos), Z);
        }

        /// <summary>
        /// Rotates this vector by a certain yaw and pitch (in radians).
        /// </summary>
        /// <param name="yaw">The yaw to rotate by (in radians).</param>
        /// <param name="pitch">The pitch to rotate by (in radians).</param>
        /// <returns>The rotated vector.</returns>
        public Location Rotate(double yaw, double pitch)
        {
            double cosyaw = Math.Cos(yaw);
            double cospitch = Math.Cos(pitch);
            double sinyaw = Math.Sin(yaw);
            double sinpitch = Math.Sin(pitch);
            double bX = Z * sinpitch + X * cospitch;
            double bZ = Z * cospitch - X * sinpitch;
            return new Location(bX * cosyaw - Y * sinyaw, bX * sinyaw + Y * cosyaw, bZ);
        }
        #endregion

        #region utilities
        /// <summary>
        /// A copy of this <see cref="Location"/> with a different <see cref="X"/> coordinate value.
        /// </summary>
        /// <param name="_x">The new X coordinate.</param>
        /// <returns>The updated location.</returns>
        public Location WithX(double _x)
        {
            return new Location(_x, Y, Z);
        }

        /// <summary>
        /// A copy of this <see cref="Location"/> with a different <see cref="Y"/> coordinate value.
        /// </summary>
        /// <param name="_y">The new Y coordinate.</param>
        /// <returns>The updated location.</returns>
        public Location WithY(double _y)
        {
            return new Location(X, _y, Z);
        }

        /// <summary>
        /// A copy of this <see cref="Location"/> with a different <see cref="Z"/> coordinate value.
        /// </summary>
        /// <param name="_z">The new Z coordinate.</param>
        /// <returns>The updated location.</returns>
        public Location WithZ(double _z)
        {
            return new Location(X, Y, _z);
        }

        /// <summary>
        /// Returns a copy of this location.
        /// </summary>
        /// <returns>A copy of the location.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location Duplicate()
        {
            return new Location(X, Y, Z);
        }
        #endregion

        #region basic equality
        /// <summary>
        /// Returns whether this location matches any given generic object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Whether they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is not Location locationForm)
            {
                return false;
            }
            return Equals(locationForm);
        }

        /// <summary>
        /// Returns wether this location equals another location.
        /// </summary>
        /// <param name="v">The second location.</param>
        /// <returns>Whether they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Location v)
        {
            return v.X == X && v.Y == Y && v.Z == Z;
        }

        /// <summary>
        /// Gets a quick generic hash code for the location data.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }

        /// <summary>
        /// Returns whether the location is NaN.
        /// </summary>
        /// <returns>whether the location is NaN.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNaN()
        {
            return double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z);
        }

        /// <summary>
        /// Returns whether the location is infinite.
        /// </summary>
        /// <returns>whether the location is infinite.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInfinite()
        {
            return double.IsInfinity(X) || double.IsInfinity(Y) || double.IsInfinity(Z);
        }
        #endregion

        #region operators
        /// <summary>
        /// Returns whether two locations are equal.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Location v1, in Location v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        /// <summary>
        /// Returns whether two locations are not equal.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Location v1, in Location v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

        /// <summary>
        /// Adds two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator +(in Location v1, in Location v2)
        {
            return new Location(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// Negates a location.
        /// </summary>
        /// <param name="v">The first location.</param>
        public static Location operator -(in Location v)
        {
            return new Location(-v.X, -v.Y, -v.Z);
        }

        /// <summary>
        /// Subtracts two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator -(in Location v1, in Location v2)
        {
            return new Location(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Multiplies two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator *(in Location v1, in Location v2)
        {
            return new Location(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        /// <summary>
        /// Divides two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator /(in Location v1, in Location v2)
        {
            return new Location(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(in Location v, double scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(double scale, in Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(in Location v, float scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(float scale, in Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Divides a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator /(in Location v, double scale)
        {
            double sc = 1.0 / scale;
            return new Location(v.X * sc, v.Y * sc, v.Z * sc);
        }

        /// <summary>
        /// Divides a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator /(in Location v, float scale)
        {
            double sc = 1.0 / scale;
            return new Location(v.X * sc, v.Y * sc, v.Z * sc);
        }
        #endregion

        #region serialization
        /// <summary>
        /// Returns the location as a string in the form: X, Y, Z
        /// Inverts <see cref="FromString(string)"/>.
        /// </summary>
        /// <returns>The location string.</returns>
        public string ToSimpleString()
        {
            return $"{X}, {Y}, {Z}";
        }

        /// <summary>
        /// Returns the location as a string in the form: (X, Y, Z)
        /// Inverts <see cref="FromString(string)"/>.
        /// </summary>
        /// <returns>The location string.</returns>
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        /// <summary>
        /// The number format for <see cref="ToBasicString"/>.
        /// </summary>
        public const string BasicFormat = "0.00";

        /// <summary>
        /// Returns the location as a string in the form (X, Y, Z) with short decimals (2 places).
        /// Inverts <see cref="FromString(string)"/>.
        /// </summary>
        /// <returns>The basic location string.</returns>
        public string ToBasicString()
        {
            return "(" + X.ToString(BasicFormat) + ", " + Y.ToString(BasicFormat) + ", " + Z.ToString(BasicFormat) + ")";
        }

        /// <summary>
        /// Converts a string representation of a location to a Location object.
        /// Inverts <see cref="ToString"/> and <see cref="ToSimpleString"/>.
        /// Returns <see cref="NaN"/> when invalid.
        /// </summary>
        /// <param name="input">The location string.</param>
        /// <returns>the location object.</returns>
        public static Location FromString(string input)
        {
            string[] data = input.Replace('(', ' ').Replace(')', ' ').Replace(" ", "").SplitFast(',');
            if (data.Length != 3)
            {
                return NaN;
            }
            return new Location(StringConversionHelper.StringToDouble(data[0]), StringConversionHelper.StringToDouble(data[1]), StringConversionHelper.StringToDouble(data[2]));
        }

        /// <summary>
        /// Reads the byte array to a Location object.
        /// Expects 24 bytes.
        /// Inverts <see cref="ToDoubleBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="bytes">The bytes to read.</param>
        /// <param name="index">The index to start at.</param>
        /// <returns>the location object.</returns>
        public static Location FromDoubleBytes(byte[] bytes, int index)
        {
            return new Location(
                PrimitiveConversionHelper.BytesToDouble64(bytes, index),
                PrimitiveConversionHelper.BytesToDouble64(bytes, index + 8),
                PrimitiveConversionHelper.BytesToDouble64(bytes, index + (8 + 8))
                );
        }

        /// <summary>
        /// Converts the Location to a simple byte[] representation.
        /// Contains 24 bytes.
        /// Inverts <see cref="FromDoubleBytes(byte[], int)"/>.
        /// </summary>
        /// <returns>The bytes.</returns>
        public byte[] ToDoubleBytes()
        {
            byte[] toret = new byte[24];
            ToDoubleBytes(toret, 0);
            return toret;
        }

        /// <summary>
        /// Copies the Location into a byte array.
        /// Copies 24 bytes.
        /// Inverts <see cref="FromDoubleBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the output array.</param>
        public void ToDoubleBytes(byte[] outputBytes, int offset)
        {
            PrimitiveConversionHelper.Double64ToBytes(X, outputBytes, offset + 0);
            PrimitiveConversionHelper.Double64ToBytes(Y, outputBytes, offset + 8);
            PrimitiveConversionHelper.Double64ToBytes(Z, outputBytes, offset + (8 + 8));
        }
        #endregion

        #region conversion
        /// <summary>
        /// Converts the <see cref="Location"/> to an integer <see cref="Vector3i"/>.
        /// This will truncate decimal values.
        /// </summary>
        /// <returns>The integer vector.</returns>
        public Vector3i ToVec3i()
        {
            return new Vector3i((int)X, (int)Y, (int)Z);
        }

        /// <summary>Converts the <see cref="Location"/> value to a floating point <see cref="System.Numerics.Vector3"/>.</summary>
        public System.Numerics.Vector3 ToNumerics()
        {
            return new System.Numerics.Vector3(XF, YF, ZF);
        }
        #endregion

        #region min/max
        /// <summary>
        /// Returns the bigger valued coordinates for each of X, Y, and Z. Essentially, applies <see cref="Math.Max(double, double)"/> to each coordinate.
        /// </summary>
        /// <param name="two">The second Location vector.</param>
        /// <returns>The bigger valued coordinates.</returns>
        public Location Max(in Location two)
        {
            return new Location(Math.Max(X, two.X), Math.Max(Y, two.Y), Math.Max(Z, two.Z));
        }

        /// <summary>
        /// Returns the smaller valued coordinates for each of X, Y, and Z. Essentially, applies <see cref="Math.Min(double, double)"/> to each coordinate.
        /// </summary>
        /// <param name="two">The second Location vector.</param>
        /// <returns>The smaller valued coordinates.</returns>
        public Location Min(in Location two)
        {
            return new Location(Math.Min(X, two.X), Math.Min(Y, two.Y), Math.Min(Z, two.Z));
        }

        /// <summary>
        /// Returns the biggest coordinate in this location (biggest of X, Y, or Z).
        /// Essentially, applies <see cref="Math.Max(double, double)"/> to the 3 coordinate values.
        /// </summary>
        /// <returns>The biggest coordinate.</returns>
        public double BiggestValue()
        {
            return Math.Max(Math.Max(X, Y), Z);
        }

        /// <summary>
        /// Returns the smallest coordinate in this location (samallest of X, Y, or Z).
        /// Essentially, applies <see cref="Math.Min(double, double)"/> to the 3 coordinate values.
        /// </summary>
        /// <returns>The smallest coordinate.</returns>
        public double SmallestValue()
        {
            return Math.Min(Math.Min(X, Y), Z);
        }
        #endregion

        #region rounding

        /// <summary>
        /// Rounds each value towards the nearest integer.
        /// Essentially, applies <see cref="Math.Round(double)"/> to each of the 3 coordinate values separately.
        /// </summary>
        /// <returns>The rounded location value.</returns>
        public Location Round()
        {
            return new Location(Math.Round(X), Math.Round(Y), Math.Round(Z));
        }

        /// <summary>
        /// Rounds each value towards downward.
        /// Essentially, applies <see cref="Math.Floor(double)"/> to each of the 3 coordinate values separately.
        /// </summary>
        /// <returns>The floored location value.</returns>
        public Location Floor()
        {
            return new Location(Math.Floor(X), Math.Floor(Y), Math.Floor(Z));
        }

        /// <summary>
        /// Rounds each value towards upward.
        /// Essentially, applies <see cref="Math.Ceiling(double)"/> to each of the 3 coordinate values separately.
        /// </summary>
        /// <returns>The rounded location value.</returns>
        public Location Ceiling()
        {
            return new Location(Math.Ceiling(X), Math.Ceiling(Y), Math.Ceiling(Z));
        }

        /// <summary>
        /// Gets the absolute value of each component of this location.
        /// Essentially, applies <see cref="Math.Abs(double)"/> to each of the 3 coordinate values separately.
        /// </summary>
        public Location Abs()
        {
            return new Location(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        #endregion
    }

    /// <summary>Helper extensions for <see cref="Location"/>.</summary>
    public static class ExtensionsForLocation
    {
        /// <summary>Converts a floating point <see cref="System.Numerics.Vector3"/> to a <see cref="Location"/>.</summary>
        public static Location ToLocation(this System.Numerics.Vector3 vector)
        {
            return new Location(vector.X, vector.Y, vector.Z);
        }
    }
}
