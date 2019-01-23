//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using FreneticGameCore.Files;
using OpenTK.Graphics.OpenGL4;
using FreneticGameCore.ConsoleHelpers;
using FreneticUtilities.FreneticExtensions;

namespace FreneticGameGraphics.GraphicsHelpers
{
    /// <summary>
    /// The primary engine for shaders.
    /// </summary>
    public class ShaderEngine
    {
        /// <summary>
        /// A full list of currently loaded shaders.
        /// TODO: List->Dictionary?
        /// </summary>
        public List<Shader> LoadedShaders;

        /// <summary>
        /// A common shader that multiplies colors.
        /// </summary>
        public Shader ColorMultShader;

        /// <summary>
        /// A common shader that multiplies colors, explicitly for 2D usage.
        /// </summary>
        public Shader ColorMult2DShader;

        /// <summary>
        /// A common shader that removes black color.
        /// </summary>
        public Shader TextCleanerShader;
        
        /// <summary>
        /// Starts or restarts the shader system.
        /// </summary>
        public void InitShaderSystem()
        {
            // Reset shader list
            LoadedShaders = new List<Shader>();
            // Pregenerate a few needed shader
            ColorMultShader = GetShader("color_mult");
            if (File.Exists("shaders/color_mult2d.vs"))
            {
                ColorMult2DShader = GetShader("color_mult2d");
            }
            TextCleanerShader = GetShader("text_cleaner?text");
        }

        /// <summary>
        /// whether "good graphics" should be enabled for all shaders.
        /// </summary>
        public bool MCM_GOOD_GRAPHICS = true;

        /// <summary>
        /// Updates the shader engine to the new timestamp.
        /// </summary>
        /// <param name="time">The current timestamp.</param>
        public void Update(double time)
        {
            cTime = time;
        }

        /// <summary>
        /// Clears away all shaders.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < LoadedShaders.Count; i++)
            {
                LoadedShaders[i].Original_Program = -1;
                LoadedShaders[i].Internal_Program = -1;
                LoadedShaders[i].Destroy();
            }
            LoadedShaders.Clear();
        }

        /// <summary>
        /// The current tick time.
        /// </summary>
        public double cTime = 1.0;

        /// <summary>
        /// Gets the shader object for a specific shader name.
        /// </summary>
        /// <param name="shadername">The name of the shader.</param>
        /// <returns>A valid shader object.</returns>
        public Shader GetShader(string shadername)
        {
            for (int i = 0; i < LoadedShaders.Count; i++)
            {
                if (LoadedShaders[i].Name == shadername)
                {
                    return LoadedShaders[i];
                }
            }
            Shader Loaded = LoadShader(shadername);
            if (Loaded == null)
            {
                Loaded = new Shader()
                {
                    Name = shadername,
                    Internal_Program = ColorMultShader.Original_Program,
                    Original_Program = ColorMultShader.Original_Program,
                    LoadedProperly = false,
                    Engine = this
                };
            }
            LoadedShaders.Add(Loaded);
            return Loaded;
        }

        /// <summary>
        /// Loads a shader from file.
        /// </summary>
        /// <param name="filename">The name of the file to use.</param>
        /// <returns>The loaded shader, or null if it does not exist.</returns>
        public Shader LoadShader(string filename)
        {
            try
            {
                string oname = filename;
                string[] datg = filename.SplitFast('?', 1);
                string geom = datg.Length > 1 ? datg[1] : null;
                string[] dat1 = datg[0].SplitFast('#', 1);
                string[] vars = new string[0];
                if (dat1.Length == 2)
                {
                    vars = dat1[1].SplitFast(',');
                }
                filename = FileHandler.CleanFileName(dat1[0]);
                if (!File.Exists("shaders/" + filename + ".vs"))
                {
                    SysConsole.Output(OutputType.WARNING, "Cannot load vertex shader, file '" +
                        TextStyle.Standout + "shaders/" + filename + ".vs" + TextStyle.Base +
                        "' does not exist.");
                    return null;
                }
                if (!File.Exists("shaders/" + filename + ".fs"))
                {
                    SysConsole.Output(OutputType.WARNING, "Cannot load fragment shader, file '" +
                        TextStyle.Standout + "shaders/" + filename + ".fs" + TextStyle.Base +
                        "' does not exist.");
                    return null;
                }
                string VS = File.ReadAllText("shaders/" + filename + ".vs");
                string FS = File.ReadAllText("shaders/" + filename + ".fs");
                string GS = null;
                if (geom != null)
                {
                    geom = FileHandler.CleanFileName(geom);
                    if (!File.Exists("shaders/" + geom + ".geom"))
                    {
                        SysConsole.Output(OutputType.WARNING, "Cannot load geomry shader, file '" +
                            TextStyle.Standout + "shaders/" + geom + ".geom" + TextStyle.Base +
                            "' does not exist.");
                        return null;
                    }
                    GS = File.ReadAllText("shaders/" + geom + ".geom");
                }
                return CreateShader(VS, FS, oname, vars, GS);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load shader from filename '" +
                    TextStyle.Standout + "shaders/" + filename + ".fs or .vs" + TextStyle.Base + "': " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Creates a full Shader object for a VS/FS input.
        /// </summary>
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

        /// <summary>
        /// Modifies the shader code string to include any external shaders.
        /// </summary>
        /// <param name="str">The shader code.</param>
        /// <returns>The include-modified shader code.</returns>
        public string Includes(string str)
        {
            if (!str.Contains("#include"))
            {
                return str;
            }
            StringBuilder fsb = new StringBuilder();
            string[] dat = str.Replace("\r", "").Split('\n');
            for (int i = 0; i < dat.Length; i++)
            {
                if (dat[i].StartsWith("#include "))
                {
                    string name = "shaders/" + dat[i].Substring("#include ".Length);
                    name = FileHandler.CleanFileName(name.Trim());
                    if (!File.Exists(name))
                    {
                        throw new Exception("File " + name + " does not exist, but was included by a shader!");
                    }
                    string included = File.ReadAllText(name);
                    fsb.Append(included);
                }
                else
                {
                    fsb.Append(dat[i]);
                }
                fsb.Append('\n');
            }
            return fsb.ToString();
        }

        /// <summary>
        /// Compiles a compute shader by name to a shader.
        /// </summary>
        /// <param name="fname">The file name.</param>
        /// <param name="specialadder">Special additions (EG defines)</param>
        /// <returns>The shader program.</returns>
        public int CompileCompute(string fname, string specialadder = "")
        {
            fname = FileHandler.CleanFileName(fname.Trim());
            string ftxt = Includes(File.ReadAllText("shaders/" + fname + ".comp").Replace("\r\n", "\n").Replace("\r", ""));
            string bf = ftxt.BeforeAndAfter("#version 430 core\n", out string af);
            ftxt = bf + "#version 430 core\n" + specialadder + af;
            int shd = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(shd, ftxt);
            GL.CompileShader(shd);
            string SHD_Info = GL.GetShaderInfoLog(shd);
            GL.GetShader(shd, ShaderParameter.CompileStatus, out int SHD_Status);
            if (SHD_Status != 1)
            {
                throw new Exception("Error creating ComputeShader. Error status: " + SHD_Status + ", info: " + SHD_Info);
            }
            int program = GL.CreateProgram();
            GL.AttachShader(program, shd);
            GL.LinkProgram(program);
            string str = GL.GetProgramInfoLog(program);
            if (str.Length != 0)
            {
                SysConsole.Output(OutputType.INFO, "Linked shader with message: '" + str + "'" + " -- FOR -- " + ftxt);
            }
            GL.DeleteShader(shd);
            GraphicsUtil.CheckError("Shader - Compute - Compile");
            return program;
        }

        /// <summary>
        /// Compiles a VertexShader and FragmentShader to a usable shader program.
        /// </summary>
        /// <param name="VS">The input VertexShader code.</param>
        /// <param name="FS">The input FragmentShader code.</param>
        /// <param name="vars">All variables to include.</param>
        /// <param name="geom">The input GeometryShader code, if any.</param>
        /// <returns>The internal OpenGL program ID.</returns>
        public int CompileToProgram(string VS, string FS, string[] vars, string geom)
        {
            for (int i = 0; i < vars.Length; i++)
            {
                if (vars[i].Length > 0)
                {
                    VS = VS.Replace("#define " + vars[i] + " 0", "#define " + vars[i] + " 1");
                    FS = FS.Replace("#define " + vars[i] + " 0", "#define " + vars[i] + " 1");
                    if (geom != null)
                    {
                        geom = geom.Replace("#define " + vars[i] + " 0", "#define " + vars[i] + " 1");
                    }
                }
            }
            int gObj = -1;
            VS = Includes(VS);
            FS = Includes(FS);
            if (geom != null)
            {
                geom = Includes(geom);
                gObj = GL.CreateShader(ShaderType.GeometryShader);
                GL.ShaderSource(gObj, geom);
                GL.CompileShader(gObj);
                string GS_Info = GL.GetShaderInfoLog(gObj);
                GL.GetShader(gObj, ShaderParameter.CompileStatus, out int GS_Status);
                if (GS_Status != 1)
                {
                    throw new Exception("Error creating GeometryShader. Error status: " + GS_Status + ", info: " + GS_Info);
                }
            }
            int VertexObject = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexObject, VS);
            GL.CompileShader(VertexObject);
            string VS_Info = GL.GetShaderInfoLog(VertexObject);
            GL.GetShader(VertexObject, ShaderParameter.CompileStatus, out int VS_Status);
            if (VS_Status != 1)
            {
                throw new Exception("Error creating VertexShader. Error status: " + VS_Status + ", info: " + VS_Info);
            }
            int FragmentObject = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentObject, FS);
            GL.CompileShader(FragmentObject);
            string FS_Info = GL.GetShaderInfoLog(FragmentObject);
            GL.GetShader(FragmentObject, ShaderParameter.CompileStatus, out int FS_Status);
            if (FS_Status != 1)
            {
                throw new Exception("Error creating FragmentShader. Error status: " + FS_Status + ", info: " + FS_Info);
            }
            int Program = GL.CreateProgram();
            GL.AttachShader(Program, FragmentObject);
            GL.AttachShader(Program, VertexObject);
            if (geom != null)
            {
                GL.AttachShader(Program, gObj);
            }
            GL.LinkProgram(Program);
            string str = GL.GetProgramInfoLog(Program);
            if (str.Length != 0)
            {
                SysConsole.Output(OutputType.INFO, "Linked shader with message: '" + str + "'" + " -- FOR: variables: " + string.Join(",", vars));
            }
            GL.DeleteShader(FragmentObject);
            GL.DeleteShader(VertexObject);
            if (geom != null)
            {
                GL.DeleteShader(gObj);
            }
            GraphicsUtil.CheckError("Shader - Compile");
            return Program;
        }
    }

    /// <summary>
    /// Wraps an OpenGL shader.
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// Constructs an empty shader.
        /// </summary>
        public Shader()
        {
            NewVersion = this;
        }

        /// <summary>
        /// The shader engine that owns this shader.
        /// </summary>
        public ShaderEngine Engine;

        /// <summary>
        /// The name of the shader
        /// </summary>
        public string Name;

        /// <summary>
        /// The shader this shader was remapped to.
        /// </summary>
        public Shader RemappedTo;

        /// <summary>
        /// The internal OpenGL ID for the shader program.
        /// </summary>
        public int Internal_Program;

        /// <summary>
        /// The original OpenGL ID that formed this shader program.
        /// </summary>
        public int Original_Program;

        /// <summary>
        /// All variables on this shader.
        /// </summary>
        public string[] Vars;

        /// <summary>
        /// Whether the shader loaded properly.
        /// </summary>
        public bool LoadedProperly = false;

        /// <summary>
        /// Destroys the OpenGL program that this shader wraps.
        /// </summary>
        public void Destroy()
        {
            if (Original_Program > -1 && GL.IsProgram(Original_Program))
            {
                GL.DeleteProgram(Original_Program);
                Original_Program = -1;
            }
        }

        /// <summary>
        /// Removes the shader from the system.
        /// </summary>
        public void Remove()
        {
            Destroy();
            Engine.LoadedShaders.Remove(this);
        }

        /// <summary>
        /// The tick time this shader was last bound.
        /// </summary>
        public double LastBindTime = 0;

        /// <summary>
        /// A new version of the shader, that replaces this one.
        /// </summary>
        private Shader NewVersion = null;

        /// <summary>
        /// Checks if the shader is valid, and replaces it if needed.
        /// </summary>
        public void CheckValid()
        {
            if (Internal_Program == -1)
            {
                Shader temp = Engine.GetShader(Name);
                Original_Program = temp.Original_Program;
                Internal_Program = Original_Program;
                RemappedTo = temp;
                NewVersion = temp;
            }
            else if (RemappedTo != null)
            {
                RemappedTo.CheckValid();
                Internal_Program = RemappedTo.Original_Program;
            }
        }

        /// <summary>
        /// Binds this shader to OpenGL.
        /// </summary>
        public Shader Bind()
        {
            if (NewVersion != this)
            {
                return NewVersion.Bind();
            }
            LastBindTime = Engine.cTime;
            CheckValid();
            GL.UseProgram(Internal_Program);
            return NewVersion;
        }
    }
}
