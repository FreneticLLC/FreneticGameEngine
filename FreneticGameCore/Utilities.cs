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
using System.Security.Cryptography;
using BEPUutilities;
using System.Threading;

namespace FreneticGameCore
{
    /// <summary>
    /// Helpful utilities for general usage.
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// A UTF-8 without BOM encoding.
        /// </summary>
        public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

        /// <summary>
        /// Returns the next power of two.
        /// Meaning, the next number in the sequence:
        /// 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, ...
        /// Result is >= input.
        /// </summary>
        /// <param name="x">The value, less than or equal to the result.</param>
        /// <returns>The result, greater than or equal to the value.</returns>
        public static int NextPowerOfTwo(int x)
        {
            int mod = 1;
            for (int i = 1; i < 31; i++)
            {
                if (1 << mod <= x)
                {
                    return 1 << mod;
                }
            }
            // Number too massive!
            return x;
        }
        /// <summary>
        /// A thread-static random object for all non-deterministic objects to use.
        /// </summary>
        public static MTRandom UtilRandom
        {
            get
            {
                if (intRandom == null)
                {
                    intRandom = new MTRandom();
                }
                return intRandom;
            }
        }

        /// <summary>
        /// A thread-static random provider.
        /// </summary>
        [ThreadStatic]
        private static MTRandom intRandom;

        /// <summary>
        /// An SHA-512 hashing helper.
        /// </summary>
        public static SHA512Managed sha512 = new SHA512Managed();

        /// <summary>
        /// Password static salt part 1.
        /// </summary>
        public const string salt1 = "aB123!";

        /// <summary>
        /// Password static salt part 2.
        /// </summary>
        public const string salt2 = "--=123Tt=--";

        /// <summary>
        /// Password static salt part 3.
        /// </summary>
        public const string salt3 = "^&()xyZ";

        /// <summary>
        /// Quickly gets a Base-64 string of a hashed password input.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>A hash code.</returns>
        public static string HashQuick(string username, string password)
        {
            // TODO: Dynamic hash text maybe?
            // TODO: Really, any amount of protection at all here ;-;
            // Something fast but reasonably complex
            return Convert.ToBase64String(sha512.ComputeHash(DefaultEncoding.GetBytes(salt1 + username + salt2 + password + salt3)));
        }

        /// <summary>
        /// Converts a byte array to a ushort.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static ushort BytesToUShort(byte[] bytes)
        {
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to a float.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static float BytesToFloat(byte[] bytes)
        {
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to a double.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static double BytesToDouble(byte[] bytes)
        {
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Converts a character to a byte array.
        /// </summary>
        /// <param name="ch">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] CharToBytes(char ch)
        {
            return BitConverter.GetBytes(ch);
        }

        /// <summary>
        /// Converts a short to a byte array.
        /// </summary>
        /// <param name="sh">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] ShortToBytes(short sh)
        {
            return BitConverter.GetBytes(sh);
        }

        /// <summary>
        /// Converts a ushort to a byte array.
        /// </summary>
        /// <param name="ush">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] UShortToBytes(ushort ush)
        {
            return BitConverter.GetBytes(ush);
        }

        /// <summary>
        /// Converts a float to a byte array.
        /// </summary>
        /// <param name="flt">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] FloatToBytes(float flt)
        {
            return BitConverter.GetBytes(flt);
        }

        /// <summary>
        /// Converts a double to a byte array.
        /// </summary>
        /// <param name="flt">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] DoubleToBytes(double flt)
        {
            return BitConverter.GetBytes(flt);
        }

        /// <summary>
        /// Converts a byte array to a character.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static char BytesToChar(byte[] bytes)
        {
            return BitConverter.ToChar(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to a short.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static short BytesToShort(byte[] bytes)
        {
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to an int.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static int BytesToInt(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to an unsigned int.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static uint BytesToUInt(byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to a long.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static long BytesToLong(byte[] bytes)
        {
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to an unsigned long.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The actual value of it.</returns>
        public static ulong BytesToULong(byte[] bytes)
        {
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// Converts an int to a byte array.
        /// </summary>
        /// <param name="intty">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] IntToBytes(int intty)
        {
            return BitConverter.GetBytes(intty);
        }

        /// <summary>
        /// Converts an unsigned int to a byte array.
        /// </summary>
        /// <param name="intty">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] UIntToBytes(uint intty)
        {
            return BitConverter.GetBytes(intty);
        }

        /// <summary>
        /// Converts a long to a byte array.
        /// </summary>
        /// <param name="intty">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] LongToBytes(long intty)
        {
            return BitConverter.GetBytes(intty);
        }

        /// <summary>
        /// Converts an unsigned long to a byte array.
        /// </summary>
        /// <param name="intty">The actual value of it.</param>
        /// <returns>The byte array.</returns>
        public static byte[] ULongToBytes(ulong intty)
        {
            return BitConverter.GetBytes(intty);
        }

        /// <summary>
        /// Checks an exception for rethrow necessity.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        public static void CheckException(Exception ex)
        {
            if (ex is ThreadAbortException)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Grabs a sub section of a byte array.
        /// TODO: Reduce need for BytesPartial in packets via adding an index to BytesTo[Type]!
        /// </summary>
        /// <param name="full">The original byte array.</param>
        /// <param name="start">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns>The subset.</returns>
        public static byte[] BytesPartial(byte[] full, int start, int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = full[i + start];
            }
            return data;
        }

        /// <summary>
        /// Steps a value towards a goal by a specified amount, automatically moving the correct direction (positive or negative) and preventing going past the goal.
        /// </summary>
        /// <param name="start">The initial value.</param>
        /// <param name="target">The goal value.</param>
        /// <param name="amount">The amount to step by.</param>
        /// <returns>The result.</returns>
        public static double StepTowards(double start, double target, double amount)
        {
            if (start < target - amount)
            {
                return start + amount;
            }
            else if (start > target + amount)
            {
                return start - amount;
            }
            else
            {
                return target;
            }
        }

        /// <summary>
        /// Returns whether a number is close to another number, within a specified range.
        /// </summary>
        /// <param name="one">The first number.</param>
        /// <param name="target">The second number.</param>
        /// <param name="amount">The range.</param>
        /// <returns>Whether it's close.</returns>
        public static bool IsCloseTo(double one, double target, double amount)
        {
            return one > target ? one - amount < target : one + amount > target;
        }

        /// <summary>
        /// Converts a string to a double. Returns 0 if the string is not a valid double.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The converted double.</returns>
        public static float StringToFloat(string input)
        {
            if (float.TryParse(input, out float output))
            {
                return output;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Converts a string to a double. Returns 0 if the string is not a valid double.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The converted double.</returns>
        public static double StringToDouble(string input)
        {
            if (double.TryParse(input, out double output))
            {
                return output;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Converts a string to a ushort. Returns 0 if the string is not a valid ushort.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The converted ushort.</returns>
        public static ushort StringToUShort(string input)
        {
            if (ushort.TryParse(input, out ushort output))
            {
                return output;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts a string to a int. Returns 0 if the string is not a valid int.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The converted int.</returns>
        public static int StringToInt(string input)
        {
            if (int.TryParse(input, out int output))
            {
                return output;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts a string to a long. Returns 0 if the string is not a valid long.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The converted long.</returns>
        public static long StringToLong(string input)
        {
            if (long.TryParse(input, out long output))
            {
                return output;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a string representation of the specified time.
        /// </summary>
        /// <returns>The time as a string.</returns>
        public static string DateTimeToString(DateTime dt)
        {
            string utcoffset = "";
            DateTime UTC = dt.ToUniversalTime();
            if (dt.CompareTo(UTC) < 0)
            {
                TimeSpan span = UTC.Subtract(dt);
                utcoffset = "-" + Pad(((int)Math.Floor(span.TotalHours)).ToString(), '0', 2) + ":" + Pad(span.Minutes.ToString(), '0', 2);
            }
            else
            {
                TimeSpan span = dt.Subtract(UTC);
                utcoffset = "+" + Pad(((int)Math.Floor(span.TotalHours)).ToString(), '0', 2) + ":" + Pad(span.Minutes.ToString(), '0', 2);
            }
            return Pad(dt.Year.ToString(), '0', 4) + "/" + Pad(dt.Month.ToString(), '0', 2) + "/" +
                    Pad(dt.Day.ToString(), '0', 2) + " " + Pad(dt.Hour.ToString(), '0', 2) + ":" +
                    Pad(dt.Minute.ToString(), '0', 2) + ":" + Pad(dt.Second.ToString(), '0', 2) + " UTC" + utcoffset;
        }

        /// <summary>
        /// Pads a string to a specified length with a specified input, on a specified side.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="padding">The symbol to pad with.</param>
        /// <param name="length">How far to pad it to.</param>
        /// <param name="left">Whether to pad left (true), or right (false).</param>
        /// <returns>The padded string.</returns>
        public static string Pad(string input, char padding, int length, bool left = true)
        {
            int targetlength = length - input.Length;
            StringBuilder pad = new StringBuilder(targetlength <= 0 ? 1 : targetlength);
            for (int i = 0; i < targetlength; i++)
            {
                pad.Append(padding);
            }
            if (left)
            {
                return pad + input;
            }
            else
            {
                return input + pad;
            }
        }

        /// <summary>
        /// Returns a peice of text copied a specified number of times.
        /// </summary>
        /// <param name="text">What text to copy.</param>
        /// <param name="times">How many times to copy it.</param>
        /// <returns>.</returns>
        public static string CopyText(string text, int times)
        {
            StringBuilder toret = new StringBuilder(text.Length * times);
            for (int i = 0; i < times; i++)
            {
                toret.Append(text);
            }
            return toret.ToString();
        }

        /// <summary>
        /// Returns the number of times a character occurs in a string.
        /// </summary>
        /// <param name="input">The string containing the character.</param>
        /// <param name="countme">The character which the string contains.</param>
        /// <returns>How many times the character occurs.</returns>
        public static int CountCharacter(string input, char countme)
        {
            int count = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == countme)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Combines a list of strings into a single string, separated by spaces.
        /// </summary>
        /// <param name="input">The list of strings to combine.</param>
        /// <param name="start">The index to start from.</param>
        /// <returns>The combined string.</returns>
        public static string Concat(List<string> input, int start = 0)
        {
            StringBuilder output = new StringBuilder();
            for (int i = start; i < input.Count; i++)
            {
                output.Append(input[i]).Append(" ");
            }
            return (output.Length > 0 ? output.ToString().Substring(0, output.Length - 1) : "");
        }

        /// <summary>
        /// If raw string data is input by a user, call this function to clean it for tag-safety.
        /// </summary>
        /// <param name="input">The raw string.</param>
        /// <returns>A cleaned string.</returns>
        public static string CleanStringInput(string input)
        {
            // No nulls!
            return input.Replace('\0', ' ');
        }

        /// <summary>
        /// Used to identify if an input character is a valid color symbol (generally the character that follows a '^'), for use by RenderColoredText
        /// </summary>
        /// <param name="c"><paramref name="c"/>The character to check.</param>
        /// <returns>whether the character is a valid color symbol.</returns>
        public static bool IsColorSymbol(char c)
        {
            return ((c >= '0' && c <= '9') /* 0123456789 */ ||
                    (c >= 'a' && c <= 'b') /* ab */ ||
                    (c >= 'd' && c <= 'f') /* def */ ||
                    (c >= 'h' && c <= 'l') /* hijkl */ ||
                    (c >= 'n' && c <= 'u') /* nopqrstu */ ||
                    (c >= 'R' && c <= 'T') /* RST */ ||
                    (c >= '#' && c <= '&') /* #$%& */ || // 35 - 38
                    (c >= '(' && c <= '*') /* ()* */ || // 40 - 42
                    (c == 'A') ||
                    (c == 'O') ||
                    (c == '-') || // 45
                    (c == '!') || // 33
                    (c == '@') // 64
                    );
        }

        /// <summary>
        /// Represents the constant PI / 180.
        /// </summary>
        public const double PI180 = Math.PI / 180.0;

        /// <summary>
        /// Returns a one-length vector of the Yaw/Pitch angle input.
        /// </summary>
        /// <param name="yaw">The yaw angle, in radians.</param>
        /// <param name="pitch">The pitch angle, in radians.</param>
        /// <returns>.</returns>
        public static Location ForwardVector(double yaw, double pitch)
        {
            double cp = Math.Cos(pitch);
            return new Location(-(cp * Math.Cos(yaw)), -(cp * Math.Sin(yaw)), (Math.Sin(pitch)));
        }

        /// <summary>
        /// Returns a one-length vector of the Yaw/Pitch angle input in degrees
        /// </summary>
        /// <param name="yaw">The yaw angle, in radians.</param>
        /// <param name="pitch">The pitch angle, in radians.</param>
        /// <returns>.</returns>
        public static Location ForwardVector_Deg(double yaw, double pitch)
        {
            double pitchdeg = pitch * PI180;
            double yawdeg = yaw * PI180;
            double cp = Math.Cos(pitchdeg);
            return new Location(-(cp * Math.Cos(yawdeg)), -(cp * Math.Sin(yawdeg)), (Math.Sin(pitchdeg)));
        }

        /// <summary>
        /// Rotates a vector by a certain yaw.
        /// </summary>
        /// <param name="vec">The original vector.</param>
        /// <param name="yaw">The yaw to rotate by.</param>
        /// <returns>The rotated vector.</returns>
        public static Location RotateVector(Location vec, double yaw)
        {
            double cos = Math.Cos(yaw);
            double sin = Math.Sin(yaw);
            return new Location((vec.X * cos) - (vec.Y * sin), (vec.X * sin) + (vec.Y * cos), vec.Z);
        }

        /// <summary>
        /// Rotates a vector by a certain yaw and pitch.
        /// </summary>
        /// <param name="vec">The original vector.</param>
        /// <param name="yaw">The yaw to rotate by.</param>
        /// <param name="pitch">The pitch to rotate by.</param>
        /// <returns>The rotated vector.</returns>
        public static Location RotateVector(Location vec, double yaw, double pitch)
        {
            double cosyaw = Math.Cos(yaw);
            double cospitch = Math.Cos(pitch);
            double sinyaw = Math.Sin(yaw);
            double sinpitch = Math.Sin(pitch);
            double bX = vec.Z * sinpitch + vec.X * cospitch;
            double bZ = vec.Z * cospitch - vec.X * sinpitch;
            return new Location(bX * cosyaw - vec.Y * sinyaw, bX * sinyaw + vec.Y * cosyaw, bZ);
        }

        /// <summary>
        /// Converts a string to a quaternion.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <returns>The quaternion, or the identity quaternion.</returns>
        public static BEPUutilities.Quaternion StringToQuat(string input)
        {
            string[] data = input.Replace('(', ' ').Replace(')', ' ').Replace(" ", "").SplitFast(',');
            if (data.Length != 4)
            {
                return BEPUutilities.Quaternion.Identity;
            }
            return new BEPUutilities.Quaternion(StringToFloat(data[0]), StringToFloat(data[1]), StringToFloat(data[2]), StringToFloat(data[3]));
        }

        /// <summary>
        /// Converts a quaternion to a string.
        /// </summary>
        /// <param name="quat">The quaternion.</param>
        /// <returns>The string.</returns>
        public static string QuatToString(BEPUutilities.Quaternion quat)
        {
            return "(" + quat.X + ", " + quat.Y + ", " + quat.Z + ", " + quat.W + ")";
        }

        /// <summary>
        /// Converts a quaternion to a byte array.
        /// 16 bytes.
        /// </summary>
        /// <param name="quat">The quaternion.</param>
        /// <returns>The byte array.</returns>
        public static byte[] QuaternionToBytes(BEPUutilities.Quaternion quat)
        {
            byte[] dat = new byte[4 + 4 + 4 + 4];
            FloatToBytes((float)quat.X).CopyTo(dat, 0);
            FloatToBytes((float)quat.Y).CopyTo(dat, 4);
            FloatToBytes((float)quat.Z).CopyTo(dat, 4 + 4);
            FloatToBytes((float)quat.W).CopyTo(dat, 4 + 4 + 4);
            return dat;
        }

        /// <summary>
        /// Converts a byte array to a quaternion.
        /// </summary>
        /// <param name="dat">The byte array.</param>
        /// <param name="offset">The offset in the array.</param>
        /// <returns>The quaternion.</returns>
        public static BEPUutilities.Quaternion BytesToQuaternion(byte[] dat, int offset)
        {
            return new BEPUutilities.Quaternion(BytesToFloat(BytesPartial(dat, offset, 4)), BytesToFloat(BytesPartial(dat, offset + 4, 4)),
                BytesToFloat(BytesPartial(dat, offset + 4 + 4, 4)), BytesToFloat(BytesPartial(dat, offset + 4 + 4 + 4, 4)));

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
            return Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), (rot.X * PI180))
                    * Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (rot.Y * PI180))
                    * Matrix.CreateFromAxisAngle(new Vector3(0, 0, 1), (rot.Z * PI180));
        }

        /// <summary>
        /// Converts a forward vector to a yaw angle, in radians.
        /// </summary>
        /// <param name="input">The forward vector.</param>
        /// <returns>The yaw angle.</returns>
        public static double VectorToAnglesYawRad(Location input)
        {
            if (input.X == 0 && input.Y == 0)
            {
                return 0;
            }
            if (input.X != 0)
            {
                return Math.Atan2(input.Y, input.X);
            }
            if (input.Y > 0)
            {
                return 0;
            }
            return Math.PI;
        }

        /// <summary>
        /// Converts a forward vector to yaw/pitch angles.
        /// </summary>
        /// <param name="input">The forward vector.</param>
        /// <returns>The yaw/pitch angle vector.</returns>
        public static Location VectorToAngles(Location input)
        {
            if (input.X == 0 && input.Y == 0)
            {
                if (input.Z > 0)
                {
                    return new Location(0, 0, 0);
                }
                else
                {
                    return new Location(0, 180, 0);
                }
            }
            else
            {
                double yaw;
                double pitch;
                if (input.X != 0)
                {
                    yaw = (Math.Atan2(input.Y, input.X) * (180.0 / Math.PI));
                }
                else if (input.Y > 0)
                {
                    yaw = 0;
                }
                else
                {
                    yaw = 180;
                }
                pitch = (Math.Atan2(input.Z, Math.Sqrt(input.X * input.X + input.Y * input.Y)) * (180.0 / Math.PI));
                while (pitch < -180)
                {
                    pitch += 360;
                }
                while (pitch > 180)
                {
                    pitch -= 360;
                }
                while (yaw < 0)
                {
                    yaw += 360;
                }
                while (yaw > 360)
                {
                    yaw -= 360;
                }
                Location loc = new Location()
                {
                    Yaw = yaw,
                    Pitch = pitch
                };
                return loc;
            }
        }

        /// <summary>
        /// Validates a username as correctly formatted.
        /// </summary>
        /// <param name="str">The username to validate.</param>
        /// <returns>Whether the username is valid.</returns>
        public static bool ValidateUsername(string str)
        {
            if (str == null)
            {
                return false;
            }
            // Length = 4-15
            if (str.Length < 4 || str.Length > 15)
            {
                return false;
            }
            // Starts A-Z
            if (!(str[0] >= 'a' && str[0] <= 'z') && !(str[0] >= 'A' && str[0] <= 'Z'))
            {
                return false;
            }
            // All symbols are A-Z, 0-9, _
            for (int i = 0; i < str.Length; i++)
            {
                if (!(str[i] >= 'a' && str[i] <= 'z') && !(str[i] >= 'A' && str[i] <= 'Z')
                    && !(str[i] >= '0' && str[i] <= '9') && !(str[i] == '_'))
                {
                    return false;
                }
            }
            // Valid if all tests above passed
            return true;
        }

        /// <summary>
        /// Calculates a Halton Sequence result.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="basen">The base number, should be prime.</param>
        static double HaltonSequence(int index, int basen)
        {
            if (basen <= 1)
            {
                return 0;
            }
            double res = 0;
            double f = 1;
            int i = index;
            while (i > 0)
            {
                f = f / basen;
                res = res + f * (i % basen);
                i = (int)Math.Floor((double)i / basen);
            }
            return res;
        }

        /// <summary>
        /// Formats a long with "123,456" style notation.
        /// </summary>
        /// <param name="input">The number.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatNumber(long input)
        {
            // TODO: Better method here.
            string basinp = input.ToString();
            string creation = "";
            int c = 0;
            for (int i = basinp.Length - 1; i >= 0; i--)
            {
                if ((c % 3) == 0 && c != 0)
                {
                    creation = basinp[i] + "," + creation;
                }
                else
                {
                    creation = basinp[i] + creation;
                }
                c++;
            }
            return creation;
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

    /// <summary>
    /// Holds a volatile integer.
    /// TODO: Delete?
    /// </summary>
    public class IntHolder
    {
        /// <summary>
        /// The value.
        /// </summary>
        public volatile int Value = 0;
    }

    /// <summary>
    /// Holds any data in a class object.
    /// </summary>
    /// <typeparam name="T">The type of data to holder.</typeparam>
    public class DataHolder<T>
    {
        /// <summary>
        /// The held data.
        /// </summary>
        public T Data;
    }
}
