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
using FGECore.CoreSystems;

namespace FGECore.ConsoleHelpers
{
    /// <summary>
    /// Holds all common text styles to be used by the <see cref="SysConsole"/> or FGEGraphic's FontSetEngine.
    /// </summary>
    public static class TextStyle
    {
        /// <summary>Default style, default value: ^r^7</summary>
        public static string Default = "^r^7";
        /// <summary>Reset style, default value: ^r</summary>
        public static string Reset = "^r";
        /// <summary>Bold style, default value: ^b</summary>
        public static string Bold = "^b";
        /// <summary>Italic style, default value: ^i</summary>
        public static string Italic = "^i";
        /// <summary>Transparent style, default value: ^t</summary>
        public static string Transparent = "^t";
        /// <summary>Opaque style, default value: ^o</summary>
        public static string Opaque = "^o";
        /// <summary>White color, default value: ^7</summary>
        public static string White = "^7";
        /// <summary>Minor style, default value: ^r^)</summary>
        public static string Minor = "^r^)";
        /// <summary>Simple style, default value: ^r^7</summary>
        public static string Simple = "^r^7";
        /// <summary>Standout style, default value: ^r^0^h^5</summary>
        public static string Standout = "^r^0^h^5";
        /// <summary>Readable style, default value: ^r^7^e^0^b</summary>
        public static string Readable = "^r^7^e^0^b";
        /// <summary>Chat style, default value: ^r^2^d</summary>
        public static string Chat = "^r^2^d";
        /// <summary>Error style, default value: ^r^0^h^3</summary>
        public static string Error = "^r^0^h^3";
        /// <summary>Warning style, default value: ^r^0^h^1</summary>
        public static string Warning = "^r^0^h^1";
        /// <summary>Commandhelp style, default value: ^r^0^h^1</summary>
        public static string Commandhelp = "^r^0^h^1";
        /// <summary>Separate style, default value: ^r^5</summary>
        public static string Separate = "^r^5";
        /// <summary>Good output style, default value: ^r^2</summary>
        public static string Outgood = "^r^2";
        /// <summary>Bad output style, default value: ^r^1</summary>
        public static string Outbad = "^r^1";
        /// <summary>Important information style, default value: ^r^3</summary>
        public static string Importantinfo = "^r^3";
        /// <summary>Base coloring style, default value: ^B</summary>
        public static string Base = "^B";

        /// <summary>
        /// Applies a base color to the string.
        /// That is, replaces <see cref="Base"/> with the input color (and a reset for any existing colors).
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="color">The new base color to use.</param>
        /// <returns>The fixed string.</returns>
        public static string ApplyBaseColor(this string text, string color)
        {
            return text.Replace(Base, "^r^7" + color);
        }
    }
}
