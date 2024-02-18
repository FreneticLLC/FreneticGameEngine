using BepuPhysics.Constraints;
using FGECore.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem.JointSystems.PhysicsJoints;

/// <summary>Constrains the relative angular velocity of two bodies around a local axis attached to body A to a target velocity.</summary>
public class JointAngularAxisMotor(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location axis) : PhysicsJointBase<AngularAxisMotor>(e1, e2)
{
    /// <summary>The relative rotational axis.</summary>
    public Location Axis = axis;

    /// <summary>Inverse damping; how soft this motor should be, 0 means super-rigid, 1 means very soft, above 1 has very little force application left.</summary>
    public float Softness = 0.03f;

    /// <summary>Maximum amount of force this motor can apply in one unit-time (?).</summary>
    public float MaximumForce = 100_000;

    /// <summary>Current target speed.</summary>
    public float TargetVelocity = 0;

    /// <inheritdoc/>
    public override AngularAxisMotor CreateJointDescription()
    {
        return new AngularAxisMotor() { TargetVelocity = TargetVelocity, LocalAxisA = Axis.ToNumerics(), Settings = new() { MaximumForce = MaximumForce, Softness = Softness } };
    }
}
