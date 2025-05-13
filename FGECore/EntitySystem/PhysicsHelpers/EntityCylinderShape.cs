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

/// <summary>A cylinder shape for an entity.</summary>
public class EntityCylinderShape : EntityShapeHelper
{
    /// <summary>Gets the volume of a cylinder of the given radius and height.</summary>
    public static double CalculateCylinderVolume(double radius, double height) => Math.PI * radius * radius * height;

    /// <summary>Constructs a new <see cref="EntityCylinderShape"/> of the specified size.</summary>
    public EntityCylinderShape(float radius, float height, PhysicsSpace space) : base(space)
    {
        BepuShape = new Cylinder(radius, height);
        Volume = CalculateCylinderVolume(radius, height);
    }

    /// <summary>The index of the cylinder sub-component, if registered.</summary>
    public TypedIndex CylinderIndex;

    /// <summary>The buffer for the shape's compound child, if registered.</summary>
    public Buffer<CompoundChild> CompoundBuffer;

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntityCylinderShape Register()
    {
        EntityCylinderShape dup = MemberwiseClone() as EntityCylinderShape;
        dup.CylinderIndex = Space.Internal.CoreSimulation.Shapes.Add((Cylinder)BepuShape);
        Space.Internal.Pool.Take(1, out dup.CompoundBuffer);
        dup.CompoundBuffer[0].LocalPosition = Vector3.Zero;
        dup.CompoundBuffer[0].LocalOrientation = Quaternion_Y2Z;
        dup.CompoundBuffer[0].ShapeIndex = dup.CylinderIndex;
        Compound compound = new(dup.CompoundBuffer);
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add(compound);
        return dup;
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Unregister"/>.</summary>
    public override void Unregister()
    {
        if (CylinderIndex.Exists)
        {
            Space.Internal.CoreSimulation.Shapes.Remove(CylinderIndex);
            CylinderIndex = default;
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
        Cylinder cylinder = (Cylinder)BepuShape;
        return $"{nameof(EntityCylinderShape)}(radius={cylinder.Radius}, length={cylinder.Length})";
    }

    /// <inheritdoc/>
    public override void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler)
    {
        RigidPose pose = new(pos, Quaternion_Y2Z);
        simulation.Sweep((Cylinder)BepuShape, pose, velocity, maximumT, pool, ref hitHandler);
    }
}
