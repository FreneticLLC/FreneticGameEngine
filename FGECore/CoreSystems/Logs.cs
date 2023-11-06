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
