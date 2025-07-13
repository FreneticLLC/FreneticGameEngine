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
using FGECore.PhysicsSystem;
using FGECore.PropertySystem;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>A sphere shape for an entity.</summary>
public class EntitySphereShape : EntityShapeHelper
{
    private const double SPHERE_VOLUME_CONSTANT = 4.0 / 3.0 * Math.PI;

    /// <summary>Gets the volume for a sphere of the given radius.</summary>
    public static double CalculateSphereVolume(double radius) => SPHERE_VOLUME_CONSTANT * radius * radius * radius; // 4/3 pi r^3

    /// <summary>Constructs a new <see cref="EntitySphereShape"/> of the specified size.</summary>
    public EntitySphereShape(float size, PhysicsSpace space) : base(space)
    {
        BepuShape = new Sphere(size);
        Volume = CalculateSphereVolume(size);
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntitySphereShape Register()
    {
        EntitySphereShape dup = MemberwiseClone() as EntitySphereShape;
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add((Sphere)BepuShape);
        return dup;
    }

    /// <summary>Implements <see cref="Object.ToString"/>.</summary>
    public override string ToString()
    {
        Sphere sphere = (Sphere)BepuShape;
        return $"{nameof(EntitySphereShape)}({sphere.Radius})";
    }

    /// <inheritdoc/>
    public override void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler)
    {
        RigidPose pose = new(pos, Quaternion.Identity);
        simulation.Sweep((Sphere)BepuShape, pose, velocity, maximumT, pool, ref hitHandler);
    }
}
