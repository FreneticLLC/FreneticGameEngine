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
using FreneticUtilities.FreneticToolkit;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGECore.ModelSystems;
using FGEGraphics.ClientSystem;
using FGEGraphics.ClientSystem.ViewRenderSystem;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers.Models;

/// <summary>System to handle 3D models for rendering.</summary>
public class ModelEngine
{
    /// <summary>All currently loaded models, mapped by name.</summary>
    public Dictionary<string, Model> LoadedModels;

    /// <summary>Internal model helper from the core.</summary>
    public CoreModelEngine CoreModels;

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
        CoreModels = new(tclient);
        GraphicsUtil.CheckError("ModelEngine - Init - Pre");
        Window = tclient;
        AnimEngine = engine;
        LoadedModels = new(128);
        Cube = FromScene(null, CoreModels.Cube, "cube");
        LoadedModels.Add("cube", Cube);
        Cylinder = FromScene(null, CoreModels.Cylinder, "cylinder");
        LoadedModels.Add("cylinder", Cylinder);
        Sphere = FromScene(null, CoreModels.Sphere, "sphere");
        LoadedModels.Add("sphere", Sphere);
        Clear = new Model("clear") { Engine = this, Skinned = true, Original = CoreModels.Clear };
        LoadedModels.Add("clear", Clear);
        GraphicsUtil.CheckError("ModelEngine - Init - Final");
    }

    /// <summary>Update delta time tracker.</summary>
    /// <param name="time">The new noted time.</param>
    public void Update(double time)
    {
        CurrentTime = time;
    }

    /// <summary>
    /// Gets the model object for a specific model name.
    /// If the relevant model exists but is not yet loaded, will load it from file.
    /// </summary>
    /// <param name="modelname">The relative filename, including folder path, beneath the 'models/' dir. For example, "vehicles/car" as input will match to the file at "models/vehicles/car.fmi".</param>
    /// <returns>A valid model object. If the model cannot be loaded, a placeholder cube will be substituted.</returns>
    public Model GetModel(string modelname)
    {
        modelname = FileEngine.CleanFileName(modelname);
        if (LoadedModels.TryGetValue(modelname, out Model existingModel))
        {
            return existingModel;
        }
        Model Loaded;
        try
        {
            Loaded = GetModelFromInfoDynamic(modelname, loadNow: true);
        }
        catch (Exception ex)
        {
            Logs.CriticalError($"Reading model '{modelname}'", ex);
            Loaded = new Model(modelname) { Engine = this, Root = Cube.Root, RootNode = Cube.RootNode, Meshes = Cube.Meshes, MeshMap = Cube.MeshMap, Original = Cube.Original };
            LoadedModels.Add(modelname, Loaded);
        }
        return Loaded;
    }

    /// <summary>Dynamically loads a direct data (.fmd - Frenetic Model Data) format model (by default returns a temporary copy of 'Cube', then fills it in when possible).
    /// <para><see cref="Model.IsLoaded"/> will be <c>false</c> until loaded, then <c>true</c> when ready. May stay false forever if the model file does not exist.</para></summary>
    /// <param name="modelName">The relative filename, including folder path, beneath the 'models/' dir. For example, "vehicles/car" as input will match to the file at "models/vehicles/car.fmd".</param>
    /// <param name="loadNow">If true, the model must load immediately, even if the game will freeze because of it. If false, a dynamic load with a placeholder will be used.</param>
    /// <param name="loadInto">Optional, existing model object to load the model data into. If unspecified, a placeholder cube model will be generated.</param>
    /// <param name="onLoad">Optional action to fire after the model data has loaded. Does not fire if the model fails to load. Fires on window sync thread.</param>
    /// <param name="setLoaded">If true, set <see cref="Model.IsLoaded"/> to true. If false, do not (eg for onLoad to have its own action).</param>
    /// <returns>The model object, generally only containing placeholder cube data initially.</returns>
    public Model GetDirectModelDynamic(string modelName, bool loadNow = false, Model loadInto = null, Action onLoad = null, bool setLoaded = true)
    {
        modelName = FileEngine.CleanFileName(modelName);
        Model model = loadInto ?? new(modelName) { Engine = this, Root = Cube.Root, RootNode = Cube.RootNode, Meshes = Cube.Meshes, MeshMap = Cube.MeshMap, Original = Cube.Original };
        if (loadInto is null)
        {
            LoadedModels.Add(modelName, model);
        }
        CoreModels.LoadDirectModelDynamic(modelName, scene =>
        {
            List<KeyValuePair<ModelMesh, Renderable.ArrayBuilder>> builders = [];
            Model mod = FromSceneNoGenerate(null, scene, modelName, builders);
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
                model.IsLoaded = setLoaded;
                onLoad?.Invoke();
            });
        }, loadNow: loadNow);
        return model;
    }

    /// <summary>Dynamically loads an info (.fmi - Frenetic Model Info) format model (by default returns a temporary copy of 'Cube', then fills it in when possible).
    /// <para><see cref="Model.IsLoaded"/> will be <c>false</c> until loaded, then <c>true</c> when ready. May stay false forever if the model file does not exist.</para></summary>
    /// <param name="modelName">The relative filename, including folder path, beneath the 'models/' dir. For example, "vehicles/car" as input will match to the file at "models/vehicles/car.fmi".</param>
    /// <param name="loadNow">If true, the model must load immediately, even if the game will freeze because of it. If false, a dynamic load with a placeholder will be used. Textures will be loaded with highest priority, but may not be instant still.</param>
    /// <returns>The model object, generally only containing placeholder cube data initially.</returns>
    public Model GetModelFromInfoDynamic(string modelName, bool loadNow = false)
    {
        modelName = FileEngine.CleanFileName(modelName);
        Model model = new(modelName) { Engine = this, Root = Cube.Root, RootNode = Cube.RootNode, Meshes = Cube.Meshes, MeshMap = Cube.MeshMap, Original = Cube.Original };
        CoreModels.GetModelFromInfoDynamic(modelName, scene =>
        {
            Window.Schedule.ScheduleSyncTask(() =>
            {
                FromScene(model, scene, modelName);
                foreach (string line in scene.InfoDataLines)
                {
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    string[] datums = line.SplitFast('=');
                    if (datums.Length != 2 || datums[0] == "model")
                    {
                        continue;
                    }
                    Texture tex = Window.Textures.DynamicLoadTexture(datums[1], loadNow);
                    bool success = false;
                    string meshName = datums[0].BeforeAndAfter(":::", out string texType).ToLowerFast();
                    texType = texType.ToLowerFast();
                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        if (mesh.NameLower != meshName)
                        {
                            continue;
                        }
                        if (texType == "specular")
                        {
                            mesh.BaseRenderable.SpecularTexture = tex;
                        }
                        else if (texType == "reflectivity")
                        {
                            mesh.BaseRenderable.ReflectivityTexture = tex;
                        }
                        else if (texType == "normal")
                        {
                            mesh.BaseRenderable.NormalTexture = tex;
                        }
                        else if (texType == "" || texType == "color")
                        {
                            mesh.BaseRenderable.ColorTexture = tex;
                        }
                        else
                        {
                            Logs.Warning($"While loading model '{modelName}': Unknown model info texture type: '{texType}', expected 'reflectivity', 'specular', 'normal', or empty (color)!");
                        }
                        success = true;
                        model.Skinned = true;
                    }
                    if (!success)
                    {
                        Logs.Warning($"While loading model '{modelName}': Unknown skin entry {datums[0]}, available: {model.Meshes.Select(m => m.Name).JoinString(", ")}");
                    }
                }
            });
        }, loadNow: loadNow);
        LoadedModels.Add(modelName, model);
        return model;
    }

    /// <summary>Converts a core Scene to a renderable model.</summary>
    /// <param name="loadInto">Optional model to load the data into.</param>
    /// <param name="scene">The backing model.</param>
    /// <param name="name">The name to use.</param>
    /// <returns>The model, fully generated and ready to render.</returns>
    public Model FromScene(Model loadInto, Model3D scene, string name)
    {
        List<KeyValuePair<ModelMesh, Renderable.ArrayBuilder>> builders = [];
        Model mod = FromSceneNoGenerate(loadInto, scene, name, builders);
        foreach (KeyValuePair<ModelMesh, Renderable.ArrayBuilder> builder in builders)
        {
            builder.Key.BaseRenderable.GenerateVBO(builder.Value);
        }
        mod.IsLoaded = true;
        return mod;
    }

    /// <summary>Converts a core Scene to a renderable model, without running the VBO generate step.</summary>
    /// <param name="loadInto">Optional model to load the data into.</param>
    /// <param name="scene">The backing model.</param>
    /// <param name="name">The name to use.</param>
    /// <param name="vboBuilders">The VBO builders for output.</param>
    /// <returns>The model, not generated yet.</returns>
    public Model FromSceneNoGenerate(Model loadInto, Model3D scene, string name, List<KeyValuePair<ModelMesh, Renderable.ArrayBuilder>> vboBuilders)
    {
        Model model = loadInto ?? new(name) { Engine = this };
        model.Meshes = [];
        model.MeshMap = [];
        model.Original = scene;
        model.Root = scene.MatrixA.Convert();
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
            bool hascolors = mesh.Colors is not null && mesh.Colors.Length == mesh.Vertices.Length;
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
                builder.TexCoords[i] = hastc ? new Vector3(mesh.TexCoords[i].X, 1 - mesh.TexCoords[i].Y, 0) : new Vector3(0, 0, 0);
                builder.Normals[i] = hasn ? mesh.Normals[i].ToOpenTK() : new Vector3(0f, 0f, 1f);
                builder.Colors[i] = hascolors ? mesh.Colors[i].ToOpenTK() : new Vector4(1, 1, 1, 1); // TODO: From the mesh?
#if DEBUG
                if (builder.Vertices[i].ToLocation().IsNaNOrInfinite() || builder.TexCoords[i].ToLocation().IsNaNOrInfinite() || builder.Normals[i].ToLocation().IsNaNOrInfinite())
                {
                    throw new Exception($"Mesh has invalid vertex data! ({name}) (index={i}, vertex={builder.Vertices[i]}, tc={builder.TexCoords[i]}, normal={builder.Normals[i]}, color={builder.Colors[i]})");
                }
                if (builder.Normals[i].LengthSquared < 0.01f)
                {
                    throw new Exception($"Mesh has invalid normal vector! ({name}) (index={i}, normal={builder.Normals[i]})");
                }
#endif
            }
            for (int i = 0; i < mesh.Indices.Length; i++)
            {
                builder.Indices[i] = mesh.Indices[i];
#if DEBUG
                if (mesh.Indices[i] >= mesh.Vertices.Length)
                {
                    throw new Exception($"Mesh has invalid index {mesh.Indices[i]} (vertex count {mesh.Vertices.Length})! ({name})");
                }
#endif
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
        for (int i = 0; i < scene.Meshes.Length; i++)
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
