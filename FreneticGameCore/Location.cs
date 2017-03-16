using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a 3D location, using 3 double-precision doubleing-point coordinates.
    /// Occupies 24 bytes, calculated as 8 * 3, as it has 3 fields (X, Y, Z) each occupying 8 bytes (a double).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Location : IEquatable<Location>
    {
        // TODO: Use doubles to simplify and accelerate this class?

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
        /// Returns the full linear length of the vector location, squared for efficiency.
        /// </summary>
        /// <returns>The squared length.</returns>
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
        public bool IsNaN()
        {
            return double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z);
        }

        /// <summary>
        /// Returns whether the location is infinite.
        /// </summary>
        /// <returns>whether the location is infinite.</returns>
        public bool IsInfinite()
        {
            return double.IsInfinity(X) || double.IsInfinity(Y) || double.IsInfinity(Z);
        }

        /// <summary>
        /// Returns the dot product of this and another location.
        /// </summary>
        /// <param name="two">The second location.</param>
        /// <returns>The dot product.</returns>
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

        const string basicformat = "#.00";

        /// <summary>
        /// Returns the location as a string in the form (X, Y, Z) with short decimals (2 places).
        /// Inverts .FromString()
        /// </summary>
        /// <returns>The basic location string.</returns>
        public string ToBasicString()
        {
            return "(" + X.ToString(basicformat) + ", " + Y.ToString(basicformat) + ", " + Z.ToString(basicformat) + ")";
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
        /// </summary>
        /// <returns>A valid normal location.</returns>
        public Location Normalize()
        {
            double len = Length();
            if (len == 0)
            {
                return Location.Zero;
            }
            return new Location(X / len, Y / len, Z / len);
        }

        /// <summary>
        /// Returns the cross product of this location with another.
        /// </summary>
        /// <param name="two">The second location vector.</param>
        /// <returns>The cross product of the two.</returns>
        public Location CrossProduct(Location two)
        {
            return new Location(Y * two.Z - two.Y * Z, two.X * Z - X * two.Z, X * two.Y - Y * two.X);
        }

        /// <summary>
        /// Reflect a location vector against a normal.
        /// </summary>
        /// <param name="normal">The normal vector.</param>
        /// <returns>The reflected vector.</returns>
        public Location Reflect(Location normal)
        {
            return this - (2 * this.Dot(normal) * normal);
        }

        /// <summary>
        /// Converts the Location to a simple byte[] representation.
        /// Contains 24 bytes.
        /// Inverts .FromDoubleBytes()
        /// </summary>
        public byte[] ToDoubleBytes()
        {
            byte[] toret = new byte[24];
            Utilities.DoubleToBytes(X).CopyTo(toret, 0);
            Utilities.DoubleToBytes(Y).CopyTo(toret, 8);
            Utilities.DoubleToBytes(Z).CopyTo(toret, 8 + 8);
            return toret;
        }

        /// <summary>
        /// Returns a copy of this location.
        /// </summary>
        /// <returns>A copy of the location.</returns>
        public Location Duplicate()
        {
            return new Location(X, Y, Z);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Location))
            {
                return false;
            }
            Location tobj = (Location)obj;
            return tobj.X == X && tobj.Y == Y && tobj.Z == Z;
        }

        public bool Equals(Location v)
        {
            return v.X == X && v.Y == Y && v.Z == Z;
        }

        public static bool operator ==(Location v1, Location v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static bool operator !=(Location v1, Location v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

        public override int GetHashCode()
        {
            return (int)(X + Y + Z);
        }

        public static Location operator +(Location v1, Location v2)
        {
            return new Location(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Location operator -(Location v)
        {
            return new Location(-v.X, -v.Y, -v.Z);
        }

        public static Location operator -(Location v1, Location v2)
        {
            return new Location(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Location operator *(Location v1, Location v2)
        {
            return new Location(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static Location operator /(Location v1, Location v2)
        {
            return new Location(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        public static Location operator *(Location v, double scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator *(double scale, Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator *(Location v, float scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator *(float scale, Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator /(Location v, double scale)
        {
            double sc = 1 / scale;
            return new Location(v.X * sc, v.Y * sc, v.Z * sc);
        }

        public static Location operator /(Location v, float scale)
        {
            double sc = 1 / scale;
            return new Location(v.X * sc, v.Y * sc, v.Z * sc);
        }

        /// <summary>
        /// Converts a string representation of a location to a Location object.
        /// Inverts .ToString(), .ToSimpleString()
        /// </summary>
        /// <param name="input">The location string.</param>
        /// <returns>the location object.</returns>
        public static Location FromString(string input)
        {
            string[] data = input.Replace('(', ' ').Replace(')', ' ').Replace(" ", "").SplitFast(',');
            if (data.Length != 3)
            {
                return Location.NaN;
            }
            return new Location(Utilities.StringToDouble(data[0]), Utilities.StringToDouble(data[1]), Utilities.StringToDouble(data[2]));
        }

        /// <summary>
        /// Reads the byte array to a Location object.
        /// Expects 24 bytes.
        /// Inverts .ToDoubleBytes()
        /// </summary>
        /// <param name="bytes">The bytes to read.</param>
        /// <param name="index">The index to start at.</param>
        /// <returns>the location object.</returns>
        public static Location FromDoubleBytes(byte[] bytes, int index)
        {
            if (bytes.Length - index < 24)
            {
                return new Location(0); // TODO: Exception?
            }
            double X = Utilities.BytesToDouble(Utilities.BytesPartial(bytes, index, 8));
            double Y = Utilities.BytesToDouble(Utilities.BytesPartial(bytes, index + 8, 8));
            double Z = Utilities.BytesToDouble(Utilities.BytesPartial(bytes, index + 8 + 8, 8));
            return new Location(X, Y, Z);
        }

        /// <summary>
        /// Converts the Location to a BEPUPhysics Vector3.
        /// </summary>
        /// <returns>.</returns>
        public BEPUutilities.Vector3 ToBVector()
        {
            return new BEPUutilities.Vector3(X, Y, Z);
        }

        /// <summary>
        /// Gets the location of the block this location is within. (Round-down all values).
        /// </summary>
        /// <returns>The block location.</returns>
        public Location GetBlockLocation()
        {
            return new Location(Math.Floor(X), Math.Floor(Y), Math.Floor(Z));
        }

        /// <summary>
        /// Gets the location of the next block corner up from this location. (Round-up all values).
        /// </summary>
        /// <returns>The block location.</returns>
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
        /// Returns the samallest coordinate in this location (samallest of X, Y, or Z).
        /// </summary>
        /// <returns>The samallest coordinate.</returns>
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
