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
using BepuPhysics;
using FGECore.CoreSystems;
using FGECore.MathHelpers;

namespace FGECore.EntitySystem.JointSystems.NonPhysicsJoints;

/// <summary>Constrains entity Two to be force-welded to entity One, meaning that wherever One moves, Two will follow.</summary>
public class JointForceWeld : NonPhysicalJointBase
{
    /// <summary>If true, this weld is a visual only weld. If false, physical position will be modified. Only one VisualOnly joint can control an entity (ie EntityTwo) at any given time.
    /// <para>Do not modify this after construction, instead rebuild with a new value.</para></summary>
    public bool VisualOnly;

    /// <summary>The relative pose offset from entity One to entity Two.</summary>
    public RigidPose Offset;
    
    /// <summary>Gets the Physics world offset value.</summary>
    public Location WorldOffset => EngineGeneric.PhysicsWorldGeneric.Offset;

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
        RigidPose rt1 = GetPoseFrom(One);
        RigidPose rt2 = GetPoseFrom(Two);
        RigidPose.Invert(rt1, out RigidPose rt1inv);
        RigidPose.MultiplyWithoutOverlap(rt2, rt1inv, out Offset);
    }

    /// <summary>Value of <see cref="BasicEngine.GlobalTickTime"/> when this joint was last solved. This is a special case to allow perfect orderly solving of chained weld joints.</summary>
    public double LastSolveTime = -1;

    /// <summary>Implements <see cref="NonPhysicalJointBase.Solve"/>.</summary>
    public override void Solve()
    {
        if (LastSolveTime == EngineGeneric.GlobalTickTime)
        {
            return;
        }
        LastSolveTime = EngineGeneric.GlobalTickTime;
        if (One.Joints.FirstOrDefault(j => j is JointForceWeld jfw && jfw.Two == One) is JointForceWeld joint)
        {
            joint.Solve();
        }
        RigidPose onePose = GetPoseFrom(One);
        RigidPose.MultiplyWithoutOverlap(Offset, onePose, out RigidPose result);
        if (VisualOnly)
        {
            Two.AltRenderAt = result.Position.ToLocation() + WorldOffset;
            Two.AltRenderOrientation = result.Orientation.ToCore();
        }
        else
        {
            Two.SetPosition(result.Position.ToLocation() + WorldOffset);
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
