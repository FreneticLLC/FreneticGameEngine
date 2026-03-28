//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.FileSystems;

namespace FGECore.ModelSystems;

/// <summary>FGECore implementation basis of a model engine. Can be used by servers to process basic model data, and is also used as a core component of the Graphics system to load models.</summary>
public class CoreModelEngine
{
    /// <summary>All currently loaded models, mapped by name.</summary>
    public ConcurrentDictionary<string, Model3D> LoadedModels = [];

    /// <summary>All currently loaded models, mapped by direct ('.fmd') name.</summary>
    public ConcurrentDictionary<string, Model3D> LoadedDirectModels = [];

    /// <summary>Internal model helper from the core.</summary>
    public ModelHandler Handler = new();

    /// <summary>A generic 1x1x1 cube model.</summary>
    public Model3D Cube = ShapeGenerators.GenerateCube(1);

    /// <summary>A generic cylinder model (radius=1, height=2).</summary>
    public Model3D Cylinder = ShapeGenerators.GenerateCylinder(1, 2, 20);

    /// <summary>A generic sphere model (radius=1).</summary>
    public Model3D Sphere = ShapeGenerators.GenerateUVSphere(1, 10, 60);

    /// <summary>A clear (empty) model.</summary>
    public Model3D Clear = new();

    /// <summary>The game instance this model engine belongs to.</summary>
    public GameInstance Instance;

    /// <summary>Construct the model engine.</summary>
    /// <param name="_instance">The game instance this model engine belongs to.</param>
    public CoreModelEngine(GameInstance _instance)
    {
        Instance = _instance;
        Cube.Name = "cube";
        LoadedModels["cube"] = Cube;
        Cylinder.Name = "cylinder";
        LoadedModels["cylinder"] = Cylinder;
        Sphere.Name = "sphere";
        LoadedModels["sphere"] = Sphere;
        Clear.Name = "clear";
        LoadedModels["clear"] = Clear;
    }

    /// <summary>Dynamically loads a direct data (.fmd - Frenetic Model Data). Fires an action when loaded.</summary>
    /// <param name="modelName">The relative filename, including folder path, beneath the 'models/' dir. For example, "vehicles/car" as input will match to the file at "models/vehicles/car.fmd".</param>
    /// <param name="onLoad">Action to fire after the model data has loaded. Does not fire if the model fails to load. Fire on an arbitrary thread.</param>
    /// <param name="loadNow">If true, the model must load immediately, even if the game will freeze because of it. If false, a dynamic load with a placeholder will be used.</param>
    /// <param name="onFailure">Optional action to fire when loading fails for any reason.</param>
    /// <returns>The model object, generally only containing placeholder cube data initially.</returns>
    public void InternalLoadDirectModelDynamic(string modelName, Action<Model3D> onLoad, Action onFailure = null, bool loadNow = false)
    {
        modelName = FileEngine.CleanFileName(modelName);
        if (LoadedDirectModels.TryGetValue(modelName, out Model3D existing))
        {
            onLoad(existing);
            return;
        }
        string targetPath = $"models/{modelName}.fmd";
        void processLoad(byte[] data)
        {
            Model3D scene = Handler.LoadModel(data);
            scene.Name = modelName;
            LoadedDirectModels[modelName] = scene;
            onLoad(scene);
        }
        void fileMissing()
        {
            Logs.Warning($"[CoreModelEngine] Cannot load non-existent model data '{TextStyle.Standout}{targetPath}{TextStyle.Base}'. {Instance.Files.WarnWhyFileMissing(targetPath)}");
            onFailure?.Invoke();
        }
        void handleError(string message)
        {
            Logs.Error($"[CoreModelEngine] Failed to load model from data filename '{TextStyle.Standout}{targetPath}{TextStyle.Base}': {message}");
            onFailure?.Invoke();
        }
        if (loadNow)
        {
            if (Instance.Files.TryReadFileData(targetPath, out byte[] bits))
            {
                processLoad(bits);
            }
            else
            {
                fileMissing();
            }
        }
        else
        {
            Instance.AssetStreaming.AddGoal(targetPath, false, processLoad, fileMissing, handleError);
        }
    }

    /// <summary>Dynamically loads an info (.fmi - Frenetic Model Info) format model (by default returns a temporary copy of 'Cube', then fills it in when possible).</summary>
    /// <param name="modelName">The relative filename, including folder path, beneath the 'models/' dir. For example, "vehicles/car" as input will match to the file at "models/vehicles/car.fmi". This can also be 'direct:name:model=somefmd'.</param>
    /// <param name="loadNow">If true, the model must load immediately, even if the game will freeze because of it. If false, a dynamic load with a placeholder will be used.</param>
    /// <param name="onLoad">Action to fire after the model data has loaded. Does not fire if the model fails to load. Fire on an arbitrary thread.</param>
    /// <param name="onFailure">Optional action to fire when loading fails for any reason.</param>
    /// <returns>The model object, generally only containing placeholder cube data initially.</returns>
    public void GetModelFromInfoDynamic(string modelName, Action<Model3D> onLoad, Action onFailure = null, bool loadNow = false)
    {
        string cleanName = FileEngine.CleanFileName(modelName);
        if (LoadedModels.TryGetValue(cleanName, out Model3D existing))
        {
            onLoad(existing);
            return;
        }
        string targetPath = $"models/{cleanName}.fmi";
        if (modelName.StartsWith("direct:"))
        {
            string data = modelName["direct:".Length..];
            (string name, string content) = data.BeforeAndAfter(':');
            processLoad(StringConversionHelper.UTF8Encoding.GetBytes(content));
            return;
        }
        void processLoad(byte[] data)
        {
            string fileText = StringConversionHelper.UTF8Encoding.GetString(data);
            string[] lines = fileText.SplitFast('\n');
            string fmdName = cleanName;
            foreach (string line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }
                string[] datums = line.SplitFast('=');
                if (datums.Length != 2)
                {
                    Logs.Warning($"[CoreModelEngine] Invalid line in model info file '{targetPath}' (missing a '=' symbol): {line}");
                    continue;
                }
                if (datums[0] == "model")
                {
                    fmdName = datums[1];
                }
            }
            void doNextStep(Model3D model)
            {
                model = model.Duplicate();
                model.Name = cleanName;
                model.InfoDataLines = lines;
                LoadedModels[cleanName] = model;
                onLoad?.Invoke(model);
            }
            InternalLoadDirectModelDynamic(fmdName, loadNow: loadNow, onLoad: doNextStep, onFailure: onFailure);
        }
        void fileMissing()
        {
            Logs.Warning($"[CoreModelEngine] Cannot load non-existent model info file '{TextStyle.Standout}{targetPath}{TextStyle.Base}'. {Instance.Files.WarnWhyFileMissing(targetPath)}");
            onFailure?.Invoke();
        }
        void handleError(string message)
        {
            Logs.Error($"[CoreModelEngine] Failed to load model from info filename '{TextStyle.Standout}{targetPath}{TextStyle.Base}': {message}");
            onFailure?.Invoke();
        }
        if (loadNow)
        {
            if (Instance.Files.TryReadFileData(targetPath, out byte[] bits))
            {
                processLoad(bits);
            }
            else
            {
                fileMissing();
            }
        }
        else
        {
            Instance.AssetStreaming.AddGoal(targetPath, false, processLoad, fileMissing, handleError);
        }
    }
}
