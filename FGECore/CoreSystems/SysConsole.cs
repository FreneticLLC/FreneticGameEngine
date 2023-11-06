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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using FGECore.ConsoleHelpers;
using FGECore.FileSystems;
using FGECore.StackNoteSystem;
using FGECore.UtilitySystems;
using FreneticUtilities.FreneticExtensions;

namespace FGECore.CoreSystems;

/// <summary>
/// A helper wrapper for the System Console.
/// TODO: Make this not be purely static based!
/// </summary>
public class SysConsole
{
    /// <summary>Closes the <see cref="SysConsole"/>.</summary>
    public static void ShutDown()
    {
        lock (Internal.ConsoleLock)
        {
            lock (Internal.WriteLock)
            {
                Internal.ConsoleOutputCanceller.Cancel();
                if (Internal.Waiting.Count > 0)
                {
                    Internal.WriteInternal(Internal.GatherText());
                    Internal.Waiting.Clear();
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
        return logfolder + DT.Day.ToString().PadLeft(2, '0') + "_" + DT.Hour.ToString().PadLeft(2, '0') + "_" + DT.Minute.ToString().PadLeft(2, '0') + "_" + Environment.ProcessId + ".log";
    }

    /// <summary>The name for the log file to use. Set this BEFORE calling <see cref="Init"/>.</summary>
    // TODO: Configuration option to change the log file name (with a disable option that routes to a null). With options to put a value like logs/%year%/%month%/%day%.log
    public static string LogFileName = GenerateLogFileName();

    /// <summary>A message to test console colors.</summary>
    public const string TestColorsMessage = "Test colors: ^r^7Text Colors: ^0^h^1^^n1 ^!^^n! ^2^^n2 ^@^^n@ ^3^^n3 ^#^^n# ^4^^n4 ^$^^n$ ^5^^n5 ^%^^n% ^6^^n6 ^-^^n- ^7^^n7 ^&^^n& ^8^^n8 ^*^^** ^9^^n9 ^(^^n( ^&^h^0^^n0^h ^)^^n) ^a^^na ^A^^nA\n" +
                        "^r^7Text styles: ^b^^nb is bold,^r ^i^^ni is italic,^r ^u^^nu is underline,^r ^s^^ns is strike-through,^r ^O^^nO is overline,^r ^7^h^0^^nh is highlight,^r^7 ^j^^nj is jello (AKA jiggle),^r " +
                        "^7^h^2^e^0^^ne is emphasis,^r^7 ^t^^nt is transparent,^r ^T^^nT is more transparent,^r ^o^^no is opaque,^r ^R^^nR is random,^r ^p^^np is pseudo-random,^r ^^nU is unreadable (^Uexample^r),^r " +
                        "^^nS is ^SSuperScript^r, ^^nl is ^lSubScript (AKA Lower-Text)^r, ^h^8^d^^nd is Drop-Shadow,^r^7 ^f^^nf is flip,^r ^^nr is regular text, ^^nq is a ^qquote^q, ^^nn is nothing (escape-symbol),^r " +
                        "and ^^nB is base-colors.";

    /// <summary>Prepares the system console.</summary>
    public static void Init()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Preparing console...");
        Internal.ConsoleLock = new LockObject();
        Internal.WriteLock = new LockObject();
        Internal.ConsoleOutputCanceller = new CancellationTokenSource();
        Internal.ConsoleOutputThread = new Thread(Internal.ConsoleLoop) { Name = "FGE_SysConsole" };
        Internal.ConsoleOutputThread.Start();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFileName));
            Internal.FSOUT = File.OpenWrite(LogFileName);
        }
        catch (Exception ex)
        {
            Output(OutputType.WARNING, "Unable to open log file, will not log for this session!");
            Output("Loading Log File", ex);
        }
        OutputCustom("SysConsole/Core/Init", "Console prepared...");
        OutputCustom("SysConsole/Core/Init", TestColorsMessage);
    }

    /// <summary>Internal handlers for the SysConsole.</summary>
    public static class Internal
    {
        /// <summary>All currently waiting messages.</summary>
        public readonly static List<(string, string)> Waiting = new();

        /// <summary>Locker for the console.</summary>
        public static LockObject ConsoleLock;

        /// <summary>Locker for writing.</summary>
        public static LockObject WriteLock;

        /// <summary>The thread being used for console output.</summary>
        public static Thread ConsoleOutputThread;

        /// <summary>Helper to cancel the console output thread.</summary>
        public static CancellationTokenSource ConsoleOutputCanceller;

        /// <summary>Log file output stream.</summary>
        public static FileStream FSOUT = null;

        /// <summary>Reusable <see cref="StringBuilder"/> for <see cref="GatherText"/>.</summary>
        public static StringBuilder ReusableBuilder = new();

        /// <summary>Converts <see cref="Waiting"/> text to writeable text.</summary>
        public static string GatherText()
        {
            ReusableBuilder.Clear();
            foreach ((string text, string bcolor) in Waiting)
            {
                ReusableBuilder.Append(bcolor).Append(text.ApplyBaseColor(bcolor));
            }
            return ReusableBuilder.ToString();
        }

        /// <summary>Central loop thread for for the console handler.</summary>
        public static void ConsoleLoop()
        {
            while (true)
            {
                Thread.Sleep(100);
                string toWrite;
                lock (ConsoleLock)
                {
                    if (ConsoleOutputCanceller.IsCancellationRequested)
                    {
                        return;
                    }
                    if (Waiting.IsEmpty())
                    {
                        continue;
                    }
                    toWrite = GatherText();
                    Waiting.Clear();
                }
                lock (WriteLock)
                {
                    WriteInternal(toWrite);
                }
            }
        }

        /// <summary>Writes some colored text to the system console.</summary>
        /// <param name="text">The text to write.</param>
        /// <param name="bcolor">The base color.</param>
        public static void Write(string text, string bcolor)
        {
            lock (ConsoleLock)
            {
                Written?.Invoke(null, new ConsoleWrittenEventArgs() { Text = text, BaseColor = bcolor });
                Waiting.Add((text, bcolor));
            }
        }

        /// <summary>Writes some colored text to the system console.</summary>
        /// <param name="text">The text to write.</param>
        public static void WriteInternal(string text)
        {
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
                StringBuilder outp = new(t.Length);
                for (int i = 0; i < t.Length; i++)
                {
                    outp.Append((char)t[i]);
                }
                Console.Write(outp.ToString());
                return;
            }
            StringBuilder outme = new();
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
                        default: outme.Append($"INVALID-COLOR-CHAR:{text[i]}?"); break;
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
    }

    /// <summary>The console title.</summary>
    public static string Title = "";

    /// <summary>Fixes the title of the system console to how launcher clients would expect it.</summary>
    public static void FixTitle()
    {
        Title = $"{Program.GameName} / {Environment.ProcessId}";
        Console.Title = Title;
    }

#if !LINUX
    /// <summary>Internal values only applicable on Windows OS.</summary>
    public static class Internal_Windows
    {
        /// <summary>Used by <see cref="HideConsole"/> and <see cref="ShowConsole"/>.</summary>
        /// <param name="hWnd">Window handle.</param>
        /// <param name="nCmdShow"><see cref="SW_SHOW"/> or <see cref="SW_HIDE"/>.</param>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>Enum constants for <see cref="ShowWindow(IntPtr, int)"/>.</summary>
        public const int SW_HIDE = 0, SW_SHOW = 5;

        /// <summary>Current console window handle, if any.</summary>
        public static IntPtr ConsoleHandle = IntPtr.Zero;
    }
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
            if (Internal_Windows.ConsoleHandle == IntPtr.Zero)
            {
                Internal_Windows.ConsoleHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            Internal_Windows.ShowWindow(Internal_Windows.ConsoleHandle, Internal_Windows.SW_HIDE);
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
            if (Internal_Windows.ConsoleHandle == IntPtr.Zero)
            {
                Internal_Windows.ConsoleHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            Internal_Windows.ShowWindow(Internal_Windows.ConsoleHandle, Internal_Windows.SW_SHOW);
        }
#endif
    }

    /// <summary>Writes a line of colored text to the system console.</summary>
    /// <param name="text">The text to write.</param>
    /// <param name="bcolor">The base color.</param>
    public static void WriteLine(string text, string bcolor)
    {
        Internal.Write(text + "\n", bcolor);
    }

    /// <summary>Event fired when the console is written to.</summary>
    public static EventHandler<ConsoleWrittenEventArgs> Written;

    /// <summary>Color symbols ASCII matcher, for <see cref="IsColorSymbol(char)"/>.</summary>
    public static AsciiMatcher ColorSymbolMatcher = new("0123456789" + "ab" + "def" + "hijkl" + "nopqrstu" + "RST" + "#$%&" + "()*" + "A" + "O" + "-" + "!" + "@");

    /// <summary>Used to identify if an input character is a valid color symbol (generally the character that follows a '^'), for use by RenderColoredText.</summary>
    /// <param name="c">The character to check.</param>
    /// <returns>whether the character is a valid color symbol.</returns>
    public static bool IsColorSymbol(char c)
    {
        return ColorSymbolMatcher.IsMatch(c);
    }

    /// <summary>Outputs an exception with a message explaining the source of the exception.</summary>
    public static void Output(string message, Exception ex)
    {
        Output(OutputType.ERROR, $"{message}: {ex}\n\n{Environment.StackTrace}\n\n{StackNoteHelper.Notes}");
    }

    /// <summary>Outputs an exception.</summary>
    public static void Output(Exception ex)
    {
        Output(OutputType.ERROR, $"{ex}\n\n{Environment.StackTrace}\n\n{StackNoteHelper.Notes}");
    }

    /// <summary>Gets a date-time-string for output.</summary>
    public static string DateTimeString(DateTimeOffset time)
    {
        return time.Year.ToString().PadLeft(4, '0') + "/" + time.Month.ToString().PadLeft(2, '0') + "/" + time.Day.ToString().PadLeft(2, '0')
            + " " + time.Hour.ToString().PadLeft(2, '0') + ":" + time.Minute.ToString().PadLeft(2, '0') + ":" + time.Second.ToString().PadLeft(2, '0');
    }

    /// <summary>Outputs custom debug information.</summary>
    /// <param name="type">The custom type.</param>
    /// <param name="message">The message.</param>
    /// <param name="bcolor">The custom base color.</param>
    public static void OutputCustom(string type, string message, string bcolor = "^r^7")
    {
        WriteLine($"^r^7{DateTimeString(DateTimeOffset.Now)} [{bcolor}{type}^r^7] {bcolor}{message}", bcolor);
    }

    /// <summary>Can be replaced to control whether the SysConsole should output debug data.</summary>
    public static Func<bool> ShouldOutputDebug = () => true;

    /// <summary>Properly formats system console output.</summary>
    /// <param name="outputType">What type of output to use.</param>
    /// <param name="text">The text to output.</param>
    /// <param name="bcolor">The base color (if any).</param>
    public static void Output(OutputType outputType, string text, string bcolor = null)
    {
        if (outputType == OutputType.DEBUG && !ShouldOutputDebug())
        {
            return;
        }
        WriteLine($"^r^7{DateTimeString(DateTimeOffset.Now)} [{outputType.BaseColor}{outputType.Name}^r^7] {outputType.BaseColor}{text}", bcolor ?? outputType.BaseColor);
    }
}

/// <summary>An event arguments class for when the <see cref="SysConsole"/> is written to.</summary>
public class ConsoleWrittenEventArgs : EventArgs
{
    /// <summary>The written text.</summary>
    public string Text;

    /// <summary>The base color written with.</summary>
    public string BaseColor;
}

/// <summary>All possible console output types.</summary>
public class OutputType
{
    /// <summary>When the client is sending information to console.</summary>
    public static OutputType CLIENTINFO = new() { Name = "INFO/CLIENT", BaseColor = TextStyle.Simple };

    /// <summary>General information.</summary>
    public static OutputType SERVERINFO = new() { Name = "INFO/SERVER", BaseColor = TextStyle.Simple };

    /// <summary>During the server startup sequence.</summary>
    public static OutputType SERVERINIT = new() { Name = "INIT/SERVER", BaseColor = "^r^2" };

    /// <summary>Initialization from the client.</summary>
    public static OutputType CLIENTINIT = new() { Name = "INIT/CLIENT", BaseColor = "^r^@" };

    /// <summary>"Out - Good" script output from the client.</summary>
    public static OutputType CLIENTOUTGOOD = new() { Name = "OUT/GOOD/CLIENT", BaseColor = "^r^2" };

    /// <summary>"Out - Bad" script output from the client.</summary>
    public static OutputType CLIENTOUTBAD = new() { Name = "OUT/BAD/CLIENT", BaseColor = "^r^1" };

    /// <summary>"Out - Good" script output from the server.</summary>
    public static OutputType SERVEROUTGOOD = new() { Name = "OUT/GOOD/SERVER", BaseColor = "^r^2" };

    /// <summary>"Out - Bad" script output from the server.</summary>
    public static OutputType SERVEROUTBAD = new() { Name = "OUT/BAD/SERVER", BaseColor = "^r^1" };

    /// <summary>A (probably) ignorable error.</summary>
    public static OutputType WARNING = new() { Name = "WARNING", BaseColor = "^r^3" };

    /// <summary>A major error.</summary>
    public static OutputType ERROR = new() { Name = "ERROR", BaseColor = "^r^7^h^0" };

    /// <summary>Disable-able minor debug information.</summary>
    public static OutputType DEBUG = new() { Name = "DEBUG", BaseColor = "^7^&" };
    // TODO: More?

    /// <summary>The name of the output type.</summary>
    public string Name;

    /// <summary>The base color for this type.</summary>
    public string BaseColor;

    /// <summary>Outputs a message to the <see cref="SysConsole"/>. This is a convenience method equivalent to <see cref="SysConsole.Output(OutputType, string, string)"/>.</summary>
    public void Output(string message)
    {
        SysConsole.Output(this, message, BaseColor);
    }
}
