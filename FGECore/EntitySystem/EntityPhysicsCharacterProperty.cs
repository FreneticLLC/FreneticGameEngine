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
using FGECore.CoreSystems;
using FGECore.EntitySystem.PhysicsHelpers;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem.BepuCharacters;
using FGECore.PropertySystem;
using BepuPhysics;

using Quaternion = FGECore.MathHelpers.Quaternion;

namespace FGECore.EntitySystem;

/// <summary>Tracks and controls a basic implementation of a humanoid character in the physics world.</summary>
public class EntityPhysicsCharacterProperty : BasicEntityProperty
{
    /// <summary>The relevant <see cref="EntityPhysicsProperty"/>.</summary>
    public EntityPhysicsProperty Physics;

    /// <summary>A getter for the underlying <see cref="CharacterControllers"/> instance.</summary>
    public CharacterControllers Characters => Physics.PhysicsWorld.Internal.Characters;

    /// <summary>A getter for a reference to the underlying <see cref="CharacterController"/> structure.</summary>
    public ref CharacterController Controller => ref Characters.GetCharacterByBodyHandle(Physics.SpawnedBody.Handle);

    /// <summary>The forward vector that the character should be facing.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public Location ViewDirection;

    /// <summary>The 2D movement vector the character is moving on (length should be less than or equal to 1).</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public Location Movement;

    /// <summary>How fast the character can move.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float SpeedStanding = 4, SpeedCrouching = 2, SpeedProne = 1;

    /// <summary>How the multiplier over body height when the character's stance changes.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float HeightModStanding = 1, HeightModCrouching = 0.5f, HeightModProne = 0.3f;

    /// <summary>The character's current speed (changes when stance does).</summary>
    public float CurrentSpeed = 4;

    /// <summary>The character's current height (changes when stance does).</summary>
    public float CurrentHeight = 2f;

    /// <summary>The character's body height.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float BodyHeight = 2f;

    /// <summary>The character's body radius.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float BodyRadius = 0.5f;

    /// <summary>How much force the character can apply while in the air (falling or jumping).</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float MaximumAerialForce = 100;

    /// <summary>The fraction of normal speed that can be achieved by moving while in the air.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float AerialVelocityFraction = 0.6f;

    /// <summary>Possible built-in character stances.</summary>
    public enum Stance
    {
        /// <summary>The character is standing normally.</summary>
        STANDING,
        /// <summary>The character is crouching.</summary>
        CROUCHING,
        /// <summary>The character is prone (laying down).</summary>
        PRONE
    }

    /// <summary>Which stance the character is currently in.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public Stance CurrentStance = Stance.STANDING;

    /// <summary>Set to true to indicate that the character should be trying to jump (will only actually jump if ground supports it).</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public bool TryingToJump = false;

    /// <summary>Changes the character's current stance.</summary>
    public void SetStance(Stance stance)
    {
        if (stance == CurrentStance)
        {
            return;
        }
        switch (stance)
        {
            case Stance.STANDING:
                SetStance(HeightModStanding, SpeedStanding);
                break;
            case Stance.CROUCHING:
                SetStance(HeightModCrouching, SpeedCrouching);
                break;
            case Stance.PRONE:
                SetStance(HeightModProne, SpeedProne);
                break;
        }
        CurrentStance = stance;
    }

    /// <summary>Changes the character's current stance details.</summary>
    public void SetStance(float heightMod, float speed)
    {
        CurrentSpeed = speed;
        CurrentHeight = heightMod;
        // TODO: Actually resize the physics body
        // TODO: Reject or delay impossible growth
    }

    /// <summary>Fired when the entity is added to the world.</summary>
    public override void OnSpawn()
    {
        if (IsSpawned)
        {
            return;
        }
        if (Physics == null && !Entity.TryGetProperty(out Physics))
        {
            Physics = new EntityPhysicsProperty()
            {
                Mass = 60,
            };
            Entity.AddProperty(Physics);
        }
        Physics.Shape = new EntityCapsuleShape(BodyRadius, BodyHeight * 0.5f, Engine.PhysicsWorldGeneric);
        Physics.OnSpawn();
        Physics.SpawnedBody.LocalInertia = new BodyInertia { InverseMass = 1f / Physics.Mass };
        ref CharacterController controller = ref Characters.AllocateCharacter(Physics.SpawnedBody.Handle);
        controller.LocalUp = Vector3.UnitZ;
        // TODO: Customizable values for these.
        controller.CosMaximumSlope = MathF.Cos(MathF.PI * 0.4f);
        controller.JumpVelocity = 6;
        controller.MaximumVerticalForce = 100 * Physics.Mass;
        controller.MaximumHorizontalForce = 80 * Physics.Mass;
        controller.MinimumSupportDepth = BodyRadius * -0.01f;
        controller.MinimumSupportContinuationDepth = -0.1f;
        MaximumAerialForce = 10 * Physics.Mass;
        Entity.OnTick += Tick;
        IsSpawned = true;
    }

    /// <summary>Whether this character is currently spawned.</summary>
    public bool IsSpawned = false;

    /// <summary>Fired when the entity is removed from the world.</summary>
    public override void OnDespawn()
    {
        if (!IsSpawned)
        {
            return;
        }
        Entity.OnTick -= Tick;
        IsSpawned = false;
    }

    /// <summary>Handles removal event.</summary>
    public override void OnRemoved()
    {
        base.OnRemoved();
    }

    /// <summary>Ticks the character.</summary>
    public void Tick()
    {
        ref CharacterController controller = ref Controller;
        Vector3 velocity = Movement.ToNumerics() * CurrentSpeed;
        if (!Physics.SpawnedBody.Awake && (TryingToJump || velocity.LengthSquared() > 0 || controller.ViewDirection != ViewDirection.ToNumerics()))
        {
            Physics.SpawnedBody.Awake = true;
        }
        controller.TargetVelocity = new Vector2(velocity.X, velocity.Y);
        controller.ViewDirection = ViewDirection.ToNumerics();
        controller.TryJump = TryingToJump;
        if (!controller.Supported && MaximumAerialForce > 0 && Physics.SpawnedBody.LocalInertia.InverseMass > 0)
        {
            Location airMoveGoal = Quaternion.GetQuaternionBetween(Location.UnitY, ViewDirection.WithZ(0).Normalize()).Transform(Movement * CurrentSpeed * AerialVelocityFraction);
            Location rawVelocity = Physics.SpawnedBody.Velocity.Linear.ToLocation();
            Location absoluteAirMoveGoal = airMoveGoal.Abs();
            Location absoluteVelocity = rawVelocity.Abs();
            Location impulse = airMoveGoal * (MaximumAerialForce * Engine.Delta);
            Location postVelocity = rawVelocity + impulse * Physics.SpawnedBody.LocalInertia.InverseMass;
            Location postVelocityAbs = postVelocity.Abs();
            if (Math.Sign(airMoveGoal.X) == Math.Sign(rawVelocity.X))
            {
                if (absoluteVelocity.X > absoluteAirMoveGoal.X)
                {
                    impulse.X = 0;
                }
                else
                {
                    if (postVelocityAbs.X > absoluteAirMoveGoal.X)
                    {
                        impulse.X = Math.Sign(impulse.X) * (absoluteAirMoveGoal.X - absoluteVelocity.X) * Physics.Mass;
                    }
                }
            }
            if (Math.Sign(airMoveGoal.Y) == Math.Sign(rawVelocity.Y) && absoluteVelocity.Y > absoluteAirMoveGoal.Y)
            {
                if (absoluteVelocity.Y > absoluteAirMoveGoal.Y)
                {
                    impulse.Y = 0;
                }
                else
                {
                    if (postVelocityAbs.Y > absoluteAirMoveGoal.Y)
                    {
                        impulse.Y = Math.Sign(impulse.Y) * (absoluteAirMoveGoal.Y - absoluteVelocity.Y) * Physics.Mass;
                    }
                }
            }
            Physics.SpawnedBody.ApplyLinearImpulse(impulse.ToNumerics());
        }
    }
}
