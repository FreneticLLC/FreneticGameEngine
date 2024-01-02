//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FGECore;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.GraphicsHelpers.Shaders;

/// <summary>The primary engine for shaders.</summary>
public class ShaderEngine
{
    /// <summary>A full list of currently loaded shaders.</summary>
    public Dictionary<string, Shader> LoadedShaders;

    /// <summary>A cache of shader file text (post-includes).</summary>
    public Dictionary<string, string> ShaderFilesCache;

    /// <summary>A common shader that multiplies colors.</summary>
    public Shader ColorMultShader;

    /// <summary>A common shader that multiplies colors, explicitly for 2D usage.</summary>
    public Shader ColorMult2DShader;

    /// <summary>A common shader that removes black color.</summary>
    public Shader TextCleanerShader;

    /// <summary>The backing file engine.</summary>
    public FileEngine Files;

    /// <summary>Starts or restarts the shader system.</summary>
    /// <param name="files">The backing file engine.</param>
    public void InitShaderSystem(FileEngine files)
    {
        Files = files;
        // Reset shader list
        LoadedShaders = new Dictionary<string, Shader>(128);
        ShaderFilesCache = new Dictionary<string, string>(256);
        // Pregenerate a few needed shader
        ColorMultShader = GetShader("color_mult");
        if (Files.FileExists("shaders/color_mult2d.vs"))
        {
            ColorMult2DShader = GetShader("color_mult2d");
        }
        TextCleanerShader = GetShader("text_cleaner?text");
    }

    /// <summary>
    /// whether "good graphics" should be enabled for all shaders.
    /// TODO: Autoset this value (should be false for old intel graphics or similar).
    /// </summary>
    public bool MCM_GOOD_GRAPHICS = true;

    /// <summary>Updates the shader engine to the new timestamp.</summary>
    /// <param name="time">The current timestamp.</param>
    public void Update(double time)
    {
        cTime = time;
    }

    /// <summary>Clears away all shaders.</summary>
    public void Clear()
    {
        foreach (Shader shader in LoadedShaders.Values)
        {
            shader.Original_Program = -1;
            shader.Internal_Program = -1;
            shader.Destroy();
        }
        LoadedShaders.Clear();
        ShaderFilesCache.Clear();
    }

    /// <summary>Tries to get the text of a shader file, automatically handling the file cache.</summary>
    /// <param name="filename">The name of the shader file.</param>
    /// <param name="text">The text gotten (or null if none).</param>
    /// <returns>True if successful, false if file does not exist.</returns>
    public bool TryGetShaderFileText(string filename, out string text)
    {
        filename = FileEngine.CleanFileName(filename.Trim());
        if (ShaderFilesCache.TryGetValue(filename, out text))
        {
            return true;
        }
        if (!Files.TryReadFileText(filename, out string newData))
        {
            return false;
        }
        if (ShaderFilesCache.Count > 128) // TODO: Configurable?
        {
            ShaderFilesCache.Clear();
        }
        newData = Includes(filename, newData.Replace("\r\n", "\n").Replace("\r", ""));
        ShaderFilesCache[filename] = newData;
        text = newData;
        return true;
    }

    /// <summary>The current tick time.</summary>
    public double cTime = 1.0;

    /// <summary>
    /// Gets the shader object for a specific shader name.
    /// If the relevant shader exists but is not yet loaded, will load it from file.
    /// </summary>
    /// <param name="shadername">The name of the shader.</param>
    /// <returns>A valid shader object.</returns>
    public Shader GetShader(string shadername)
    {
        if (LoadedShaders.TryGetValue(shadername, out Shader foundShader))
        {
            return foundShader;
        }
        Shader Loaded = LoadShader(shadername);
        Loaded ??= new Shader()
            {
                Name = shadername,
                Internal_Program = ColorMultShader.Original_Program,
                Original_Program = ColorMultShader.Original_Program,
                LoadedProperly = false,
                Engine = this
            };
        LoadedShaders.Add(shadername, Loaded);
        return Loaded;
    }

    /// <summary>
    /// Loads a shader from file.
    /// <para>Note: Most users should not use this method. Instead, use <see cref="GetShader(string)"/>.</para>
    /// </summary>
    /// <param name="filename">The name of the file to use.</param>
    /// <returns>The loaded shader, or null if it does not exist.</returns>
    public Shader LoadShader(string filename)
    {
        try
        {
            string originalName = filename;
            string[] shaderVariables = filename.SplitFast('?', 1); // TODO: Less ridiculous system
            string geomFilename = shaderVariables.Length > 1 ? shaderVariables[1] : null;
            string[] dat1 = shaderVariables[0].SplitFast('#', 1);
            string[] vars = [];
            if (dat1.Length == 2)
            {
                vars = dat1[1].SplitFast(',');
            }
            filename = FileEngine.CleanFileName(dat1[0]);
            filename = $"shaders/{filename}";
            if (!TryGetShaderFileText(filename + ".vs", out string VS))
            {
                Logs.Warning($"Cannot load vertex shader, file '{TextStyle.Standout}{filename}.vs{TextStyle.Base}' does not exist.");
                return null;
            }
            if (!TryGetShaderFileText(filename + ".fs", out string FS))
            {
                Logs.Warning($"Cannot load fragment shader, file '{TextStyle.Standout}{filename}.fs{TextStyle.Base}' does not exist.");
                return null;
            }
            string GS = null;
            if (geomFilename != null)
            {
                geomFilename = FileEngine.CleanFileName(geomFilename);
                geomFilename = $"shaders/{geomFilename}.geom";
                if (!TryGetShaderFileText(geomFilename, out GS))
                {
                    Logs.Warning($"Cannot load geometry shader, file '{TextStyle.Standout}{geomFilename}{TextStyle.Base}' does not exist.");
                    return null;
                }
            }
            return CreateShader(VS, FS, originalName, vars, GS);
        }
        catch (Exception ex)
        {
            Logs.Error($"Failed to load shader from filename '{TextStyle.Standout}{filename}.fs or .vs{TextStyle.Base}': {ex}");
            return null;
        }
    }

    /// <summary>Creates a full Shader object for a VS/FS input.</summary>
    /// <param name="VS">The input VertexShader code.</param>
    /// <param name="FS">The input FragmentShader code.</param>
    /// <param name="name">The name of the shader.</param>
    /// <param name="vars">The variables to use.</param>
    /// <param name="geom">The geometry shader, if any.</param>
    /// <returns>A valid Shader object.</returns>
    public Shader CreateShader(string VS, string FS, string name, string[] vars, string geom)
    {
        int Program = CompileToProgram(VS, FS, vars, geom);
        return new Shader()
        {
            Name = name,
            LoadedProperly = true,
            Internal_Program = Program,
            Original_Program = Program,
            Vars = vars,
            Engine = this
        };
    }

    /// <summary>Processes "#define" lines in a shader.</summary>
    /// <param name="originalText">The shader text.</param>
    /// <param name="defValues">The define-value map.</param>
    /// <returns>The processed shader text.</returns>
    public static string PatchDefs(string originalText, Dictionary<string, string> defValues)
    {
        if (!originalText.Contains("#define"))
        {
            return originalText;
        }
        StringBuilder fullFileText = new(originalText.Length + defValues.Count);
        string[] dat = originalText.Replace("\r", "").Split('\n');
        for (int i = 0; i < dat.Length; i++)
        {
            if (dat[i].StartsWith("#define "))
            {
                string defined = dat[i]["#define ".Length..];
                string name = defined.Before(" ");
                if (defValues.TryGetValue(name, out string newValue))
                {
                    fullFileText.Append("#define ").Append(name).Append(' ').Append(newValue);
                }
                else
                {
                    fullFileText.Append(dat[i]);
                }
            }
            else
            {
                fullFileText.Append(dat[i]);
            }
            fullFileText.Append('\n');
        }
        return fullFileText.ToString();
    }

    /// <summary>Modifies the shader code string to include any external shaders.</summary>
    /// <param name="filename">The name of the shader file processing includes.</param>
    /// <param name="str">The shader code.</param>
    /// <returns>The include-modified shader code.</returns>
    public string Includes(string filename, string str)
    {
        if (!str.Contains("#include"))
        {
            return str;
        }
        StringBuilder fullFileText = new(str.Length * 2);
        string[] dat = str.Replace("\r", "").Split('\n');
        for (int i = 0; i < dat.Length; i++)
        {
            if (dat[i].StartsWith("#include "))
            {
                string includeFilename = dat[i]["#include ".Length..];
                includeFilename = $"shaders/{includeFilename}";
                if (!TryGetShaderFileText(includeFilename, out string included))
                {
                    throw new Exception($"File '{includeFilename}' does not exist, but was included by shader '{filename}'!");
                }
                fullFileText.Append(included);
            }
            else
            {
                fullFileText.Append(dat[i]);
            }
            fullFileText.Append('\n');
        }
        return fullFileText.ToString();
    }

    const string FILE_START = "#version 430 core\n";

    /// <summary>Compiles a compute shader by name to a shader.</summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="specialadder">Special additions (EG defines)</param>
    /// <returns>The shader program.</returns>
    public int CompileCompute(string fileName, string specialadder = "")
    {
        fileName = FileEngine.CleanFileName(fileName.Trim());
        fileName = $"shaders/{fileName}.comp";
        if (!TryGetShaderFileText(fileName, out string fileText))
        {
            throw new Exception($"Compute shader file '{fileName}' does not exist.");
        }
        int index = fileText.IndexOf(FILE_START, StringComparison.Ordinal);
        if (index < 0)
        {
            index = 0;
        }
        else
        {
            index += FILE_START.Length;
        }
        fileText = fileText.Insert(index, specialadder);
        int shaderObject = GL.CreateShader(ShaderType.ComputeShader);
        GL.ShaderSource(shaderObject, fileText);
        GL.CompileShader(shaderObject);
        string SHD_Info = GL.GetShaderInfoLog(shaderObject);
        GL.GetShader(shaderObject, ShaderParameter.CompileStatus, out int SHD_Status);
        if (SHD_Status != 1)
        {
#if DEBUG
            File.WriteAllText($"temp_shd_{fileName}.txt", fileText);
#endif
            throw new Exception($"Error creating ComputeShader '{fileName}'. Error status: {SHD_Status}, info: {SHD_Info}");
        }
        int program = GL.CreateProgram();
        GL.AttachShader(program, shaderObject);
        GL.LinkProgram(program);
        string str = GL.GetProgramInfoLog(program);
        if (str.Length != 0)
        {
            Logs.ClientInfo($"Linked shader '{fileName}' with message: '{str}' -- FOR -- {fileText}");
        }
        GL.DeleteShader(shaderObject);
        GraphicsUtil.CheckError("Shader - Compute - Compile");
        return program;
    }

    private readonly Dictionary<string, string> ReusableDefValues = new(128);

    /// <summary>Compiles a VertexShader and FragmentShader to a usable shader program.</summary>
    /// <param name="VS">The input VertexShader code.</param>
    /// <param name="FS">The input FragmentShader code.</param>
    /// <param name="vars">All variables to include.</param>
    /// <param name="GS">The input GeometryShader code, if any.</param>
    /// <returns>The internal OpenGL program ID.</returns>
    public int CompileToProgram(string VS, string FS, string[] vars, string GS)
    {
        GraphicsUtil.CheckError("Shader - BeforeCompile");
        if (vars.Length > 0)
        {
            ReusableDefValues.Clear();
            for (int i = 0; i < vars.Length; i++)
            {
                if (vars[i].Length > 0)
                {
                    ReusableDefValues.Add(vars[i], "1");
                }
            }
            VS = PatchDefs(VS, ReusableDefValues);
            FS = PatchDefs(FS, ReusableDefValues);
            if (GS != null)
            {
                GS = PatchDefs(GS, ReusableDefValues);
            }
        }
        int geomObject = -1;
        if (GS != null)
        {
            geomObject = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geomObject, GS);
            GL.CompileShader(geomObject);
            string GS_Info = GL.GetShaderInfoLog(geomObject);
            GL.GetShader(geomObject, ShaderParameter.CompileStatus, out int GS_Status);
            if (GS_Status != 1)
            {
                throw new Exception($"Error creating GeometryShader. Error status: {GS_Status}, info: {GS_Info}");
            }
        }
        int VertexObject = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexObject, VS);
        GL.CompileShader(VertexObject);
        string VS_Info = GL.GetShaderInfoLog(VertexObject);
        GL.GetShader(VertexObject, ShaderParameter.CompileStatus, out int VS_Status);
        if (VS_Status != 1)
        {
            throw new Exception($"Error creating VertexShader. Error status: {VS_Status}, info: {VS_Info}");
        }
        int FragmentObject = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentObject, FS);
        GL.CompileShader(FragmentObject);
        string FS_Info = GL.GetShaderInfoLog(FragmentObject);
        GL.GetShader(FragmentObject, ShaderParameter.CompileStatus, out int FS_Status);
        if (FS_Status != 1)
        {
            throw new Exception($"Error creating FragmentShader. Error status: {FS_Status}, info: {FS_Info}");
        }
        int Program = GL.CreateProgram();
        GL.AttachShader(Program, FragmentObject);
        GL.AttachShader(Program, VertexObject);
        if (GS != null)
        {
            GL.AttachShader(Program, geomObject);
        }
        GL.LinkProgram(Program);
        string str = GL.GetProgramInfoLog(Program);
        if (str.Length != 0)
        {
            Logs.ClientInfo($"Linked shader with message: '{str}' -- FOR: variables: {vars.JoinString(",")}");
        }
        GL.DeleteShader(FragmentObject);
        GL.DeleteShader(VertexObject);
        if (GS != null)
        {
            GL.DeleteShader(geomObject);
        }
        GraphicsUtil.CheckError("Shader - Compile");
        return Program;
    }
}
