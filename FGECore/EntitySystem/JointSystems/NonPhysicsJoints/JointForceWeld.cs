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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using BepuPhysics;

namespace FGECore.EntitySystem.JointSystems.NonPhysicsJoints;

/// <summary>Constrains entity Two to be force-welded to entity One, meaning that wherever One moves, Two will follow.</summary>
public class JointForceWeld : NonPhysicalJointBase
{
    /// <summary>If true, this weld is a visual only weld. If false, physical position will be modified. Only one VisualOnly joint can control an entity (ie EntityTwo) at any given time.
    /// <para>Do not modify this after construction, instead rebuild with a new value.</para></summary>
    public bool VisualOnly;

    /// <summary>If true, and both entities are physics entities, any separation entity two has from entity one will be applied back onto entity one as a force. Essentially making this joint actual more like a physics weld (not actually identical).
    /// TODO: the implementation is not stable
    /// </summary>
    public bool SendForceBack = false;

    /// <summary>The relative pose offset from entity One to entity Two.</summary>
    public RigidPose Offset;
    
    /// <summary>Gets the Physics world offset value.</summary>
    public Location WorldOffset => EngineGeneric.PhysicsWorldGeneric.Offset;

    /// <inheritdoc/>
    public override bool JointImpliesPhysicalLinkage => true;

    /// <summary>Gets the correct pose reference data for the given entity. Location is offset by <see cref="WorldOffset"/>.</summary>
    public RigidPose GetPoseFrom(BasicEntity entity)
    {
        if (VisualOnly)
        {
            return new RigidPose((entity.RenderAt - WorldOffset).ToNumerics(), entity.RenderOrientation.ToNumerics());
        }
        return new RigidPose((entity.LastKnownPosition - WorldOffset).ToNumerics(), entity.LastKnownOrientation.ToNumerics());
    }

    /// <summary>Constructs the <see cref="JointForceWeld"/>.</summary>
    public JointForceWeld(BasicEntity e1, BasicEntity e2, bool isVisualOnly = false) : base(e1, e2)
    {
        VisualOnly = isVisualOnly;
        CalculateRelativeOffset();
    }

    /// <summary>Sets the offset to match the current offset between entities.</summary>
    public void CalculateRelativeOffset()
    {
        RigidPose rt1 = GetPoseFrom(One);
        RigidPose rt2 = GetPoseFrom(Two);
        RigidPose.Invert(rt1, out RigidPose rt1inv);
        RigidPose.MultiplyWithoutOverlap(rt2, rt1inv, out Offset);
    }

    /// <summary>Value of <see cref="BasicEngine.NonPhysicalJointSolves"/> when this joint was last solved. This is a special case to allow perfect orderly solving of chained weld joints.</summary>
    public double LastSolveTime = -1;

    /// <summary>
    /// TODO: Arbitrary constants. Need proper handling decisions for these.
    /// </summary>
    public const double MinSeparationForce = 0.0001, SeparationForceFactor = 0.9;

    /// <inheritdoc/>
    public override void Solve(double delta)
    {
        if (LastSolveTime == EngineGeneric.NonPhysicalJointSolves)
        {
            return;
        }
        LastSolveTime = EngineGeneric.NonPhysicalJointSolves;
        if (One.Joints.FirstOrDefault(j => j is JointForceWeld jfw && jfw.Two == One) is JointForceWeld joint)
        {
            joint.Solve(delta);
        }
        RigidPose onePose = GetPoseFrom(One);
        RigidPose.MultiplyWithoutOverlap(Offset, onePose, out RigidPose result);
        Location pos = result.Position.ToLocation() + WorldOffset;
        if (VisualOnly)
        {
            Two.AltRenderAt = pos;
            Two.AltRenderOrientation = result.Orientation.ToCore();
        }
        else
        {
            // TODO: These should probably be stored
            if (One.TryGetProperty(out EntityPhysicsProperty physOne) && Two.TryGetProperty(out EntityPhysicsProperty physTwo))
            {
                // TODO: Figure out angular velocity stable relative transfer
                if (SendForceBack)
                {
                    // TODO: Downtrack if there's multiple forcewelds to the root and apply the force to that
                    Location separation = pos - Two.LastKnownPosition;
                    if (separation.LengthSquared() > MinSeparationForce)
                    {
                        physOne.ApplyForce(pos - One.LastKnownPosition, separation * (physTwo.Mass / delta) * SeparationForceFactor);
                    }
                }
                // TODO: Grab angular velocity
                physTwo.LinearVelocity = physOne.LinearVelocity;
            }
            Two.SetPosition(pos);
            Two.SetOrientation(result.Orientation.ToCore());
        }
    }

    /// <summary>Disables the joint, removing visual lock if relevant.</summary>
    public override void Disable()
    {
        if (VisualOnly)
        {
            Two.AltRenderAt = Location.NaN;
            Two.AltRenderOrientation = MathHelpers.Quaternion.NaN;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{base.ToString()}(OffsetPos={Offset.Position.ToLocation().ToBasicString()}, Orient={Offset.Orientation.ToCore()}, IsVisual={VisualOnly})";
    }
}
