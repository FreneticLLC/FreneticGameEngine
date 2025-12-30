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
using FGECore.StackNoteSystem;
using FreneticUtilities.FreneticExtensions;

namespace FGECore.CoreSystems;

/// <summary>Convenience class for quick-logging to console.</summary>
public class Logs
{
    /// <summary>Disable-able minor debug information.</summary>
    public static void Debug(string message)
    {
        OutputType.DEBUG.Output(message);
    }

    /// <summary>A major error.</summary>
    public static void Error(string message)
    {
        OutputType.ERROR.Output(message);
    }

    /// <summary>A major error, with a stack trace.</summary>
    public static void CriticalError(string message)
    {
        string[] stack = Environment.StackTrace.Split('\n');
        int skip = 0;
        if (stack.Length > 1 && stack[1].Trim() == "at System.Environment.get_StackTrace()")
        {
            skip = 1;
            if (stack.Length > 2 && stack[2].Trim().StartsWith("at FGECore.CoreSystems.Logs.CriticalError("))
            {
                skip = 2;
            }
        }
        OutputType.ERROR.Output($"{message}\n{stack.Skip(skip).JoinString("\n")}\n\n{StackNoteHelper.Notes}");
    }

    /// <summary>A major error, with a stack trace.</summary>
    public static void CriticalError(string message, Exception ex)
    {
        if (ex is ThreadAbortException)
        {
            // Shouldn't be possible, but has happened before, so...
            throw ex;
        }
        OutputType.ERROR.Output($"{message}: {ex}\n\n{StackNoteHelper.Notes}");
    }

    /// <summary>A (probably) ignorable error.</summary>
    public static void Warning(string message)
    {
        OutputType.WARNING.Output(message);
    }

    /// <summary>Initialization from the client.</summary>
    public static void ClientInit(string message)
    {
        OutputType.CLIENTINIT.Output(message);
    }

    /// <summary>During the server startup sequence.</summary>
    public static void ServerInit(string message)
    {
        OutputType.SERVERINIT.Output(message);
    }

    /// <summary>General information from the server.</summary>
    public static void ServerInfo(string message)
    {
        OutputType.SERVERINFO.Output(message);
    }

    /// <summary>General information from the client.</summary>
    public static void ClientInfo(string message)
    {
        OutputType.CLIENTINFO.Output(message);
    }
}
