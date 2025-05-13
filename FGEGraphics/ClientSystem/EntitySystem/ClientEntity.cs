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
using FGECore.EntitySystem;
using FGECore.PropertySystem;

namespace FGEGraphics.ClientSystem.EntitySystem;

/// <summary>Represents an entity on the client side.</summary>
/// <param name="_engine">The owning game engine.</param>
public class ClientEntity(GameEngineBase _engine) : BasicEntity<ClientEntity, GameEngineBase>(_engine)
{
    /// <summary>
    /// Get or set the renderer for this entity.
    /// Adding or removing a renderable will set this value.
    /// </summary>
    public EntityRenderableProperty Renderer = null;

    /// <summary>Dynamic action for any custom 2D rendering behavior, triggered after the main <see cref="Renderer"/> has rendered.</summary>
    public Action<RenderContext2D> OnRendered2D;

    /// <summary>Dynamic action for any custom 3D rendering behavior, triggered after the main <see cref="Renderer"/> has rendered.</summary>
    public Action<RenderContext> OnRendered;

    /// <summary>Gets the client window associated with this entity.</summary>
    public GameClientWindow Window => Engine.Client;

    /// <summary>Called when a property is added.</summary>
    public override void OnAdded(Property prop)
    {
        base.OnAdded(prop);
        if (Renderer is null && prop is EntityRenderableProperty rnd)
        {
            Renderer = rnd;
        }
    }

    /// <summary>Called when a property is removed.</summary>
    public override void OnRemoved(Property prop)
    {
        base.OnRemoved(prop);
        if (prop == Renderer)
        {
            Renderer = null;
        }
    }

    /// <summary>Gets a string debug helper for this entity.</summary>
    public override string ToString()
    {
        return $"ClientEntity of type: {GetType().Name}, ID: {EID}, with properties: {PropertyList()}";
    }
}
