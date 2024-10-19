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
using System.Threading.Tasks;

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
