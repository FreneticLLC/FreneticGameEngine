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
        errorMessage.Append($"OpenGL error [{callerLocationLabel} {contextText}(bound shader=`{ShaderEngine.BoundNow?.Name ?? "(none)"}`)]: ");
        while (ec != ErrorCode.NoError)
        {
            errorMessage.Append($"{ec}");
            ec = GL.GetError();
            if (ec != ErrorCode.NoError)
            {
                errorMessage.Append(", ");
            }
        }
        Logs.CriticalError($"{errorMessage}");
    }

    /// <summary>Only if DEBUG flag is set, assert that a value must be true, error if it is not.</summary>
    /// <param name="mustBeTrue">The value that must be true.</param>
    /// <param name="message">The message if it is not true.</param>
    [Conditional("DEBUG")]
    public static void DebugAssert(bool mustBeTrue, string message)
    {
        if (!mustBeTrue)
        {
            Logs.CriticalError($"Assertion failed: {message}");
            CheckError("GraphicsUtil DebugAssert");
            throw new Exception($"Assertion failed: {message}");
        }
    }

#if DEBUG
    /// <summary>Map of all current active/allocated buffers (VBOs) to their source strings.</summary>
    public static Dictionary<uint, string> ActiveBuffers = [];

    /// <summary>Map of all current active/allocated vertex arrays (VAOs) to their source strings.</summary>
    public static Dictionary<uint, string> ActiveVertexArrays = [];
#endif

    /// <summary>Generates a new buffer. Equivalent to <see cref="GL.GenBuffers(int, out uint)"/> with a count of 1. Also immediately binds the buffer.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    /// <param name="target">What buffer target to bind the buffer to.</param>
    public static uint GenBuffer(string source, BufferTarget target)
    {
        GL.GenBuffers(1, out uint buffer);
        GL.BindBuffer(target, buffer);
        LabelObject(ObjectLabelIdentifier.Buffer, buffer, $"FGEBuffer_{source}");
#if DEBUG
        ActiveBuffers[buffer] = source;
        CheckError($"GraphicsUtil GenBuffer", source);
#endif
        return buffer;
    }

    /// <summary>Generates new buffers. Equivalent to <see cref="GL.GenBuffers(int, uint[])"/>. Also immediately binds the buffers, leaving the first one bound.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    /// <param name="count">How many to generate.</param>
    /// <param name="arr">Where to store them.</param>
    /// <param name="target">What buffer target to bind the buffer to.</param>
    public static void GenBuffers(string source, int count, uint[] arr, BufferTarget target)
    {
        if (count <= 0)
        {
            return;
        }
        GL.GenBuffers(count, arr);
        for (int i = count - 1; i >= 0; i--)
        {
            GL.BindBuffer(target, arr[i]);
            LabelObject(ObjectLabelIdentifier.Buffer, arr[i], $"FGEBuffer_{source}_{i}");
#if DEBUG
            ActiveBuffers[arr[i]] = source;
#endif
        }
        CheckError($"GraphicsUtil GenBuffers", source);
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

    /// <summary>Represents a buffer in a trackable, single-disposable way. Also immediately binds the buffer.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    /// <param name="target">What buffer target to bind the buffer to.</param>
    public class TrackedBuffer(string source, BufferTarget target)
    {
        /// <summary>The buffer ID.</summary>
        public uint ID = GenBuffer(source, target);

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

    /// <summary>Generates a new vertex array. Equivalent to <see cref="GL.GenVertexArrays(int, out uint)"/> with a count of 1. Also immediately binds the array.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    public static uint GenVertexArray(string source)
    {
        GL.GenVertexArrays(1, out uint array);
        GL.BindVertexArray(array);
        LabelObject(ObjectLabelIdentifier.VertexArray, array, $"FGEVertArr_{source}");
#if DEBUG
        ActiveVertexArrays[array] = source;
        CheckError($"GraphicsUtil GenVertexArray", source);
#endif
        return array;
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

    /// <summary>Generates a new texture. Equivalent to <see cref="GL.GenTextures(int, out uint)"/> with a count of 1. This will also immediately bind the texture.</summary>
    /// <param name="source">A short string that identifies the source of this texture, for debugging usage.</param>
    /// <param name="target">Target to bind it to.</param>
    public static uint GenTexture(string source, TextureTarget target)
    {
        GL.GenTextures(1, out uint texture);
        GL.BindTexture(target, texture);
        LabelObject(ObjectLabelIdentifier.Texture, texture, $"FGETexture_{source}");
        CheckError($"GraphicsUtil GenTexture", source);
        return texture;
    }

    /// <summary>The max label length for OpenGL's Object Labels.</summary>
    public static int MaxLabelLength = -1;

    /// <summary>Apply an OpenGL debugging label to an object.
    /// <para>Note that "Gen(X)" calls do not "Create" an object, they just reserve a name. You cannot label an uncreated object - you must cause it to be created (eg bind it).</para></summary>
    /// <param name="type">Namespace to categorize the object.</param>
    /// <param name="obj">The object itself.</param>
    /// <param name="label">The label text. Keep it short.</param>
    public static void LabelObject(ObjectLabelIdentifier type, uint obj, string label)
    {
        if (MaxLabelLength < 0)
        {
            MaxLabelLength = GL.GetInteger(GetPName.MaxLabelLength);
        }
        if (label.Length >= MaxLabelLength)
        {
            label = label[0..(MaxLabelLength - 1)]; // 1 short for null term
        }
        CheckError($"GraphicsUtil Pre-Label");
        GL.ObjectLabel(type, obj, label.Length, label);
        CheckError($"GraphicsUtil Do Label {label.Length}/{MaxLabelLength} == {label}");
    }

    /// <summary>Sets the current Texture2D texture's Min/Mag filters to Linear, and WrapS/T to ClampToEdge.</summary>
    public static void TexParamLinearClamp()
    {
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
    }
}
