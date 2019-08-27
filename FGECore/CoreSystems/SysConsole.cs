//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FGECore.FileSystems;
using System.IO;
using FGECore.StackNoteSystem;
using FGECore.UtilitySystems;
using FGECore.ConsoleHelpers;
using FreneticUtilities.FreneticToolkit;

namespace FGECore.CoreSystems
{
    /// <summary>
    /// A helper wrapper for the System Console.
    /// TODO: Make this not be purely static based!
    /// </summary>
    public class SysConsole
    {
        /// <summary>
        /// All currently waiting messages.
        /// </summary>
        readonly static List<KeyValuePair<string, string>> Waiting = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Locker for the console.
        /// </summary>
        static Object ConsoleLock;

        /// <summary>
        /// Locker for writing.
        /// </summary>
        static Object WriteLock;

        /// <summary>
        /// The thread being used for console output.
        /// </summary>
        static Thread ConsoleOutputThread;

        /// <summary>
        /// Helper to cancel the console output thread.
        /// </summary>
        public static CancellationTokenSource ConsoleOutputCanceller;

        /// <summary>
        /// Closes the SysConsole.
        /// </summary>
        public static void ShutDown()
        {
            lock (ConsoleLock)
            {
                lock (WriteLock)
                {
                    ConsoleOutputCanceller.Cancel();
                    if (Waiting.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> message in Waiting)
                        {
                            WriteInternal(message.Value, message.Key);
                        }
                        Waiting.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Whether to allow the cursor on this console.
        /// IE, should the console show a ">" at the end.
        /// </summary>
        public static bool AllowCursor = true;

        /// <summary>
        /// Generates a standard log file name.
        /// Uses a format of "/logs/(year)/(month)/(day)_(hour)_(min)_(procid).log"
        /// </summary>
        /// <returns>The generated log file name.</returns>
        public static string GenerateLogFileName()
        {
            DateTime DT = DateTime.Now;
            string logfolder = Environment.CurrentDirectory + "/logs/" + DT.Year.ToString().PadLeft(4, '0') + "/" + DT.Month.ToString().PadLeft(2, '0') + "/";
            return logfolder + DT.Day.ToString().PadLeft(2, '0') + "_" + DT.Hour.ToString().PadLeft(2, '0') + "_" + DT.Minute.ToString().PadLeft(2, '0') + "_" + Process.GetCurrentProcess().Id + ".log";
        }

        /// <summary>
        /// The name for the log file to use. Set this BEFORE calling <see cref="Init"/>.
        /// </summary>
        // TODO: Configuration option to change the log file name (with a disable option that routes to a null). With options to put a value like logs/%year%/%month%/%day%.log
        public static string LogFileName = GenerateLogFileName();

        /// <summary>
        /// Prepares the system console.
        /// </summary>
        public static void Init()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Preparing console...");
            ConsoleLock = new Object();
            WriteLock = new Object();
            ConsoleOutputCanceller = new CancellationTokenSource();
            ConsoleOutputThread = new Thread(new ParameterizedThreadStart(ConsoleLoop));
            ConsoleOutputThread.Start(ConsoleOutputCanceller);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFileName));
                FSOUT = File.OpenWrite(LogFileName);
            }
            catch (Exception ex)
            {
                Output(OutputType.WARNING, "Unable to open log file, will not log for this session!");
                Output("Loading Log File", ex);
            }
            Output(OutputType.INIT, "Console prepared...");
            Output(OutputType.INIT, "Test colors: ^r^7Text Colors: ^0^h^1^^n1 ^!^^n! ^2^^n2 ^@^^n@ ^3^^n3 ^#^^n# ^4^^n4 ^$^^n$ ^5^^n5 ^%^^n% ^6^^n6 ^-^^n- ^7^^n7 ^&^^n& ^8^^n8 ^*^^** ^9^^n9 ^(^^n( ^&^h^0^^n0^h ^)^^n) ^a^^na ^A^^nA\n" +
                            "^r^7Text styles: ^b^^nb is bold,^r ^i^^ni is italic,^r ^u^^nu is underline,^r ^s^^ns is strike-through,^r ^O^^nO is overline,^r ^7^h^0^^nh is highlight,^r^7 ^j^^nj is jello (AKA jiggle),^r " +
                            "^7^h^2^e^0^^ne is emphasis,^r^7 ^t^^nt is transparent,^r ^T^^nT is more transparent,^r ^o^^no is opaque,^r ^R^^nR is random,^r ^p^^np is pseudo-random,^r ^^nk is obfuscated (^kobfu^r),^r " +
                            "^^nS is ^SSuperScript^r, ^^nl is ^lSubScript (AKA Lower-Text)^r, ^h^8^d^^nd is Drop-Shadow,^r^7 ^f^^nf is flip,^r ^^nr is regular text, ^^nq is a ^qquote^q, ^^nn is nothing (escape-symbol),^r " +
                            "and ^^nB is base-colors.");
        }

        static FileStream FSOUT = null;

        static void ConsoleLoop(Object obj)
        {
            CancellationTokenSource cts = obj as CancellationTokenSource;
            while (true)
            {
                List<KeyValuePair<string, string>> twaiting;
                lock (ConsoleLock)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }
                    twaiting = new List<KeyValuePair<string, string>>(Waiting);
                    Waiting.Clear();
                }
                if (twaiting.Count > 0)
                {
                    lock (WriteLock)
                    {
                        foreach (KeyValuePair<string, string> message in twaiting)
                        {
                            WriteInternal(message.Value, message.Key);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// The console title.
        /// </summary>
        public static string Title = "";

        /// <summary>
        /// Fixes the title of the system console to how the Client expects it.
        /// </summary>
        public static void FixTitle()
        {
            Title = Program.GameName + " / " + Process.GetCurrentProcess().Id.ToString();
            Console.Title = Title;
        }

#if !LINUX
        // Used for HideConsole() and ShowConsole(), exclusively on Windows.
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        static IntPtr ConsoleHandle;
#endif

        /// <summary>
        /// Hides the system console from view.
        /// Only functions on Windows.
        /// </summary>
        public static void HideConsole()
        {
#if !LINUX
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ShowWindow(ConsoleHandle, SW_HIDE);
            }
#endif
        }

        /// <summary>
        /// Shows (un-hides) the system console.
        /// Only functions on Windows.
        /// </summary>
        public static void ShowConsole()
        {
#if !LINUX
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (ConsoleHandle == null)
                {
                    ConsoleHandle = Process.GetCurrentProcess().MainWindowHandle;
                }
                ShowWindow(ConsoleHandle, SW_SHOW);
            }
#endif
        }

        /// <summary>
        /// Writes a line of colored text to the system console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="bcolor">The base color.</param>
        public static void WriteLine(string text, string bcolor)
        {
            Write(text + "\n", bcolor);
        }

        /// <summary>
        /// Event fired when the console is written to.
        /// </summary>
        public static EventHandler<ConsoleWrittenEventArgs> Written;
        
        /// <summary>
        /// Color symbols ASCII matcher, for <see cref="IsColorSymbol(char)"/>.
        /// </summary>
        public static AsciiMatcher ColorSymbolMatcher = new AsciiMatcher(
            "0123456789" + "ab" + "def" + "hijkl" + "nopqrstu" + "RST" + "#$%&" + "()*" + "A" + "O" + "-" + "!" + "@");

        /// <summary>
        /// Used to identify if an input character is a valid color symbol (generally the character that follows a '^'), for use by RenderColoredText
        /// </summary>
        /// <param name="c"><paramref name="c"/>The character to check.</param>
        /// <returns>whether the character is a valid color symbol.</returns>
        public static bool IsColorSymbol(char c)
        {
            return ColorSymbolMatcher.IsMatch(c);
        }

        /// <summary>
        /// Writes some colored text to the system console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="bcolor">The base color.</param>
        private static void Write(string text, string bcolor)
        {
            lock (ConsoleLock)
            {
                Written?.Invoke(null, new ConsoleWrittenEventArgs() { Text = text, BColor = bcolor });
                Waiting.Add(new KeyValuePair<string, string>(bcolor, text));
            }
        }

        static void WriteInternal(string text, string bcolor)
        {
            text = bcolor + text.ApplyBaseColor(bcolor);
            if (FSOUT != null)
            {
                byte[] b = StringConversionHelper.UTF8Encoding.GetBytes(text);
                FSOUT.Write(b, 0, b.Length);
                FSOUT.Flush(); // TODO: Flush(true)?
            }
            if (AllowCursor)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
            }
            else
            {
                byte[] t = StringConversionHelper.UTF8Encoding.GetBytes(text);
                StringBuilder outp = new StringBuilder(t.Length);
                for (int i = 0; i < t.Length; i++)
                {
                    outp.Append((char)t[i]);
                }
                Console.Write(outp.ToString());
                return;
            }
            StringBuilder outme = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '^' && i + 1 < text.Length && IsColorSymbol(text[i + 1]))
                {
                    if (outme.Length > 0)
                    {
                        Console.Write(outme);
                        outme.Clear();
                    }
                    i++;
                    switch (text[i])
                    {
                        case '0': Console.ForegroundColor = ConsoleColor.Black; break;
                        case '1': Console.ForegroundColor = ConsoleColor.Red; break;
                        case '2': Console.ForegroundColor = ConsoleColor.Green; break;
                        case '3': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case '4': Console.ForegroundColor = ConsoleColor.Blue; break;
                        case '5': Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case '6': Console.ForegroundColor = ConsoleColor.Magenta; break;
                        case '7': Console.ForegroundColor = ConsoleColor.White; break;
                        case '8': Console.ForegroundColor = ConsoleColor.Magenta; break;
                        case '9': Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case 'a': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case ')': Console.ForegroundColor = ConsoleColor.DarkGray; break;
                        case '!': Console.ForegroundColor = ConsoleColor.DarkRed; break;
                        case '@': Console.ForegroundColor = ConsoleColor.DarkGreen; break;
                        case '#': Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                        case '$': Console.ForegroundColor = ConsoleColor.DarkBlue; break;
                        case '%': Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                        case '-': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                        case '&': Console.ForegroundColor = ConsoleColor.Gray; break;
                        case '*': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                        case '(': Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                        case 'A': Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                        case 'b': break;
                        case 'i': break;
                        case 'u': break;
                        case 's': break;
                        case 'O': break;
                        case 'j': break;
                        case 'e': break;
                        case 't': break;
                        case 'T': break;
                        case 'o': break;
                        case 'R': break;
                        case 'p': break; // TODO: Probably shouldn't be implemented, but... it's possible
                        case 'k': break;
                        case 'S': break;
                        case 'l': break;
                        case 'd': break;
                        case 'f': break;
                        case 'n': break;
                        case 'q': outme.Append('"'); break;
                        case 'r': Console.BackgroundColor = ConsoleColor.Black; break;
                        case 'h': Console.BackgroundColor = Console.ForegroundColor; break;
                        default: outme.Append("INVALID-COLOR-CHAR:" + text[i] + "?"); break;
                    }
                }
                else
                {
                    outme.Append(text[i]);
                }
            }
            if (outme.Length > 0)
            {
                Console.Write(outme);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            if (AllowCursor)
            {
                Console.Write(">");// TODO + ConsoleHandler.read);
            }
        }

        /// <summary>
        /// Outputs an exception with a message explaining the source of the exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public static void Output(string message, Exception ex)
        {
            Output(OutputType.ERROR, message + ": " + ex.ToString() + "\n\n" + Environment.StackTrace + "\n\n" + StackNoteHelper.Notes);
        }

        /// <summary>
        /// Outputs an exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void Output(Exception ex)
        {
            Output(OutputType.ERROR, ex.ToString() + "\n\n" + Environment.StackTrace + "\n\n" + StackNoteHelper.Notes);
        }

        /// <summary>
        /// Gets a date-time-string for output.
        /// </summary>
        /// <param name="time">The DateTime.</param>
        /// <returns>A string form of it.</returns>
        public static string DateTimeString(DateTime time)
        {
            return time.Year.ToString().PadLeft(4, '0') + "/" + time.Month.ToString().PadLeft(2, '0') + "/" + time.Day.ToString().PadLeft(2, '0')
                + " " + time.Hour.ToString().PadLeft(2, '0') + ":" + time.Minute.ToString().PadLeft(2, '0') + ":" + time.Second.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// Outputs custom debug information.
        /// </summary>
        /// <param name="type">The custom type.</param>
        /// <param name="message">The message.</param>
        /// <param name="bcolor">The custom base color.</param>
        public static void OutputCustom(string type, string message, string bcolor = "^r^7")
        {
            WriteLine("^r^7" + DateTimeString(DateTime.Now) + " [" + bcolor + type + "^r^7] " + bcolor + message, bcolor);
        }

        /// <summary>
        /// Can be replaced to control whether the SysConsole should output debug data.
        /// </summary>
        public static Func<bool> ShouldOutputDebug = () => true;

        /// <summary>
        /// Properly formats system console output.
        /// </summary>
        /// <param name="ot">What type of output to use.</param>
        /// <param name="text">The text to output.</param>
        /// <param name="bcolor">The base color.</param>
        public static void Output(OutputType ot, string text, string bcolor = null)
        {
            if (ot == OutputType.DEBUG && !ShouldOutputDebug())
            {
                return;
            }
            WriteLine("^r^7" + DateTimeString(DateTime.Now) + " [" + OutputColors[(int)ot] +
                OutputNames[(int)ot] + "^r^7] " + OutputColors[(int)ot] + text, bcolor ?? OutputColors[(int)ot]);
        }

        /// <summary>
        /// Contains the default set of output colors.
        /// </summary>
        public static readonly string[] OutputColors = new string[]
        {
            "^r^7ERROR:OUTPUTTYPE=NONE?",
            "^r^7",
            "^r^2",
            "^r^3",
            "^r^7^h^0",
            "^r^7",
            "^7^&",
            "^r^@"
        };

        /// <summary>
        /// Contains the default set of output type names.
        /// </summary>
        public static readonly string[] OutputNames = new string[]
        {
            "NONE",
            "INFO/CLIENT",
            "INIT",
            "WARNING",
            "ERROR",
            "INFO",
            "DEBUG",
            "INIT/CLIENT"
        };
    }

    /// <summary>
    /// An event arguments for when the <see cref="SysConsole"/> is written to.
    /// </summary>
    public class ConsoleWrittenEventArgs : EventArgs
    {
        /// <summary>
        /// The written text.
        /// </summary>
        public string Text;

        /// <summary>
        /// The base color written with.
        /// </summary>
        public string BColor;
    }

    /// <summary>
    /// All possible console output types.
    /// </summary>
    public enum OutputType : int
    {
        /// <summary>
        /// Do not use.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// When the client is sending information to console.
        /// </summary>
        CLIENTINFO = 1,
        /// <summary>
        /// During the startup sequence.
        /// </summary>
        INIT = 2,
        /// <summary>
        /// An ignorable error.
        /// </summary>
        WARNING = 3,
        /// <summary>
        /// A major error.
        /// </summary>
        ERROR = 4,
        /// <summary>
        /// General information.
        /// </summary>
        INFO = 5,
        /// <summary>
        /// Disable-able minor debug information.
        /// </summary>
        DEBUG = 6,
        /// <summary>
        /// Initialization from the client
        /// </summary>
        CLIENTINIT = 7
        // TODO: More?
    }
}
