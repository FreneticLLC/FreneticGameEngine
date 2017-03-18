using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    /// <summary>
    /// Holds all text styles to be used... (mainly by the SysConsole, can be used elsewhere).
    /// </summary>
    public class TextStyle
    {
        /// <summary>Default style: ^r^7</summary>
        public static string Default = "^r^7";
        /// <summary>Reset style: ^r</summary>
        public static string Reset = "^r";
        /// <summary>Bold style: ^b</summary>
        public static string Bold = "^b";
        /// <summary>Italic style: ^i</summary>
        public static string Italic = "^i";
        /// <summary>Transparent style: ^t</summary>
        public static string Transparent = "^t";
        /// <summary>Opaque style: ^o</summary>
        public static string Opaque = "^o";
        /// <summary>White color: ^7</summary>
        public static string White = "^7";
        /// <summary>Simple style: ^r^7</summary>
        public static string Color_Simple = "^r^7";
        /// <summary>Standout style: ^r^0^h^5</summary>
        public static string Color_Standout = "^r^0^h^5";
        /// <summary>Readable style: ^r^7^e^0^b</summary>
        public static string Color_Readable = "^r^7^e^0^b";
        /// <summary>Chat style: ^r^2^d</summary>
        public static string Color_Chat = "^r^2^d";
        /// <summary>Error style: ^r^0^h^3</summary>
        public static string Color_Error = "^r^0^h^3";
        /// <summary>Warning style: ^r^0^h^1</summary>
        public static string Color_Warning = "^r^0^h^1";
        /// <summary>Commandhelp style: ^r^0^h^1</summary>
        public static string Color_Commandhelp = "^r^0^h^1";
        /// <summary>Separate style: ^r^5</summary>
        public static string Color_Separate = "^r^5";
        /// <summary>Good output style: ^r^2</summary>
        public static string Color_Outgood = "^r^2";
        /// <summary>Bad output style: ^r^1</summary>
        public static string Color_Outbad = "^r^1";
        /// <summary>Important information style: ^r^3</summary>
        public static string Color_Importantinfo = "^r^3";
        /// <summary>Base coloring style: ^B</summary>
        public static string Color_Base = "^B";
    }
}
