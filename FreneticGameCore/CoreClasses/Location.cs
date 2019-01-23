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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using FreneticGameCore.Collision;
using FreneticUtilities.FreneticExtensions;
using FreneticGameCore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a 3D location, using 3 double-precision floating-point coordinates.
    /// Occupies 24 bytes, calculated as 8 * 3, as it has 3 fields (X, Y, Z) each occupying 8 bytes (a double).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Location : IEquatable<Location>
    {
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
        /// If this location is an angle set, returns the Yaw value of this angle set.
        /// Equivalent to Z.
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
        /// Equivalent to Y.
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
        /// Equivalent to X.
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
        /// This is a slow operation.
        /// X = 0, Y = 1, Z = 2.
        /// </summary>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new ArgumentOutOfRangeException("index", index, "Must be between 0 and 2");
                }
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
                        throw new ArgumentOutOfRangeException("index", index, "Must be between 0 and 2");
                }
            }
        }

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
        /// Constructs a Location from a BEPUPhysics Vector3 structure, perfectly replicating it.
        /// </summary>
        public Location(BEPUutilities.Vector3 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
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

        /// <summary>
        /// Returns the flat (X/Y) linear distance of the vector location to another vector location, squared for efficiency.
        /// </summary>
        /// <returns>The squared distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistanceSquared_Flat(Location two)
        {
            double x1 = X - two.X;
            double y1 = Y - two.Y;
            return x1 * x1 + y1 * y1;
        }

        /// <summary>
        /// Returns the full linear distance of the vector location to another vector location, squared for efficiency.
        /// </summary>
        /// <returns>The squared distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistanceSquared(Location two)
        {
            double x1 = X - two.X;
            double y1 = Y - two.Y;
            double z1 = Z - two.Z;
            return x1 * x1 + y1 * y1 + z1 * z1;
        }

        /// <summary>
        /// Returns the full linear distance of the vector location to another vector location, which goes through a square-root operation (inefficient).
        /// </summary>
        /// <returns>The square-rooted distance.</returns>
        public double Distance(Location two)
        {
            double x1 = X - two.X;
            double y1 = Y - two.Y;
            double z1 = Z - two.Z;
            return Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
        }

        /// <summary>
        /// Caps the location to a specific maximum length, returning the result.
        /// <para>Negative numbers will cause issues!</para>
        /// </summary>
        /// <param name="len">New max length.</param>
        /// <returns>Same or shortened Location vector.</returns>
        public Location CappedToLength(double len)
        {
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
        /// Zero locations will result in an X-Positive vector.
        /// </summary>
        /// <param name="len">Minimum length.</param>
        /// <returns>The new Location vector.</returns>
        public Location MinimumLengthXP(double len)
        {
            double lsq = LengthSquared();
            if (lsq < len * len)
            {
                if (lsq == 0)
                {
                    return UnitX;
                }
                return this * (len / Math.Sqrt(lsq));
            }
            return this;
        }

        /// <summary>
        /// Sets a minimum length for this Location vector.
        /// Meaning, if the vector has a lower length than the input value, the vector will extend to the given length exactly.
        /// Zero locations will result in Zero output.
        /// </summary>
        /// <param name="len">Minimum length.</param>
        /// <returns>The new Location vector.</returns>
        public Location MinimumLength(double len)
        {
            double lsq = LengthSquared();
            if (lsq < len * len)
            {
                if (lsq == 0)
                {
                    return Zero;
                }
                return this * (len / Math.Sqrt(lsq));
            }
            return this;
        }

        /// <summary>
        /// Sets the length of a location.
        /// This will return Zero if the Location vector is Zero.
        /// Negative input length values will invert the vector, and produce a resultant length matching the absolute value of the input length, or zero.
        /// </summary>
        /// <param name="len">The new length.</param>
        /// <returns>The Location with a length, or XP vector with the length.</returns>
        public Location SetLength(double len)
        {
            return Normalize() * len;
        }

        /// <summary>
        /// Sets the length of a location.
        /// This will return an X-Positive directional vector if the Location vector is Zero.
        /// Negative input length values will invert the vector, and produce a resultant length matching the absolute value of the input length.
        /// </summary>
        /// <param name="len">The new length.</param>
        /// <returns>The Location with a length, or zero.</returns>
        public Location SetLengthXP(double len)
        {
            return NormalizeOrXP() * len;
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
        /// Returns the location as a string in the form: (X, Y, Z)
        /// Inverts .FromString()
        /// </summary>
        /// <returns>The location string.</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        /// <summary>
        /// The number format for <see cref="ToBasicString"/>.
        /// </summary>
        public const string BasicFormat = "0.00";

        /// <summary>
        /// Returns the location as a string in the form (X, Y, Z) with short decimals (2 places).
        /// Inverts .FromString()
        /// </summary>
        /// <returns>The basic location string.</returns>
        public string ToBasicString()
        {
            return "(" + X.ToString(BasicFormat) + ", " + Y.ToString(BasicFormat) + ", " + Z.ToString(BasicFormat) + ")";
        }

        /// <summary>
        /// Returns the location as a string in the form: X, Y, Z
        /// Inverts .FromString()
        /// </summary>
        /// <returns>The location string.</returns>
        public string ToSimpleString()
        {
            return X + ", " + Y + ", " + Z;
        }

        /// <summary>
        /// Returns a normal form of this location.
        /// Zeroes become X-Positive vector.
        /// </summary>
        /// <returns>A valid normal location.</returns>
        public Location NormalizeOrXP()
        {
            double len = Length();
            if (len == 0.0)
            {
                return UnitX;
            }
            len = 1.0 / len;
            return new Location(X * len, Y * len, Z * len);
        }

        /// <summary>
        /// Returns a normal form of this location.
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
        public Location CrossProduct(Location two)
        {
            return new Location(Y * two.Z - two.Y * Z, two.X * Z - X * two.Z, X * two.Y - Y * two.X);
        }

        /// <summary>
        /// Reflect a location vector against a normal.
        /// </summary>
        /// <param name="normal">The normal vector.</param>
        /// <returns>The reflected vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location Reflect(Location normal)
        {
            return this - (2 * Dot(normal) * normal);
        }

        /// <summary>
        /// Projects this location onto another.
        /// </summary>
        /// <param name="b">The other.</param>
        /// <returns>The projection.</returns>
        public Location Project(Location b)
        {
            return b * (Dot(b) / b.LengthSquared());
        }

        /// <summary>
        /// Converts the Location to a simple byte[] representation.
        /// Contains 24 bytes.
        /// Inverts <see cref="FromDoubleBytes(byte[], int)"/>.
        /// </summary>
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
        public void ToDoubleBytes(byte[] outputBytes, int offset)
        {
            PrimitiveConversionHelper.Double64ToBytes(X, outputBytes, offset + 0);
            PrimitiveConversionHelper.Double64ToBytes(Y, outputBytes, offset + 8);
            PrimitiveConversionHelper.Double64ToBytes(Z, outputBytes, offset + (8 + 8));
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

        /// <summary>
        /// Returns whether this location matches any given generic object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Whether they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            Location tobj = (Location)obj;
            return tobj.X == X && tobj.Y == Y && tobj.Z == Z;
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
        /// Returns whether two locations are equal.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Location v1, Location v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        /// <summary>
        /// Returns whether two locations are not equal.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Location v1, Location v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
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
        /// Adds two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator +(Location v1, Location v2)
        {
            return new Location(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// Negates a location.
        /// </summary>
        /// <param name="v">The first location.</param>
        public static Location operator -(Location v)
        {
            return new Location(-v.X, -v.Y, -v.Z);
        }

        /// <summary>
        /// Subtracts two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator -(Location v1, Location v2)
        {
            return new Location(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Multiplies two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator *(Location v1, Location v2)
        {
            return new Location(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        /// <summary>
        /// Divides two locations.
        /// </summary>
        /// <param name="v1">The first location.</param>
        /// <param name="v2">The second location.</param>
        public static Location operator /(Location v1, Location v2)
        {
            return new Location(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(Location v, double scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(double scale, Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(Location v, float scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Multiplies a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator *(float scale, Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Divides a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator /(Location v, double scale)
        {
            double sc = 1.0 / scale;
            return new Location(v.X * sc, v.Y * sc, v.Z * sc);
        }

        /// <summary>
        /// Divides a location by a scalar.
        /// </summary>
        /// <param name="v">The location.</param>
        /// <param name="scale">The scalar.</param>
        public static Location operator /(Location v, float scale)
        {
            double sc = 1.0 / scale;
            return new Location(v.X * sc, v.Y * sc, v.Z * sc);
        }

        /// <summary>
        /// Converts a string representation of a location to a Location object.
        /// Inverts <see cref="ToString"/> and <see cref="ToSimpleString"/>.
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
        /// Converts the Location to a BEPUPhysics Vector3.
        /// </summary>
        /// <returns>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BEPUutilities.Vector3 ToBVector()
        {
            return new BEPUutilities.Vector3(X, Y, Z);
        }

        /// <summary>
        /// Gets the location of the block this location is within. (Round-down all values).
        /// </summary>
        /// <returns>The block location.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location GetBlockLocation()
        {
            return new Location(Math.Floor(X), Math.Floor(Y), Math.Floor(Z));
        }

        /// <summary>
        /// Converts the Location to an integer vector.
        /// </summary>
        /// <returns>The integer vector.</returns>
        public Vector3i ToVec3i()
        {
            return new Vector3i((int)X, (int)Y, (int)Z);
        }

        /// <summary>
        /// Gets the location of the next block corner up from this location. (Round-up all values).
        /// </summary>
        /// <returns>The block location.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location GetUpperBlockBorder()
        {
            return new Location(Math.Ceiling(X), Math.Ceiling(Y), Math.Ceiling(Z));
        }

        /// <summary>
        /// Returns the bigger valued coordinates for each of X, Y, and Z. Essentially, applies Math.Max to each coordinate.
        /// </summary>
        /// <param name="two">The second Location vector.</param>
        /// <returns>The bigger valued coordinates.</returns>
        public Location Max(Location two)
        {
            return new Location(Math.Max(X, two.X), Math.Max(Y, two.Y), Math.Max(Z, two.Z));
        }

        /// <summary>
        /// Returns the smaller valued coordinates for each of X, Y, and Z. Essentially, applies Math.Min to each coordinate.
        /// </summary>
        /// <param name="two">The second Location vector.</param>
        /// <returns>The smaller valued coordinates.</returns>
        public Location Min(Location two)
        {
            return new Location(Math.Min(X, two.X), Math.Min(Y, two.Y), Math.Min(Z, two.Z));
        }

        /// <summary>
        /// Returns the biggest coordinate in this location (biggest of X, Y, or Z).
        /// </summary>
        /// <returns>The biggest coordinate.</returns>
        public double BiggestValue()
        {
            return Math.Max(Math.Max(X, Y), Z);
        }

        /// <summary>
        /// Returns the smallest coordinate in this location (samallest of X, Y, or Z).
        /// </summary>
        /// <returns>The smallest coordinate.</returns>
        public double SmallestValue()
        {
            return Math.Min(Math.Min(X, Y), Z);
        }

        /// <summary>
        /// Accelerated method of adding 3 locations together, for debug reasons.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddThree(ref Location l1, ref Location l2, ref Location l3, out Location l4)
        {
            l4 = new Location(l1.X + l2.X + l3.X, l1.Y + l2.Y + l3.Y, l1.Z + l2.Z + l3.Z);
        }
    }
}