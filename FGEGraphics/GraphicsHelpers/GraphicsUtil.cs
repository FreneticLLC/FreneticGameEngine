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
using System.Threading;
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
    /// <summary>Initialize the graphics utility.</summary>
    public static void Init()
    {
        GraphicsThreadID = Environment.CurrentManagedThreadId;
    }

    /// <summary>Checks errors when debug is enabled.</summary>
    /// <param name="callerLocationLabel">A simple text string describing the source calling location.</param>
    /// <param name="context">An optional context object.</param>
    [Conditional("DEBUG")]
    public static void CheckError(string callerLocationLabel, object context = null)
    {
        if (Environment.CurrentManagedThreadId != GraphicsThreadID)
        {
            Logs.CriticalError($"OpenGL call made from non-graphics thread! (Thread '{Thread.CurrentThread.Name}'/{Environment.CurrentManagedThreadId} vs expected {GraphicsThreadID})");
        }
        ErrorCode ec = GL.GetError();
        if (ec == ErrorCode.NoError)
        {
            return;
        }
        StringBuilder errorMessage = new();
        string contextText = context is null ? "" : $"(context=`{context}`): ";
        errorMessage.Append($"OpenGL error [{callerLocationLabel} {contextText}(bound shader=`{BoundShader}`=`{ShaderEngine.BoundNow?.Name ?? "(none)"}`, texture=`{BoundTexture}`)]: ");
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

    /// <summary>Map of all current active/allocated textures to their source strings.</summary>
    public static Dictionary<uint, string> ActiveTextures = [];

    /// <summary>Map of all current active/allocated shaders to their source strings.</summary>
    public static Dictionary<int, string> ActiveShaders = [];

    /// <summary>ID of the thread that OpenGL is expected to run on.</summary>
    public static long GraphicsThreadID;

    /// <summary>Which shader ID is currently bound, or 0 if none.</summary>
    public static int BoundShader = 0;

    /// <summary>Which texture ID is currently bound, or 0 if none.</summary>
    public static uint BoundTexture = 0;
#endif

    /// <summary>Generates a new buffer. Equivalent to <see cref="GL.GenBuffers(int, out uint)"/> with a count of 1. Also immediately binds the buffer.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    /// <param name="target">What buffer target to bind the buffer to.</param>
    public static uint GenBuffer(string source, BufferTarget target)
    {
        GL.GenBuffers(1, out uint buffer);
        GL.BindBuffer(target, buffer);
#if DEBUG
        ActiveBuffers[buffer] = source;
        CheckError("GraphicsUtil GenBuffer", source);
#endif
        LabelObject(ObjectLabelIdentifier.Buffer, buffer, $"FGEBuffer_{source}");
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
        CheckError("GraphicsUtil GenBuffers", source);
    }

    /// <summary>Deletes a buffer. Equivalent to <see cref="GL.DeleteBuffer(uint)"/>.</summary>
    public static void DeleteBuffer(uint buffer)
    {
        GL.DeleteBuffer(buffer);
#if DEBUG
        if (!ActiveBuffers.Remove(buffer))
        {
            throw new Exception($"Attempted to delete non-tracked buffer ID {buffer}!");
        }
        CheckError("GraphicsUtil DeleteBuffer", buffer);
#endif
    }

    /// <summary>Creates multiple tracked buffers. Does not bind.</summary>
    /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
    /// <param name="count">How many to generate.</param>
    /// <param name="target">What buffer target to bind the buffer to.</param>
    public static TrackedBuffer[] CreateBuffers(string source, int count, BufferTarget target)
    {
        uint[] buffers = new uint[count];
        GenBuffers(source, count, buffers, target);
        TrackedBuffer[] tracked = new TrackedBuffer[count];
        for (int i = 0; i < count; i++)
        {
            tracked[i] = new TrackedBuffer(source, target, buffers[i]);
        }
        return tracked;
    }

    /// <summary>Represents a buffer in a trackable, single-disposable way.</summary>
    public class TrackedBuffer
    {
        /// <summary>Create a new tracked buffer, binds it immediately.</summary>
        /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
        /// <param name="target">What buffer target to bind the buffer to.</param>
        public TrackedBuffer(string source, BufferTarget target)
        {
            Source = source;
            Target = target;
            ID = GenBuffer(source, target);
        }

        /// <summary>Create a tracked buffer from an existing buffer ID.</summary>
        /// <param name="source">A string that identifies the source of this buffer, for debugging usage.</param>
        /// <param name="target">What buffer target to bind the buffer to.</param>
        /// <param name="existingID">The existing buffer ID.</param>
        public TrackedBuffer(string source, BufferTarget target, uint existingID)
        {
            ID = existingID;
            Target = target;
            Source = source;
        }

        /// <summary>The buffer ID.</summary>
        public uint ID;

        /// <summary>If true, the buffer is generated. If false, it is gone.</summary>
        public bool IsValid = true;

        /// <summary>The buffer target.</summary>
        public BufferTarget Target;

        /// <summary>The source string.</summary>
        public string Source;

        /// <summary>Binds the buffer to its tracked target.</summary>
        public void Bind()
        {
            if (!IsValid)
            {
                throw new Exception($"Attempted to bind an invalid (already disposed) buffer: {Source}!");
            }
            GL.BindBuffer(Target, ID);
            CheckError("TrackedBuffer Bind", this);
        }

        /// <summary>Dipose the buffer.</summary>
        public void Dispose()
        {
            if (IsValid)
            {
                DeleteBuffer(ID);
                IsValid = false;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"TrackedBuffer(ID={ID}, Source=`{Source}`, Target={Target}, IsValid={IsValid})";
        }
    }

    /// <summary>Represents a texture in a trackable, single-disposable way. Also immediately binds the texture.</summary>
    /// <param name="source">A short string that identifies the source of this texture, for debugging usage.</param>
    /// <param name="target">What texture target to bind the texture to.</param>
    public class TrackedTexture(string source, TextureTarget target)
    {
        /// <summary>The texture target.</summary>
        public TextureTarget Target = target;

        /// <summary>The source string.</summary>
        public string Source = source;

        /// <summary>The texture ID.</summary>
        public uint ID = GenTexture(source, target);

        /// <summary>Binds the texture to its tracked target.</summary>
        public void Bind()
        {
            if (!IsValid)
            {
                throw new Exception($"Attempted to bind an invalid (already disposed) texture: {Source}!");
            }
            BindTexture(Target, ID);
        }

        /// <summary>Binds the texture to an alternate target (eg TextureBuffer).</summary>
        /// <param name="alternateTarget">The alternate target to use.</param>
        public void Bind(TextureTarget alternateTarget)
        {
            if (!IsValid)
            {
                throw new Exception($"Attempted to bind an invalid (already disposed) texture: {Source}!");
            }
            BindTexture(alternateTarget, ID);
        }

        /// <summary>If true, the texture is generated. If false, it is gone.</summary>
        public bool IsValid = true;

        /// <summary>Dipose the texture.</summary>
        public void Dispose()
        {
            if (IsValid)
            {
                DeleteTexture(ID);
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
        CheckError("GraphicsUtil GenVertexArray", source);
#endif
        return array;
    }

    /// <summary>Deletes a vertex array. Equivalent to <see cref="GL.DeleteVertexArray(uint)"/>.</summary>
    public static void DeleteVertexArray(uint buffer)
    {
        GL.DeleteVertexArray(buffer);
#if DEBUG
        if (!ActiveVertexArrays.Remove(buffer))
        {
            throw new Exception($"Attempted to delete non-tracked vertex array ID {buffer}!");
        }
        CheckError("GraphicsUtil DeleteVertexArray", buffer);
#endif
    }

    /// <summary>Binds shader buffer object data directly, equivalent to <code>BindBuffer(target, id); BufferData(target, data, etc); BindBuffer(0);</code></summary>
    public static void BindBufferData<T>(BufferTarget bufferTarget, TrackedBuffer buffer, T[] data, BufferUsageHint hint) where T : unmanaged
    {
        BindBufferData(bufferTarget, buffer, data, data.Length, hint);
    }

    /// <summary>Binds shader buffer object data directly, equivalent to <code>BindBuffer(target, id); BufferData(target, data, etc); BindBuffer(0);</code></summary>
    public static unsafe void BindBufferData<T>(BufferTarget bufferTarget, TrackedBuffer buffer, T[] data, int len, BufferUsageHint hint) where T: unmanaged
    {
#if DEBUG
        if (buffer is null || !buffer.IsValid)
        {
            throw new Exception($"Attempted to bind data to an invalid (already disposed) buffer: {buffer?.Source ?? "null"}!");
        }
#endif
        GL.BindBuffer(bufferTarget, buffer.ID);
        GL.BufferData(bufferTarget, len * sizeof(T), data, hint); // sizeof(T) is 'unsafe' because it's not compile time known, but *is* JIT-Compile time known
        GL.BindBuffer(bufferTarget, 0);
    }

    /// <summary>Binds shader buffer object data directly as null/empty, equivalent to <code>BindBuffer(target, id); BufferData(target, 0, IntPtr.Zero, etc); BindBuffer(0);</code></summary>
    public static void BindBufferDataEmpty(BufferTarget bufferTarget, TrackedBuffer buffer, int len, BufferUsageHint hint)
    {
#if DEBUG
        if (buffer is null || !buffer.IsValid)
        {
            throw new Exception($"Attempted to bind data to an invalid (already disposed) buffer: {buffer?.Source ?? "null"}!");
        }
#endif
        GL.BindBuffer(bufferTarget, buffer.ID);
        GL.BufferData(bufferTarget, len, IntPtr.Zero, hint);
        GL.BindBuffer(bufferTarget, 0);
    }

    /// <summary>Generates a new texture. Equivalent to <see cref="GL.GenTextures(int, out uint)"/> with a count of 1. This will also immediately bind the texture.</summary>
    /// <param name="source">A short string that identifies the source of this texture, for debugging usage.</param>
    /// <param name="target">Target to bind it to.</param>
    public static uint GenTexture(string source, TextureTarget target)
    {
        GL.GenTextures(1, out uint texture);
#if DEBUG
        ActiveTextures[texture] = source;
#endif
        BindTexture(target, texture);
        LabelObject(ObjectLabelIdentifier.Texture, texture, $"FGETexture_{source}");
        CheckError("GraphicsUtil GenTexture", source);
        return texture;
    }

    /// <summary>Deletes an existing texture.</summary>
    /// <param name="texture">The texture ID to delete.</param>
    public static void DeleteTexture(uint texture)
    {
        GL.DeleteTexture(texture);
#if DEBUG
        if (!ActiveTextures.Remove(texture))
        {
            throw new Exception($"Attempted to delete non-tracked texture ID {texture}!");
        }
        if (texture == BoundTexture)
        {
            throw new Exception($"Attempted to delete currently bound texture ID {texture}!");
        }
        CheckError("GraphicsUtil DeleteTexture", texture);
#endif
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
        CheckError("GraphicsUtil Pre-Label");
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

    /// <summary>Create a new shader object.</summary>
    /// <param name="type">The type of shader object to create.</param>
    /// <param name="label">The label to give to the new shader.</param>
    /// <param name="sourceText">The source text of the shader to compile.</param>
    public static int CreateShader(ShaderType type, string label, string sourceText)
    {
        int shaderObject = GL.CreateShader(type);
        LabelObject(ObjectLabelIdentifier.Shader, (uint)shaderObject, label);
        CheckError("GraphicsUtil CreateShader", label);
        GL.ShaderSource(shaderObject, sourceText);
        GL.CompileShader(shaderObject);
        string info = GL.GetShaderInfoLog(shaderObject);
        GL.GetShader(shaderObject, ShaderParameter.CompileStatus, out int status);
        if (status != 1)
        {
            throw new Exception($"Error creating {type} '{label}'. Error status: {status}, info: {info}");
        }
        return shaderObject;
    }

    /// <summary>Create a new shader program.</summary>
    /// <param name="label">The label to give to the new shader.</param>
    public static int CreateProgram(string label)
    {
        int program = GL.CreateProgram();
        LabelObject(ObjectLabelIdentifier.Program, (uint)program, label);
#if DEBUG
        ActiveShaders[program] = label;
        CheckError("GraphicsUtil CreateProgram", label);
#endif
        return program;
    }

    /// <summary>Bind a shader program.</summary>
    /// <param name="source">A short string that identifies the source of this shader bind, for debugging usage.</param>
    /// <param name="shader">The shader to bind.</param>
    public static void UseProgram(string source, int shader)
    {
#if DEBUG
        ShaderEngine.BoundNow = null;
        if (shader < 0)
        {
            throw new Exception($"Attempted to use invalid shader program ID {shader}!");
        }
        if (shader != 0 && !ActiveShaders.ContainsKey(shader))
        {
            throw new Exception($"Attempted to bind non-tracked shader ID {shader}!");
        }
        BoundShader = shader;
#endif
        GL.UseProgram(shader);
        CheckError("GraphicsUtil UseProgram", shader);
    }

    /// <summary>Binds a texture.</summary>
    /// <param name="target">The texture target to bind to, such as Texture2D.</param>
    /// <param name="texture">The texture ID to bind.</param>
    public static void BindTexture(TextureTarget target, uint texture)
    {
#if DEBUG
        if (texture != 0 && !ActiveTextures.ContainsKey(texture))
        {
            throw new Exception($"Attempted to bind non-tracked texture ID {texture}!");
        }
        BoundTexture = texture;
#endif
        GL.BindTexture(target, texture);
        CheckError("GraphicsUtil BindTexture");
    }
}
