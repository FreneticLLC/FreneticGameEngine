//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using FreneticUtilities.FreneticToolkit;

namespace FGECore.CoreSystems;

/// <summary>Represents the current program abstractly.</summary>
/// <param name="_name">Game name.</param>
/// <param name="_version">Game version.</param>
/// <param name="_versdesc">Game version descriptor.</param>
/// <param name="_author">Game's author label, eg a company name.</param>
public abstract class Program(string _name, string _version, string _versdesc, string _author)
{
    /// <summary>The current program instance.</summary>
    public static Program Instance;

    /// <summary>The original system culture.</summary>
    public static CultureInfo SystemCulture;

    /// <summary>
    /// This method should be called FIRST!
    /// Enforces the correct (Invariant) culture locale setting!
    /// </summary>
    public static void PreInit(Program p)
    {
        Instance = p;
        SystemCulture = SpecialTools.Internationalize();
        Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
        ThreadPool.GetMinThreads(out int work, out int comp);
        if (work < 256 || comp < 256)
        {
            ThreadPool.SetMinThreads(256, 256);
        }
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, e) =>
        {
            Console.WriteLine($"Unhandled exception: {e}");
        });
    }

    /// <summary>The name of this program.</summary>
    public readonly string Name = _name;

    /// <summary>The version (number) of this program.</summary>
    public readonly string Version = _version;

    /// <summary>The version description of this program.</summary>
    public readonly string VersionDescription = _versdesc;

    /// <summary>The short label for the author of this program.</summary>
    public readonly string Author = _author;

    /// <summary>The current program's game name.</summary>
    public static string GameName => Instance.Name;

    /// <summary>The current program's game version.</summary>
    public static string GameVersion => Instance.Version;

    /// <summary>
    /// The current program's game version description.
    /// (EG "Release", "Beta", or "Alpha" usually).
    /// </summary>
    public static string GameVersionDescription => Instance.VersionDescription;

    /// <summary>The current program's author label.</summary>
    public static string GameAuthor => Instance.Author;
}
