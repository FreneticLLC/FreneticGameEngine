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
using BepuPhysics;
using BepuPhysics.Collidables;

namespace FGECore.PhysicsSystem.BepuCharacters;

/// <summary>Raw data for a dynamic character controller instance.</summary>
public struct CharacterController
{
    /// <summary>Direction the character is looking in world space. Defines the forward direction for movement.</summary>
    public Vector3 ViewDirection;

    /// <summary>
    /// Target horizontal velocity.
    /// X component refers to desired velocity along the strafing direction (perpendicular to the view direction projected down to the surface),
    /// Y component refers to the desired velocity along the forward direction (aligned with the view direction projected down to the surface).
    /// </summary>
    public Vector2 TargetVelocity;

    /// <summary>If true, the character will try to jump on the next time step. Will be reset to false after being processed.</summary>
    public bool TryJump;

    /// <summary>Handle of the body associated with the character.</summary>
    public BodyHandle BodyHandle;

    /// <summary>Character's up direction in the local space of the character's body.</summary>
    public Vector3 LocalUp;

    /// <summary>Velocity at which the character pushes off the support during a jump.</summary>
    public float JumpVelocity;

    /// <summary>Maximum force the character can apply tangent to the supporting surface to move.</summary>
    public float MaximumHorizontalForce;

    /// <summary>Maximum force the character can apply to glue itself to the supporting surface.</summary>
    public float MaximumVerticalForce;

    /// <summary>Cosine of the maximum slope angle that the character can treat as a support.</summary>
    public float CosMaximumSlope;

    /// <summary>Depth threshold beyond which a contact is considered a support if it the normal allows it.</summary>
    public float MinimumSupportDepth;

    /// <summary>
    /// Depth threshold beyond which a contact is considered a support if the previous frame had support, even if it isn't deep enough to meet the MinimumSupportDepth.
    /// </summary>
    public float MinimumSupportContinuationDepth;

    /// <summary>Whether the character is currently supported.</summary>
    public bool Supported;

    /// <summary>Collidable supporting the character, if any. Only valid if Supported is true.</summary>
    public CollidableReference Support;

    /// <summary>Handle of the character's motion constraint, if any. Only valid if Supported is true.</summary>
    public ConstraintHandle MotionConstraintHandle;
}
