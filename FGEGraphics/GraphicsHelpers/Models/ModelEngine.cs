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
using FreneticUtilities.FreneticExtensions;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGECore.ModelSystems;
using FGECore.PhysicsSystem;
using FGEGraphics.ClientSystem;
using FGEGraphics.ClientSystem.ViewRenderSystem;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers.Models;

/// <summary>System to handle 3D models for rendering.</summary>
public class ModelEngine
{
    /// <summary>All currently loaded models, mapped by name.</summary>
    public Dictionary<string, Model> LoadedModels;

    /// <summary>Internal model helper from the core.</summary>
    public ModelHandler Handler;

    /// <summary>A generic 1x1x1 cube model.</summary>
    public Model Cube;

    /// <summary>A generic cylinder model.</summary>
    public Model Cylinder;

    /// <summary>A generic sphere model.</summary>
    public Model Sphere;

    /// <summary>A clear (empty) model.</summary>
    public Model Clear;

    /// <summary>The client game window that owns this model engine.</summary>
    public GameClientWindow Window;

    /// <summary>Current known engine time (in seconds since engine start).</summary>
    public double CurrentTime = 0;

    /// <summary>Backing animation engine.</summary>
    public AnimationEngine AnimEngine;

    /// <summary>Prepares the model system.</summary>
    /// <param name="engine">Animation engine.</param>
    /// <param name="tclient">Backing client.</param>
    public void Init(AnimationEngine engine, GameClientWindow tclient)
    {
        Window = tclient;
        AnimEngine = engine;
        Handler = new ModelHandler();
        LoadedModels = new Dictionary<string, Model>(128);
        Cube = GenerateCube();
        LoadedModels.Add("cube", Cube);
        Cylinder = FromScene(ShapeGenerators.GenerateCylinder(1, 5, 20), "cylinder");
        LoadedModels.Add("cylinder", Cylinder);
        Sphere = FromScene(ShapeGenerators.GenerateUVSphere(1, 10, 60), "sphere");
        LoadedModels.Add("sphere", Sphere);
        Clear = new Model("clear") { Engine = this, Skinned = true, Original = new Model3D() };
        LoadedModels.Add("clear", Clear);
    }

    /// <summary>Generates a cube model.</summary>
    /// <returns>The cube model.</returns>
    public Model GenerateCube()
    {
        Model m = new("cube")
        {
            Engine = this,
            Skinned = true,
            ModelMin = new Location(-0.5),
            ModelMax = new Location(0.5)
        };
        ModelMesh mm = new("cube", m);
        Renderable.ListBuilder builder = new();
        builder.Prepare();
        TextureCoordinates tc = new();
        builder.AddSide(Location.UnitX, tc, offs: true, scale: 0.5f);
        builder.AddSide(Location.UnitY, tc, offs: true, scale: 0.5f);
        builder.AddSide(Location.UnitZ, tc, offs: true, scale: 0.5f);
        builder.AddSide(-Location.UnitX, tc, offs: true, scale: 0.5f);
        builder.AddSide(-Location.UnitY, tc, offs: true, scale: 0.5f);
        builder.AddSide(-Location.UnitZ, tc, offs: true, scale: 0.5f);
        m.Original = new Model3D();
        Model3DMesh m3m = new()
        {
            Name = "cube",
            Indices = [.. builder.Indices],
            Vertices = [.. builder.Vertices.ConvertAll((o) => o.ToLocation().ToNumerics())],
            TexCoords = [.. builder.TexCoords.ConvertAll((o) => new System.Numerics.Vector2(o.X, o.Y))],
            Normals = [.. builder.Normals.ConvertAll((o) => o.ToLocation().ToNumerics())]
        };
        m.Original.Meshes = [m3m];
        mm.BaseRenderable.GenerateVBO(builder);
        m.AddMesh(mm);
        return m;
    }

    /// <summary>Update delta time tracker.</summary>
    /// <param name="time">The new noted time.</param>
    public void Update(double time)
    {
        CurrentTime = time;
    }

    /// <summary>
    /// Loads a model from file by name.
    /// <para>Note: Most users should not use this method. Instead, use <see cref="GetModel(string)"/>.</para>
    /// </summary>
    /// <param name="filename">The name.</param>
    /// <returns>The model.</returns>
    public Model LoadModel(string filename)
    {
        try
        {
            filename = FileEngine.CleanFileName(filename);
            if (!Window.Files.TryReadFileData($"models/{filename}.vmd", out byte[] bits))
            {
                Logs.Warning($"Cannot load model, file '{TextStyle.Standout}models/{filename}.vmd{TextStyle.Base}' does not exist.");
                return null;
            }
            return FromBytes(filename, bits);
        }
        catch (Exception ex)
        {
            Logs.Error($"Failed to load model from filename '{TextStyle.Standout}models/{filename}.vmd{TextStyle.Error}': {ex}");
            return null;
        }
    }

    /// <summary>
    /// Gets the model object for a specific model name.
    /// If the relevant model exists but is not yet loaded, will load it from file.
    /// </summary>
    /// <param name="modelname">The name of the model.</param>
    /// <returns>A valid model object.</returns>
    public Model GetModel(string modelname)
    {
        modelname = FileEngine.CleanFileName(modelname);
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
            Logs.Error(ex.ToString());
        }
        Loaded ??= new Model(modelname) { Engine = this, Root = Cube.Root, RootNode = Cube.RootNode, Meshes = Cube.Meshes, MeshMap = Cube.MeshMap, Original = Cube.Original };
        LoadedModels.Add(modelname, Loaded);
        return Loaded;
    }

    /// <summary>Dynamically loads a model (returns a temporary copy of 'Cube', then fills it in when possible).</summary>
    /// <param name="modelName">The model name to load.</param>
    /// <returns>The model object.</returns>
    public Model DynamicLoadModel(string modelName)
    {
        modelName = FileEngine.CleanFileName(modelName);
        Model model = new(modelName) { Engine = this, Root = Cube.Root, RootNode = Cube.RootNode, Meshes = Cube.Meshes, MeshMap = Cube.MeshMap, Original = Cube.Original };
        void processLoad(byte[] data)
        {
            Model3D scene = Handler.LoadModel(data);
            List<KeyValuePair<ModelMesh, Renderable.ArrayBuilder>> builders = [];
            Model mod = FromSceneNoGenerate(scene, modelName, builders);
            Window.Schedule.ScheduleSyncTask(() =>
            {
                foreach (KeyValuePair<ModelMesh, Renderable.ArrayBuilder> builder in builders)
                {
                    builder.Key.BaseRenderable.GenerateVBO(builder.Value);
                }
                model.Meshes = mod.Meshes;
                model.MeshMap = mod.MeshMap;
                model.Original = mod.Original;
                model.Root = mod.Root;
                model.RootNode = mod.RootNode;
                model.LODHelper = null;
                model.LODBox = default;
                model.ModelMin = new Location(-0.5);
                model.ModelMax = new Location(0.5);
                model.ModelBoundsSet = false;
                model.IsLoaded = true;
            });
        }
        void fileMissing()
        {
            Logs.Warning($"Cannot load model, file '{TextStyle.Standout}models/{modelName}.vmd{TextStyle.Base}' does not exist.");
        }
        void handleError(string message)
        {
            Logs.Error($"Failed to load model from filename '{TextStyle.Standout}models/{modelName}.vmd{TextStyle.Base}': {message}");
        }
        Window.AssetStreaming.AddGoal($"models/{modelName}.vmd", false, processLoad, fileMissing, handleError);
        return model;
    }

    /// <summary>loads a model from a file byte array.</summary>
    /// <param name="name">The name of the model.</param>
    /// <param name="data">The .obj file string.</param>
    /// <returns>A valid model.</returns>
    public Model FromBytes(string name, byte[] data)
    {
        Model3D scene = Handler.LoadModel(data);
        return FromScene(scene, name);
    }

    /// <summary>Converts a core Scene to a renderable model.</summary>
    /// <param name="scene">The backing model.</param>
    /// <param name="name">The name to use.</param>
    /// <returns>The model.</returns>
    public Model FromScene(Model3D scene, string name)
    {
        List<KeyValuePair<ModelMesh, Renderable.ArrayBuilder>> builders = [];
        Model mod = FromSceneNoGenerate(scene, name, builders);
        foreach (KeyValuePair<ModelMesh, Renderable.ArrayBuilder> builder in builders)
        {
            builder.Key.BaseRenderable.GenerateVBO(builder.Value);
        }
        mod.IsLoaded = true;
        return mod;
    }

    /// <summary>Converts a core Scene to a renderable model, without running the VBO generate step.</summary>
    /// <param name="scene">The backing model.</param>
    /// <param name="name">The name to use.</param>
    /// <param name="vboBuilders">The VBO builders for output.</param>
    /// <returns>The model.</returns>
    public Model FromSceneNoGenerate(Model3D scene, string name, List<KeyValuePair<ModelMesh, Renderable.ArrayBuilder>> vboBuilders)
    {
        Model model = new(name)
        {
            Engine = this,
            Original = scene,
            Root = scene.MatrixA.Convert()
        };
        if (scene.Meshes.Length == 0)
        {
            throw new Exception($"Scene has no meshes! ({name})");
        }
        foreach (Model3DMesh mesh in scene.Meshes)
        {
            if (mesh.Name.ToLowerFast().Contains("collision") || mesh.Name.ToLowerFast().Contains("norender"))
            {
                continue;
            }
            ModelMesh modmesh = new(mesh.Name, model);
            bool hastc = mesh.TexCoords.Length == mesh.Vertices.Length;
            bool hasn = mesh.Normals.Length == mesh.Vertices.Length;
            if (!hasn)
            {
                Logs.Warning($"Mesh has no normals! ({name})");
            }
            if (!hastc)
            {
                Logs.Warning($"Mesh has no texcoords! ({name})");
            }
            Renderable.ArrayBuilder builder = new();
            builder.Prepare(mesh.Vertices.Length, mesh.Indices.Length);
            for (int i = 0; i < mesh.Vertices.Length; i++)
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
            for (int i = 0; i < mesh.Indices.Length; i++)
            {
                builder.Indices[i] = mesh.Indices[i];
            }
            int bc = mesh.Bones.Length;
            if (bc > View3DInternalData.MAX_BONES)
            {
                Logs.Warning($"Mesh has {bc} bones! ({name})");
                bc = View3DInternalData.MAX_BONES;
            }
            int[] pos = new int[builder.Vertices.Length];
            for (int i = 0; i < bc; i++)
            {
                for (int x = 0; x < mesh.Bones[i].Weights.Length; x++)
                {
                    int IDa = mesh.Bones[i].IDs[x];
                    float Weighta = (float)mesh.Bones[i].Weights[x];
                    int spot = pos[IDa]++;
                    if (spot > 7)
                    {
                        //OutputType.WARNING.Output("Too many bones influencing " + vw.VertexID + "!");
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
            vboBuilders.Add(new KeyValuePair<ModelMesh, Renderable.ArrayBuilder>(modmesh, builder));
            model.AddMesh(modmesh);
        }
        model.RootNode = new ModelNode() { Parent = null, Name = scene.RootNode.Name.ToLowerFast() };
        List<ModelNode> allNodes = [];
        PopulateChildren(model.RootNode, scene.RootNode, model, AnimEngine, allNodes);
        for (int i = 0; i < model.Meshes.Count; i++)
        {
            for (int x = 0; x < scene.Meshes[i].Bones.Length; x++)
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
                ModelBone mb = new() { Offset = scene.Meshes[i].Bones[x].MatrixA.Convert() };
                nodet.Bones.Add(mb);
                model.Meshes[i].Bones.Add(mb);
            }
        }
        return model;
    }

    /// <summary>Populates the children of a node.</summary>
    /// <param name="node">The node.</param>
    /// <param name="orin">The original node.</param>
    /// <param name="model">The model.</param>
    /// <param name="engine">The engine.</param>
    /// <param name="allNodes">All current nodes.</param>
    public void PopulateChildren(ModelNode node, Model3DNode orin, Model model, AnimationEngine engine, List<ModelNode> allNodes)
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
        for (int i = 0; i < orin.Children.Length; i++)
        {
            ModelNode child = new() { Parent = node, Name = orin.Children[i].Name.ToLowerFast() };
            PopulateChildren(child, orin.Children[i], model, engine, allNodes);
            node.Children.Add(child);
        }
    }

    /// <summary>Foce-sets a vector subindex in a list.</summary>
    /// <param name="vecs">The vector list.</param>
    /// <param name="ind">The index.</param>
    /// <param name="subind">The vector subindex.</param>
    /// <param name="val">The new value.</param>
    static void ForceSet(Vector4[] vecs, int ind, int subind, float val)
    {
        Vector4 vec = vecs[ind];
        vec[subind] = val;
        vecs[ind] = vec;
    }
}
