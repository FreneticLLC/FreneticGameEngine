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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using FreneticGameCore.Files;
using FreneticGameGraphics.ClientSystem;
using FreneticGameCore.ConsoleHelpers;
using FreneticUtilities.FreneticExtensions;
using FreneticGameCore.ModelSystems;

namespace FreneticGameGraphics.GraphicsHelpers
{
    /// <summary>
    /// System to help with models.
    /// </summary>
    public class ModelEngine
    {
        /// <summary>
        /// All currently loaded models.
        /// </summary>
        public Dictionary<string, Model> LoadedModels;

        /// <summary>
        /// Internal model helper from the core.
        /// </summary>
        public ModelHandler Handler;

        /// <summary>
        /// A cube.
        /// </summary>
        public Model Cube;

        /// <summary>
        /// A cylinder.
        /// </summary>
        public Model Cylinder;

        /// <summary>
        /// A sphere.
        /// </summary>
        public Model Sphere;

        /// <summary>
        /// The client engine.
        /// </summary>
        public GameClientWindow TheClient;

        /// <summary>
        /// Prepares the model system.
        /// </summary>
        /// <param name="engine">Animation engine.</param>
        /// <param name="tclient">Backing client.</param>
        public void Init(AnimationEngine engine, GameClientWindow tclient)
        {
            TheClient = tclient;
            AnimEngine = engine;
            Handler = new ModelHandler();
            LoadedModels = new Dictionary<string, Model>(128);
            Cube = GenerateCube();
            LoadedModels.Add("cube", Cube);
            Cylinder = GetModel("cylinder");
            Sphere = GetModel("sphere");
        }

        /// <summary>
        /// Generates a cube model.
        /// </summary>
        /// <returns>The cube model.</returns>
        public Model GenerateCube()
        {
            Model m = new Model("cube")
            {
                Engine = this,
                Skinned = true,
                ModelMin = new BEPUutilities.Vector3(-1, -1, -1),
                ModelMax = new BEPUutilities.Vector3(1, 1, 1)
            };
            ModelMesh mm = new ModelMesh("cube");
            Renderable.ListBuilder builder = new Renderable.ListBuilder();
            builder.Prepare();
            TextureCoordinates tc = new TextureCoordinates();
            builder.AddSide(Location.UnitX, tc, offs: true);
            builder.AddSide(Location.UnitY, tc, offs: true);
            builder.AddSide(Location.UnitZ, tc, offs: true);
            builder.AddSide(-Location.UnitX, tc, offs: true);
            builder.AddSide(-Location.UnitY, tc, offs: true);
            builder.AddSide(-Location.UnitZ, tc, offs: true);
            m.Original = new Model3D();
            Model3DMesh m3m = new Model3DMesh()
            {
                Name = "cube"
            };
            m3m.Indices = builder.Indices.ConvertAll((u) => (int)u);
            m3m.Vertices = builder.Vertices.ConvertAll((o) => o.ToLocation().ToBVector());
            m3m.TexCoords = builder.TexCoords.ConvertAll((o) => new BEPUutilities.Vector2(o.X, o.Y));
            m3m.Normals = builder.Normals.ConvertAll((o) => o.ToLocation().ToBVector());
            m.Original.Meshes = new List<Model3DMesh>() { m3m };
            mm.vbo.GenerateVBO(builder);
            m.AddMesh(mm);
            return m;
        }

        /// <summary>
        /// Update for delta.
        /// </summary>
        /// <param name="time">The new noted time.</param>
        public void Update(double time)
        {
            cTime = time;
        }

        /// <summary>
        /// Current known time.
        /// </summary>
        public double cTime = 0;

        /// <summary>
        /// Loads a model by name.
        /// </summary>
        /// <param name="filename">The name.</param>
        /// <returns>The model.</returns>
        public Model LoadModel(string filename)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!TheClient.Files.Exists("models/" + filename + ".vmd"))
                {
                    SysConsole.Output(OutputType.WARNING, "Cannot load model, file '" +
                        TextStyle.Standout + "models/" + filename + ".vmd" + TextStyle.Base +
                        "' does not exist.");
                    return null;
                }
                Model m = FromBytes(filename, TheClient.Files.ReadBytes("models/" + filename + ".vmd", out PakkedFile fref));
                if (m != null)
                {
                    m.FileRef = fref;
                }
                return m;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load model from filename '" +
                    TextStyle.Standout + "models/" + filename + ".vmd" + TextStyle.Error + "': " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the model object for a specific model name.
        /// </summary>
        /// <param name="modelname">The name of the model.</param>
        /// <returns>A valid model object.</returns>
        public Model GetModel(string modelname)
        {
            modelname = FileHandler.CleanFileName(modelname);
            if (LoadedModels.TryGetValue(modelname, out Model existingModel))
            {
                return existingModel;
            }
            Model Loaded = null;
            try
            {
                Loaded = LoadModel(modelname);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
            if (Loaded == null)
            {
                if (norecurs)
                {
                    Loaded = new Model(modelname) { Engine = this, Root = Matrix4.Identity, Meshes = new List<ModelMesh>(), RootNode = null };
                    Loaded.AutoMapMeshes();
                }
                else
                {
                    norecurs = true;
                    Model m = Cube;
                    norecurs = false;
                    Loaded = new Model(modelname) { Engine = this, Root = m.Root, RootNode = m.RootNode, Meshes = m.Meshes, MeshMap = m.MeshMap, Original = m.Original };
                }
            }
            LoadedModels.Add(modelname, Loaded);
            return Loaded;
        }

        /// <summary>
        /// Helper to prevent recursion.
        /// </summary>
        bool norecurs = false;

        /// <summary>
        /// Backing animation engine.
        /// </summary>
        public AnimationEngine AnimEngine;

        /// <summary>
        /// loads a model from a file byte array.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        /// <param name="data">The .obj file string.</param>
        /// <returns>A valid model.</returns>
        public Model FromBytes(string name, byte[] data)
        {
            Model3D scene = Handler.LoadModel(data);
            return FromScene(scene, name, AnimEngine);
        }

        /// <summary>
        /// Converts a core Scene to a renderable model.
        /// </summary>
        /// <param name="scene">The backing model.</param>
        /// <param name="name">The name to use.</param>
        /// <param name="engine">The animation engine.</param>
        /// <returns>The model.</returns>
        public Model FromScene(Model3D scene, string name, AnimationEngine engine)
        {
            if (scene.Meshes.Count == 0)
            {
                throw new Exception("Scene has no meshes! (" + name + ")");
            }
            Model model = new Model(name)
            {
                Engine = this,
                Original = scene,
                Root = Convert(scene.MatrixA)
            };
            foreach (Model3DMesh mesh in scene.Meshes)
            {
                if (mesh.Name.ToLowerFast().Contains("collision") || mesh.Name.ToLowerFast().Contains("norender"))
                {
                    continue;
                }
                ModelMesh modmesh = new ModelMesh(mesh.Name);
                Renderable vbo = modmesh.vbo;
                bool hastc = mesh.TexCoords.Count == mesh.Vertices.Count;
                bool hasn = mesh.Normals.Count == mesh.Vertices.Count;
                if (!hasn)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no normals! (" + name + ")");
                }
                if (!hastc)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no texcoords! (" + name + ")");
                }
                Renderable.ArrayBuilder builder = new Renderable.ArrayBuilder();
                builder.Prepare(mesh.Vertices.Count, mesh.Indices.Count);
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    builder.Vertices[i] = mesh.Vertices[i].ToOpenTK();
                    if (!hastc)
                    {
                        builder.TexCoords[i] = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        builder.TexCoords[i] = new Vector3((float)mesh.TexCoords[i].X, 1 - (float)mesh.TexCoords[i].Y, 0);
                    }
                    if (!hasn)
                    {
                        builder.Normals[i] = new Vector3(0f, 0f, 1f);
                    }
                    else
                    {
                        builder.Normals[i] = mesh.Normals[i].ToOpenTK();
                    }
                    builder.Colors[i] = new Vector4(1, 1, 1, 1); // TODO: From the mesh?
                }
                for (int i = 0; i < mesh.Indices.Count; i++)
                {
                    builder.Indices[i] = (uint)mesh.Indices[i];
                }
                int bc = mesh.Bones.Count;
                if (bc > 200)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has " + bc + " bones! (" + name + ")");
                    bc = 200;
                }
                int[] pos = new int[builder.Vertices.Length];
                for (int i = 0; i < bc; i++)
                {
                    for (int x = 0; x < mesh.Bones[i].Weights.Count; x++)
                    {
                        int IDa = mesh.Bones[i].IDs[x];
                        float Weighta = (float)mesh.Bones[i].Weights[x];
                        int spot = pos[IDa]++;
                        if (spot > 7)
                        {
                            //SysConsole.Output(OutputType.WARNING, "Too many bones influencing " + vw.VertexID + "!");
                            ForceSet(builder.BoneWeights, IDa, 3, builder.BoneWeights[IDa][3] + Weighta);
                        }
                        else if (spot > 3)
                        {
                            ForceSet(builder.BoneIDs2, IDa, spot - 4, i);
                            ForceSet(builder.BoneWeights2, IDa, spot - 4, Weighta);
                        }
                        else
                        {
                            ForceSet(builder.BoneIDs, IDa, spot, i);
                            ForceSet(builder.BoneWeights, IDa, spot, Weighta);
                        }
                    }
                }
                vbo.GenerateVBO(builder);
                model.AddMesh(modmesh);
            }
            model.RootNode = new ModelNode() { Parent = null, Name = scene.RootNode.Name.ToLowerFast() };
            List<ModelNode> allNodes = new List<ModelNode>();
            PopulateChildren(model.RootNode, scene.RootNode, model, engine, allNodes);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                for (int x = 0; x < scene.Meshes[i].Bones.Count; x++)
                {
                    ModelNode nodet = null;
                    string nl = scene.Meshes[i].Bones[x].Name.ToLowerFast();
                    for (int n = 0; n < allNodes.Count; n++)
                    {
                        if (allNodes[n].Name == nl)
                        {
                            nodet = allNodes[n];
                            break;
                        }
                    }
                    ModelBone mb = new ModelBone() { Offset = Convert(scene.Meshes[i].Bones[x].MatrixA) };
                    nodet.Bones.Add(mb);
                    model.Meshes[i].Bones.Add(mb);
                }
            }
            return model;
        }

        /// <summary>
        /// Converts a BEPU matrix to OpenTK.
        /// </summary>
        /// <param name="mat">The matrix.</param>
        /// <returns>The matrix.</returns>
        Matrix4 Convert(BEPUutilities.Matrix mat)
        {
            return mat.Convert();
        }

        /// <summary>
        /// Populates the children of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="orin">The original node.</param>
        /// <param name="model">The model.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="allNodes">All current nodes.</param>
        void PopulateChildren(ModelNode node, Model3DNode orin, Model model, AnimationEngine engine, List<ModelNode> allNodes)
        {
            allNodes.Add(node);
            if (engine.HeadBones.Contains(node.Name))
            {
                node.Mode = 0;
            }
            else if (engine.LegBones.Contains(node.Name))
            {
                node.Mode = 2;
            }
            else
            {
                node.Mode = 1;
            }
            for (int i = 0; i < orin.Children.Count; i++)
            {
                ModelNode child = new ModelNode() { Parent = node, Name = orin.Children[i].Name.ToLowerFast() };
                PopulateChildren(child, orin.Children[i], model, engine, allNodes);
                node.Children.Add(child);
            }
        }

        /// <summary>
        /// Foce-sets a vector subindex in a list.
        /// </summary>
        /// <param name="vecs">The vector list.</param>
        /// <param name="ind">The index.</param>
        /// <param name="subind">The vector subindex.</param>
        /// <param name="val">The new value.</param>
        void ForceSet(Vector4[] vecs, int ind, int subind, float val)
        {
            Vector4 vec = vecs[ind];
            vec[subind] = val;
            vecs[ind] = vec;
        }

    }

    /// <summary>
    /// Represents a 3D model.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// The original core model.
        /// </summary>
        public Model3D Original;

        /// <summary>
        /// Constructs the model.
        /// </summary>
        /// <param name="_name">The name.</param>
        public Model(string _name)
        {
            Name = _name;
            Meshes = new List<ModelMesh>();
        }

        /// <summary>
        /// The file that was used to load this model. Can be null for manually-generated textures.
        /// </summary>
        public PakkedFile FileRef;

        /// <summary>
        /// The root transform.
        /// </summary>
        public Matrix4 Root;

        /// <summary>
        /// The name of  this model.
        /// </summary>
        public string Name;

        /// <summary>
        /// LOD helper data.
        /// </summary>
        public KeyValuePair<int, int>[] LODHelper = null;

        /// <summary>
        /// The LOD box.
        /// </summary>
        public AABB LODBox = default;

        /// <summary>
        /// All the meshes this model has.
        /// </summary>
        public List<ModelMesh> Meshes;

        /// <summary>
        /// A map of mesh names to meshes for this model.
        /// </summary>
        public Dictionary<string, ModelMesh> MeshMap;

        /// <summary>
        /// The root node.
        /// </summary>
        public ModelNode RootNode;

        /// <summary>
        /// Whether the model bounds are set and known.
        /// </summary>
        public bool ModelBoundsSet = false;

        /// <summary>
        /// The minimum model bound.
        /// </summary>
        public BEPUutilities.Vector3 ModelMin;

        /// <summary>
        /// The maximum model bound.
        /// </summary>
        public BEPUutilities.Vector3 ModelMax;

        /// <summary>
        /// Adds a mesh to this model.
        /// </summary>
        /// <param name="mesh">The mesh to add.</param>
        public void AddMesh(ModelMesh mesh)
        {
            Meshes.Add(mesh);
            MeshMap[mesh.Name] = mesh;
        }

        /// <summary>
        /// Automatically builds the <see cref="MeshMap"/>.
        /// </summary>
        public void AutoMapMeshes()
        {
            MeshMap = new Dictionary<string, ModelMesh>(Meshes.Count * 2);
            foreach (ModelMesh mesh in Meshes)
            {
                MeshMap[mesh.Name] = mesh;
            }
        }

        /// <summary>
        /// Gets a mesh by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The mesh.</returns>
        public ModelMesh MeshFor(string name)
        {
            name = name.ToLowerFast();
            if (MeshMap.TryGetValue(name, out ModelMesh mesh))
            {
                return mesh;
            }
            for (int i = 0; i < Meshes.Count; i++)
            {
                // TODO: Is StartsWith needed here?
                if (Meshes[i].Name.StartsWith(name))
                {
                    return Meshes[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the bones to an array value.
        /// </summary>
        /// <param name="mats">The relevant array.</param>
        public void SetBones(Matrix4[] mats)
        {
            float[] set = new float[mats.Length * 16];
            for (int i = 0; i < mats.Length; i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        set[i * 16 + x * 4 + y] = mats[i][x, y];
                    }
                }
            }
            GL.UniformMatrix4(101, mats.Length, false, set);
        }

        /// <summary>
        /// Clears up the bones to identity.
        /// </summary>
        public void BoneSafe()
        {
            Matrix4 ident = Matrix4.Identity;
            GL.UniformMatrix4(100, false, ref ident);
            Matrix4[] mats = new Matrix4[] { ident };
            SetBones(mats);
        }

        /// <summary>
        /// Any custom animation adjustments on this model.
        /// </summary>
        public Dictionary<string, Matrix4> CustomAnimationAdjustments = new Dictionary<string, Matrix4>();

        /// <summary>
        /// Force bones not to offset.
        /// </summary>
        public bool ForceBoneNoOffset = false;

        /// <summary>
        /// Update transformations on the model.
        /// </summary>
        /// <param name="pNode">The previous node.</param>
        /// <param name="transf">The current transform.</param>
        public void UpdateTransforms(ModelNode pNode, Matrix4 transf)
        {
            string nodename = pNode.Name;
            Matrix4 nodeTransf = Matrix4.Identity;
            SingleAnimationNode pNodeAnim = FindNodeAnim(nodename, pNode.Mode, out double time);
            if (pNodeAnim != null)
            {
                BEPUutilities.Vector3 vec = pNodeAnim.LerpPos(time);
                BEPUutilities.Quaternion quat = pNodeAnim.LerpRotate(time);
                OpenTK.Quaternion oquat = new OpenTK.Quaternion((float)quat.X, (float)quat.Y, (float)quat.Z, (float)quat.W);
                Matrix4.CreateTranslation((float)vec.X, (float)vec.Y, (float)vec.Z, out Matrix4 trans);
                trans.Transpose();
                Matrix4.CreateFromQuaternion(ref oquat, out Matrix4 rot);
                if (CustomAnimationAdjustments.TryGetValue(nodename, out Matrix4 r2))
                {
                    rot *= r2;
                }
                rot.Transpose();
                Matrix4.Mult(ref trans, ref rot, out nodeTransf);
            }
            else
            {
                if (CustomAnimationAdjustments.TryGetValue(nodename, out Matrix4 temp))
                {
                    temp.Transpose();
                    nodeTransf = temp;
                }
            }
            Matrix4.Mult(ref transf, ref nodeTransf, out Matrix4 global);
            for (int i = 0; i < pNode.Bones.Count; i++)
            {
                if (ForceBoneNoOffset)
                {
                    pNode.Bones[i].Transform = global;
                }
                else
                {
                    Matrix4.Mult(ref global, ref pNode.Bones[i].Offset, out pNode.Bones[i].Transform);
                }
            }
            for (int i = 0; i < pNode.Children.Count; i++)
            {
                UpdateTransforms(pNode.Children[i], global);
            }
        }

        /// <summary>
        /// The backing model engine.
        /// </summary>
        public ModelEngine Engine = null;

        /// <summary>
        /// Finds an animation node.
        /// </summary>
        /// <param name="nodeName">The node name.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="time">The time stamp result.</param>
        /// <returns>The node.</returns>
        SingleAnimationNode FindNodeAnim(string nodeName, int mode, out double time)
        {
            SingleAnimation nodes;
            if (mode == 0)
            {
                nodes = hAnim;
                time = aTHead;
            }
            else if (mode == 1)
            {
                nodes = tAnim;
                time = aTTorso;
            }
            else
            {
                nodes = lAnim;
                time = aTLegs;
            }
            if (nodes == null)
            {
                return null;
            }
            return nodes.GetNode(nodeName);
        }

        /// <summary>
        /// Head animation.
        /// </summary>
        SingleAnimation hAnim;

        /// <summary>
        /// Torso animation.
        /// </summary>
        SingleAnimation tAnim;

        /// <summary>
        /// Legs animation.
        /// </summary>
        SingleAnimation lAnim;

        /// <summary>
        /// Head animation time.
        /// </summary>
        double aTHead;

        /// <summary>
        /// Torso animation time.
        /// </summary>
        double aTTorso;

        /// <summary>
        /// Legs animation time.
        /// </summary>
        double aTLegs;

        /// <summary>
        /// The timestamp this model was last drawn at.
        /// </summary>
        public double LastDrawTime;

        /// <summary>
        /// Draws the model with low level of detail.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="view">The relevant view helper.</param>
        public void DrawLOD(Location pos, View3D view)
        {
            if (LODHelper == null)
            {
                return;
            }
            Vector3 wid = (LODBox.Max - LODBox.Min).ToOpenTK();
            Vector3 vpos = (pos - view.RenderRelative).ToOpenTK() + new Vector3(0f, 0f, wid.Z * 0.5f);
            Vector3 offs = new Vector3(-0.5f, -0.5f, 0f);
            Matrix4 off1 = Matrix4.CreateTranslation(offs);
            //Matrix4 off2 = Matrix4.CreateTranslation(-offs);
            //Engine.TheClient.Rendering.SetMinimumLight(1f);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[0].Key);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[0].Value);
            Engine.TheClient.Rendering3D.RenderRectangle3D(off1 * Matrix4.CreateScale(wid.X, wid.Z, 1f) * Matrix4.CreateRotationX((float)Math.PI * 0.5f) * Matrix4.CreateRotationZ((float)Math.PI * 0.25f) * Matrix4.CreateTranslation(vpos));
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[1].Key);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[1].Value);
            Engine.TheClient.Rendering3D.RenderRectangle3D(off1 * Matrix4.CreateScale(wid.X, wid.Z, 1f) * Matrix4.CreateRotationX((float)Math.PI * 0.5f) * Matrix4.CreateRotationZ((float)Math.PI * 0.75f) * Matrix4.CreateTranslation(vpos));
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[2].Key);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[2].Value);
            Engine.TheClient.Rendering3D.RenderRectangle3D(off1 * Matrix4.CreateScale(wid.Y, wid.Z, 1f) * Matrix4.CreateRotationX((float)Math.PI * 0.5f) * Matrix4.CreateRotationZ((float)Math.PI * -0.25f) * Matrix4.CreateTranslation(vpos));
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[3].Key);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[3].Value);
            Engine.TheClient.Rendering3D.RenderRectangle3D(off1 * Matrix4.CreateScale(wid.Y, wid.Z, 1f) * Matrix4.CreateRotationX((float)Math.PI * 0.5f) * Matrix4.CreateRotationZ((float)Math.PI * -0.75f) * Matrix4.CreateTranslation(vpos));
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[4].Key);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[4].Value);
            Engine.TheClient.Rendering3D.RenderRectangle3D(off1 * Matrix4.CreateScale(wid.Z, wid.X, 1f) * Matrix4.CreateTranslation(vpos));
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[5].Key);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, LODHelper[5].Value);
            Engine.TheClient.Rendering3D.RenderRectangle3D(off1 * Matrix4.CreateScale(wid.Z, wid.X, 1f) * Matrix4.CreateRotationX((float)Math.PI) * Matrix4.CreateTranslation(vpos));
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            //Engine.TheClient.Rendering.SetMinimumLight(0f);
        }

        /// <summary>
        /// Draws the model.
        /// </summary>
        /// <param name="aTimeHead">Animation time, head.</param>
        /// <param name="aTimeLegs">Animation time, legs.</param>
        /// <param name="aTimeTorso">Aniamtion time, torso.</param>
        /// <param name="forceBones">Whether to force bone setting.</param>
        /// <param name="headanim">Head animation.</param>
        /// <param name="legsanim">Legs animation.</param>
        /// <param name="torsoanim">Torso animation.</param>
        public void Draw(double aTimeHead = 0, SingleAnimation headanim = null, double aTimeTorso = 0, SingleAnimation torsoanim = null, double aTimeLegs = 0, SingleAnimation legsanim = null, bool forceBones = false)
        {
            LastDrawTime = Engine.cTime;
            hAnim = headanim;
            tAnim = torsoanim;
            lAnim = legsanim;
            bool any = hAnim != null || tAnim != null || lAnim != null || forceBones;
            if (any)
            {
                // globalInverse = Root.Inverted();
                aTHead = aTimeHead;
                aTTorso = aTimeTorso;
                aTLegs = aTimeLegs;
                UpdateTransforms(RootNode, Matrix4.Identity);
            }
            // TODO: If hasBones && !any { defaultBones() } ?
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (any && Meshes[i].Bones.Count > 0)
                {
                    Matrix4[] mats = new Matrix4[Meshes[i].Bones.Count];
                    for (int x = 0; x < Meshes[i].Bones.Count; x++)
                    {
                        mats[x] = Meshes[i].Bones[x].Transform;
                    }
                    SetBones(mats);
                }
                Meshes[i].Draw();
            }
        }

        /// <summary>
        /// Whether this model has a skin already.
        /// </summary>
        public bool Skinned = false;

        /// <summary>
        /// Loads the skin for this model.
        /// </summary>
        /// <param name="texs">Texture engine.</param>
        public void LoadSkin(TextureEngine texs)
        {
            if (Skinned)
            {
                return;
            }
            Skinned = true;
            if (Engine.TheClient.Files.Exists("models/" + Name + ".skin"))
            {
                string[] data = Engine.TheClient.Files.ReadText("models/" + Name + ".skin").SplitFast('\n');
                int c = 0;
                foreach (string datum in data)
                {
                    if (datum.Length > 0)
                    {
                        string[] datums = datum.SplitFast('=');
                        if (datums.Length == 2)
                        {
                            Texture tex = texs.GetTexture(datums[1]);
                            bool success = false;
                            string datic = datums[0].BeforeAndAfter(":::", out string typer);
                            typer = typer.ToLowerFast();
                            for (int i = 0; i < Meshes.Count; i++)
                            {
                                if (Meshes[i].Name == datic)
                                {
                                    if (typer == "specular")
                                    {
                                        Meshes[i].vbo.Tex_Specular = tex;
                                    }
                                    else if (typer == "reflectivity")
                                    {
                                        Meshes[i].vbo.Tex_Reflectivity = tex;
                                    }
                                    else if (typer == "normal")
                                    {
                                        Meshes[i].vbo.Tex_Normal = tex;
                                    }
                                    else if (typer == "")
                                    {
                                        Meshes[i].vbo.Tex = tex;
                                    }
                                    else
                                    {
                                        SysConsole.Output(OutputType.WARNING, "Unknown skin entry typer: '" + typer + "', expected reflectivity, specular, or simply no specification!");
                                    }
                                    c++;
                                    success = true;
                                }
                            }
                            if (!success)
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown skin entry " + datums[0]);
                                StringBuilder all = new StringBuilder(Meshes.Count * 100);
                                for (int i = 0; i < Meshes.Count; i++)
                                {
                                    all.Append(Meshes[i].Name + ", ");
                                }
                                SysConsole.Output(OutputType.WARNING, "Available: " + all.ToString());
                            }
                        }
                    }
                }
                if (c == 0)
                {
                    SysConsole.Output(OutputType.WARNING, "No entries in " + Name + ".skin");
                }
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "Can't find models/" + Name + ".skin!");
            }
        }

        /// <summary>
        /// Gets VRAM used by this model.
        /// </summary>
        /// <returns></returns>
        public long GetVRAMUsage()
        {
            long ret = 0;
            foreach (ModelMesh mesh in Meshes)
            {
                ret += mesh.vbo.GetVRAMUsage();
            }
            return ret;
        }
    }

    /// <summary>
    /// Represents a model's bone.
    /// </summary>
    public class ModelBone
    {
        /// <summary>
        /// The transform of the bone.
        /// </summary>
        public Matrix4 Transform = Matrix4.Identity;

        /// <summary>
        /// The offset of the bone.
        /// </summary>
        public Matrix4 Offset;
    }

    /// <summary>
    /// Represents a node in a model.
    /// </summary>
    public class ModelNode
    {
        /// <summary>
        /// The parent node.
        /// </summary>
        public ModelNode Parent = null;

        /// <summary>
        /// All children nodes.
        /// </summary>
        public List<ModelNode> Children = new List<ModelNode>();

        /// <summary>
        /// All relevant bones.
        /// </summary>
        public List<ModelBone> Bones = new List<ModelBone>();

        /// <summary>
        /// The mode ID.
        /// </summary>
        public byte Mode;

        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name;
    }

    /// <summary>
    /// Represents a mesh within a model.
    /// </summary>
    public class ModelMesh
    {
        /// <summary>
        /// The name of this mesh.
        /// </summary>
        public string Name;

        /// <summary>
        /// The bones relevant to this mesh.
        /// </summary>
        public List<ModelBone> Bones = new List<ModelBone>();

        /// <summary>
        /// Constructs the model mesh.
        /// </summary>
        /// <param name="_name">The name of it.</param>
        public ModelMesh(string _name)
        {
            Name = _name.ToLowerFast();
            if (Name.EndsWith(".001"))
            {
                Name = Name.Substring(0, Name.Length - ".001".Length);
            }
            Faces = new List<ModelFace>();
            vbo = new Renderable();
        }

        /// <summary>
        /// All the mesh's faces.
        /// </summary>
        public List<ModelFace> Faces;

        /// <summary>
        /// The VBO for this mesh.
        /// </summary>
        public Renderable vbo;

        /// <summary>
        /// Destroys the backing VBO.
        /// </summary>
        public void DestroyVBO()
        {
            vbo.Destroy();
        }
        
        /// <summary>
        /// Renders the mesh.
        /// </summary>
        public void Draw()
        {
            vbo.Render(true);
        }
    }

    /// <summary>
    /// Represents a face of a model mesh.
    /// </summary>
    public class ModelFace
    {
        /// <summary>
        /// Construct the model face.
        /// </summary>
        /// <param name="_l1">L1.</param>
        /// <param name="_l2">L2.</param>
        /// <param name="_l3">L3.</param>
        /// <param name="_t1">T1.</param>
        /// <param name="_t2">T2.</param>
        /// <param name="_t3">T3.</param>
        /// <param name="_normal">Normal vector.</param>
        public ModelFace(int _l1, int _l2, int _l3, int _t1, int _t2, int _t3, Location _normal)
        {
            L1 = _l1;
            L2 = _l2;
            L3 = _l3;
            T1 = _t1;
            T2 = _t2;
            T3 = _t3;
            Normal = _normal;
        }

        /// <summary>
        /// Normal vector.
        /// </summary>
        public Location Normal;

        /// <summary>
        /// L1.
        /// </summary>
        public int L1;

        /// <summary>
        /// L2.
        /// </summary>
        public int L2;

        /// <summary>
        /// L3.
        /// </summary>
        public int L3;

        /// <summary>
        /// T1.
        /// </summary>
        public int T1;

        /// <summary>
        /// T2.
        /// </summary>
        public int T2;

        /// <summary>
        /// T3.
        /// </summary>
        public int T3;
    }
}
