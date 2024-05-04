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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.StackNoteSystem;
using FGEGraphics.GraphicsHelpers.Shaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers;

/// <summary>Helper class for graphical systems.</summary>
public static class GraphicsUtil
{
    /// <summary>Checks errors when debug is enabled.</summary>
    /// <param name="callerLocationLabel">A simple text string describing the source calling location.</param>
    /// <param name="context">An optional context object.</param>
    [Conditional("DEBUG")]
    public static void CheckError(string callerLocationLabel, object context = null)
    {
        ErrorCode ec = GL.GetError();
        if (ec == ErrorCode.NoError)
        {
            return;
        }
        StringBuilder errorMessage = new();
        string contextText = context is null ? "" : $"(context=`{context}`): ";
        errorMessage.Append($"OpenGL error [{callerLocationLabel} {contextText} (bound shader=`{ShaderEngine.BoundNow?.Name ?? "(none)"}`)]: ");
        while (ec != ErrorCode.NoError)
        {
            errorMessage.Append($"{ec}");
            ec = GL.GetError();
            if (ec != ErrorCode.NoError)
            {
                errorMessage.Append(", ");
            }
        }
        errorMessage.Append($"\n{StackNoteHelper.Notes}\n{Environment.StackTrace}");
        Logs.Error($"{errorMessage}");
    }

}
