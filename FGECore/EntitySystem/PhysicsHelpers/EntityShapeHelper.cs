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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>Implementations of this class are helpers for the various possible entity physics shapes.</summary>
public abstract class EntityShapeHelper(PhysicsSpace _space)
{

    /// <summary>Helper value: a quaternion that represents the rotation between UnitY and UnitZ.</summary>
    public static readonly System.Numerics.Quaternion Quaternion_Y2Z = MathHelpers.Quaternion.GetQuaternionBetween(Location.UnitY, Location.UnitZ).ToNumerics();

    /// <summary>The space this shape is registered into.</summary>
    public PhysicsSpace Space = _space;

    /// <summary>The BEPU shape index if registered.</summary>
    public TypedIndex ShapeIndex;

    /// <summary>The volume of this shape.</summary>
    public double Volume = 1;

    /// <summary>Unregisters the shape from the physics space, invalidating it.</summary>
    public virtual void Unregister()
    {
        if (ShapeIndex.Exists)
        {
            Space.Internal.CoreSimulation.Shapes.Remove(ShapeIndex);
            ShapeIndex = default;
        }
    }

    /// <summary>Registers the shape into the physics space, and returns the BEPU shape index.</summary>
    public abstract EntityShapeHelper Register();

    /// <summary>Gets the BEPU convex shape (if possible).</summary>
    public IShape BepuShape;

    /// <summary>Compute inertia for the shape.</summary>
    public virtual void ComputeInertia(float mass, out BodyInertia inertia)
    {
        if (BepuShape is IConvexShape convex)
        {
            inertia = convex.ComputeInertia(mass);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Call <see cref="Simulation.Sweep{TShape, TSweepHitHandler}(in TShape, in RigidPose, in BodyVelocity, float, BufferPool, ref TSweepHitHandler)"/> for this shape.</summary>
    /// <typeparam name="TSweepHitHandler">Type of the callbacks executed when a sweep impacts an object in the scene.</typeparam>
    /// <param name="simulation">The backing simulation.</param>
    /// <param name="pos">Starting position of the sweep.</param>
    /// <param name="velocity">Velocity of the swept shape.</param>
    /// <param name="maximumT">Maximum length of the sweep in units of time used to integrate the velocity.</param>
    /// <param name="pool">Pool to allocate any temporary resources in during execution.</param>
    /// <param name="hitHandler">Callbacks executed when a sweep impacts an object in the scene.</param>
    /// <remarks>Simulation objects are treated as stationary during the sweep.</remarks>
    public abstract void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler) where TSweepHitHandler : ISweepHitHandler;
}
