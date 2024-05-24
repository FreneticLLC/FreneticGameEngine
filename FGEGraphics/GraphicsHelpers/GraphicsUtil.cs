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
using FGECore.StackNoteSystem;
using FGEGraphics.GraphicsHelpers.Shaders;
using OpenTK.Graphics.OpenGL4;

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

#if DEBUG
    /// <summary>Map of all current active/allocated buffers (VBOs) to their source strings.</summary>
    public static Dictionary<uint, string> ActiveBuffers = [];

    /// <summary>Map of all current active/allocated vertex arrays (VAOs) to their source strings.</summary>
    public static Dictionary<uint, string> ActiveVertexArrays = [];
#endif

    /// <summary>Generates a new buffer. Equivalent to <see cref="GL.GenBuffers(int, out uint)"/> with a count of 1.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    public static uint GenBuffer(string source)
    {
        GL.GenBuffers(1, out uint buffer);
#if DEBUG
        ActiveBuffers[buffer] = source;
        CheckError($"GraphicsUtil GenBuffer", source);
#endif
        return buffer;
    }

    /// <summary>Generates new buffers. Equivalent to <see cref="GL.GenBuffers(int, uint[])"/>.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    /// <param name="count">How many to generate.</param>
    /// <param name="arr">Where to store them.</param>
    public static void GenBuffers(string source, int count, uint[] arr)
    {
        GL.GenBuffers(count, arr);
#if DEBUG
        for (int i = 0; i < count; i++)
        {
            ActiveBuffers[arr[i]] = source;
        }
        CheckError($"GraphicsUtil GenBuffers", source);
#endif
    }

    /// <summary>Deletes a buffer. Equivalent to <see cref="GL.DeleteBuffer(uint)"/>.</summary>
    public static void DeleteBuffer(uint buffer)
    {
        GL.DeleteBuffer(buffer);
#if DEBUG
        ActiveBuffers.Remove(buffer);
        CheckError($"GraphicsUtil DeleteBuffer", buffer);
#endif
    }

    /// <summary>Represents a buffer in a trackable, single-dispoable way.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    public class TrackedBuffer(string source)
    {
        /// <summary>The buffer ID.</summary>
        public uint ID = GenBuffer(source);

        /// <summary>If true, the buffer is generated. If false, it is gone.</summary>
        public bool IsValid = true;

        /// <summary>Dipose the buffer.</summary>
        public void Dispose()
        {
            if (IsValid)
            {
                DeleteBuffer(ID);
                IsValid = false;
            }
        }
    }

    /// <summary>Generates a new vertex array. Equivalent to <see cref="GL.GenVertexArrays(int, out uint)"/> with a count of 1.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    public static uint GenVertexArray(string source)
    {
        GL.GenVertexArrays(1, out uint buffer);
#if DEBUG
        ActiveVertexArrays[buffer] = source;
        CheckError($"GraphicsUtil GenVertexArray", source);
#endif
        return buffer;
    }

    /// <summary>Deletes a vertex array. Equivalent to <see cref="GL.DeleteVertexArray(uint)"/>.</summary>
    public static void DeleteVertexArray(uint buffer)
    {
        GL.DeleteVertexArray(buffer);
#if DEBUG
        ActiveVertexArrays.Remove(buffer);
        CheckError($"GraphicsUtil DeleteVertexArray", buffer);
#endif
    }

    /// <summary>Binds shader buffer object data directly, equivalent to <code>BindBuffer(target, id); BufferData(target, data, etc); BindBuffer(0);</code></summary>
    public static void BindBufferData<T>(BufferTarget bufferTarget, uint bufferId, T[] data, BufferUsageHint hint) where T : unmanaged
    {
        BindBufferData(bufferTarget, bufferId, data, data.Length, hint);
    }

    /// <summary>Binds shader buffer object data directly, equivalent to <code>BindBuffer(target, id); BufferData(target, data, etc); BindBuffer(0);</code></summary>
    public static unsafe void BindBufferData<T>(BufferTarget bufferTarget, uint bufferId, T[] data, int len, BufferUsageHint hint) where T: unmanaged
    {
        GL.BindBuffer(bufferTarget, bufferId);
        GL.BufferData(bufferTarget, len * sizeof(T), data, hint); // sizeof(T) is 'unsafe' because it's not compile time known, but *is* JIT-Compile time known
        GL.BindBuffer(bufferTarget, 0);
    }

    /// <summary>Binds shader buffer object data directly as null/empty, equivalent to <code>BindBuffer(target, id); BufferData(target, 0, IntPtr.Zero, etc); BindBuffer(0);</code></summary>
    public static void BindBufferDataEmpty(BufferTarget bufferTarget, uint bufferId, int len, BufferUsageHint hint)
    {
        GL.BindBuffer(bufferTarget, bufferId);
        GL.BufferData(bufferTarget, len, IntPtr.Zero, hint);
        GL.BindBuffer(bufferTarget, 0);
    }
}
