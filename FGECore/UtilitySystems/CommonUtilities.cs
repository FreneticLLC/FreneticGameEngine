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
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.MathHelpers;

namespace FGECore.UtilitySystems
{
    /// <summary>
    /// Helpful utilities for general usage.
    /// </summary>
    public static class CommonUtilities
    {
        /// <summary>
        /// A thread-static random object for all non-deterministic objects to use.
        /// When possible, this should be avoided in favor of contextually available random objects.
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
        /// Grabs a sub section of a byte array.
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
        /// Checks an exception for rethrow necessity.
        /// <para>This in theory should not be needed as <see cref="ThreadAbortException"/> shouldn't be miscaught, but in practice it seems to sometimes happen (might no longer apply for NET 5 update?).</para>
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
        /// Returns a peice of text copied a specified number of times.
        /// </summary>
        /// <param name="text">What text to copy.</param>
        /// <param name="times">How many times to copy it.</param>
        /// <returns>The repeated text.</returns>
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
        /// Alphabetical character matcher (a-z, A-Z).
        /// </summary>
        public static readonly AsciiMatcher AlphabetMatcher = new AsciiMatcher(AsciiMatcher.BothCaseLetters);

        /// <summary>
        /// Valid ASCII symbols for a plaintext alphanumeric username (a-z, A-Z, 0-9, _).
        /// </summary>
        public static readonly AsciiMatcher UsernameValidationMatcher = new AsciiMatcher(AsciiMatcher.BothCaseLetters + AsciiMatcher.Digits + "_");

        /// <summary>
        /// Validates a username as correctly formatted, as plaintext alphanumeric ASCII (a-z, A-Z, 0-9, _).
        /// Also enforces length between 3 and 15 symbols, inclusive.
        /// </summary>
        /// <param name="str">The username to validate.</param>
        /// <returns>Whether the username is valid.</returns>
        public static bool ValidateUsername(string str)
        {
            if (str == null)
            {
                return false;
            }
            // Length = 3-15
            if (str.Length < 3 || str.Length > 15)
            {
                return false;
            }
            // Starts A-Z
            if (!AlphabetMatcher.IsMatch(str[0]))
            {
                return false;
            }
            // All symbols are a-z, A-Z, 0-9, _
            return UsernameValidationMatcher.IsOnlyMatches(str);
        }

        /// <summary>
        /// Formats a <see cref="long"/> with comma-separated thousands ("123,456" style notation).
        /// </summary>
        /// <param name="input">The number.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatThousands(long input)
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
    }
}
