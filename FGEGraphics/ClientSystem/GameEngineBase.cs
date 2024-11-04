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
using FGECore.CoreSystems;
using FGECore.StackNoteSystem;
using FGEGraphics.AudioSystem;
using FGEGraphics.ClientSystem.EntitySystem;
using FGEGraphics.GraphicsHelpers.Models;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace FGEGraphics.ClientSystem;

/// <summary>Represents the common functionality of a client Game Engine.</summary>
public abstract class GameEngineBase : BasicEngine<ClientEntity, GameEngineBase>, IDisposable
{
    /// <summary>Dumb MS logic dispose method.</summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Sounds.Dispose();
            Shutdown();
        }
    }

    /// <summary>Creates an entity.</summary>
    /// <returns>The entity.</returns>
    public override ClientEntity CreateEntity()
    {
        return new ClientEntity(this);
    }

    /// <summary>Disposes the window client.</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    /// <summary>The backing game client.</summary>
    public GameClientWindow Client => OwningInstance as GameClientWindow;

    /// <summary>Whether to shut up when the game is deselected.</summary>
    public bool QuietOnDeselect = true;

    /// <summary>The sound system.</summary>
    public SoundEngine Sounds;

    /// <summary>The audio camera view.</summary>
    public Camera3D AudioCamera = new();

    /// <summary>Gets the client window.</summary>
    public GameWindow Window
    {
        get
        {
            return Client.Window;
        }
    }

    /// <summary>Gets the client shader system.</summary>
    public ShaderEngine Shaders
    {
        get
        {
            return Client.Shaders;
        }
    }

    /// <summary>Gets the client texture system.</summary>
    public TextureEngine Textures
    {
        get
        {
            return Client.Textures;
        }
    }

    /// <summary>Gets the client model system.</summary>
    public ModelEngine Models
    {
        get
        {
            return Client.Models;
        }
    }

    /// <summary>The title of the window.</summary>
    public readonly string StartingWindowTitle;

    /// <summary>Run through a full single-frame render sequence.</summary>
    public abstract void RenderSingleFrame();

    /// <summary>Get any relevant shaders.</summary>
    public abstract void GetShaders();

    /// <summary>Reload any relevant screen buffers.</summary>
    public abstract void ReloadScreenBuffers();

    /// <summary>Loads the engine.</summary>
    public void Load()
    {
        try
        {
            StackNoteHelper.Push("GameEngineBase - Loading", this);
            Logs.ClientInit("GameEngine starting load sequence, start with basic...");
            LoadBasic();
            Logs.ClientInit("GameEngine loading shaders...");
            GetShaders();
            Logs.ClientInit("GameEngine core load complete, calling additional load...");
            PostLoad();
            Logs.ClientInit("GameEngine prepping audio systems...");
            Sounds = new SoundEngine();
            Sounds.Init(this);
            Logs.ClientInit("GameEngine load sequence complete.");
        }
        finally
        {
            StackNoteHelper.Pop();
        }
    }

    /// <summary>Any post-load actions.</summary>
    public abstract void PostLoad();

    /// <summary>Calculates whether a renderable should render.</summary>
    /// <param name="render">The renderable.</param>
    /// <param name="cast_shadows">Whether currently casting shadows.</param>
    /// <returns>Whether it should render.</returns>
    public static bool ShouldRender(EntityRenderableProperty render, bool cast_shadows)
    {
        if (render == null || !render.IsVisible)
        {
            return false;
        }
        if (cast_shadows ? !render.CastShadows : render.ShadowsOnly)
        {
            return false;
        }
        return true;
    }

    /// <summary>Returns a string of this object.</summary>
    /// <returns>The string.</returns>
    public override string ToString()
    {
        return "GameEngine";
    }
}
