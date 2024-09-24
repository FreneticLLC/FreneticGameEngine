//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Diagnostics;

namespace FGECore.CoreSystems;

/// <summary>Helper to remap Debug calls to a more appropriate debug responsive.</summary>
public class FGEDebug
{
    /// <summary>Asserts that the given condition is true. Throws an exception if it is false. Only runs on dev builds.</summary>
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Debug Assert Failed: {message}");
        }
    }

    /// <summary>Asserts that the given condition is true. Throws an exception if it is false. Only runs on dev builds.</summary>
    [Conditional("DEBUG")]
    public static void Assert(bool condition)
    {
        Assert(condition, "(No message provided.)");
    }
}
