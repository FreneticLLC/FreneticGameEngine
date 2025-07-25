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
using FGECore.PropertySystem;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>A capsule shape for an entity.</summary>
public class EntityCapsuleShape : EntityShapeHelper
{
    /// <summary>Constructs a new <see cref="EntityCapsuleShape"/> of the specified size.</summary>
    public EntityCapsuleShape(float radius, float height, PhysicsSpace space) : base(space)
    {
        BepuShape = new Capsule(radius, height);
        // A capsule is just a cylinder + two halves of a sphere
        Volume = EntityCylinderShape.CalculateCylinderVolume(radius, height) + EntitySphereShape.CalculateSphereVolume(radius);
    }

    /// <summary>The index of the capsule sub-component, if registered.</summary>
    public TypedIndex CapsuleIndex;

    /// <summary>The buffer for the shape's compound child, if registered.</summary>
    public Buffer<CompoundChild> CompoundBuffer;

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntityCapsuleShape Register()
    {
        EntityCapsuleShape dup = MemberwiseClone() as EntityCapsuleShape;
        dup.CapsuleIndex = Space.Internal.CoreSimulation.Shapes.Add((Capsule)BepuShape);
        Space.Internal.Pool.Take(1, out dup.CompoundBuffer);
        dup.CompoundBuffer[0].LocalPosition = Vector3.Zero;
        dup.CompoundBuffer[0].LocalOrientation = Quaternion_Y2Z;
        dup.CompoundBuffer[0].ShapeIndex = dup.CapsuleIndex;
        Compound compound = new(dup.CompoundBuffer);
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add(compound);
        return dup;
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Unregister"/>.</summary>
    public override void Unregister()
    {
        if (CapsuleIndex.Exists)
        {
            Space.Internal.CoreSimulation.Shapes.Remove(CapsuleIndex);
            CapsuleIndex = default;
        }
        base.Unregister();
        if (CompoundBuffer.Allocated)
        {
            Space.Internal.Pool.Return(ref CompoundBuffer);
            CompoundBuffer = default;
        }
    }

    /// <summary>Implements <see cref="Object.ToString"/>.</summary>
    public override string ToString()
    {
        Capsule capsule = (Capsule)BepuShape;
        return $"{nameof(EntityCapsuleShape)}(radius={capsule.Radius}, length={capsule.Length})";
    }

    /// <inheritdoc/>
    public override void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler)
    {
        RigidPose pose = new(pos, Quaternion_Y2Z);
        simulation.Sweep((Capsule)BepuShape, pose, velocity, maximumT, pool, ref hitHandler);
    }
}
