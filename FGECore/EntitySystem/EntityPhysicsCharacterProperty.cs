//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using BepuPhysics;
using FGECore.EntitySystem.PhysicsHelpers;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem.BepuCharacters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem
{
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
        public Location ViewDirection;

        /// <summary>The 2D movement vector the character is moving on (length should be less than or equal to 1).</summary>
        public Location Movement;

        /// <summary>How fast the character can move.</summary>
        public float Speed = 4;

        /// <summary>Set to true to indicate that the character should be trying to jump (will only actually jump if ground supports it).</summary>
        public bool TryingToJump = false;

        /// <summary>Fired when the entity is added to the world.</summary>
        public override void OnSpawn()
        {
            if (IsSpawned)
            {
                return;
            }
            float mass = 60;
            float radius = 0.5f;
            if (Physics == null && !Entity.TryGetProperty(out Physics))
            {
                Physics = new EntityPhysicsProperty()
                {
                    Mass = mass,
                    Shape = new EntityCapsuleShape(radius, 1f, Engine.PhysicsWorldGeneric)
                };
                Entity.AddProperty(Physics);
            }
            Physics.OnSpawn();
            Physics.SpawnedBody.LocalInertia = new BodyInertia { InverseMass = 1f / mass };
            ref CharacterController controller = ref Characters.AllocateCharacter(Physics.SpawnedBody.Handle);
            controller.LocalUp = Vector3.UnitZ;
            // TODO: Customizable values for these.
            controller.CosMaximumSlope = MathF.Cos(MathF.PI * 0.4f);
            controller.JumpVelocity = 6;
            controller.MaximumVerticalForce = 100 * mass;
            controller.MaximumHorizontalForce = 20 * mass;
            controller.MinimumSupportDepth = radius * -0.01f;
            controller.MinimumSupportContinuationDepth = -0.1f;
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
            Vector3 velocity = Movement.ToNumerics() * Speed;
            if (!Physics.SpawnedBody.Awake && (TryingToJump || velocity.LengthSquared() > 0 || controller.ViewDirection != ViewDirection.ToNumerics()))
            {
                Physics.SpawnedBody.Awake = true;
            }
            controller.TargetVelocity = new Vector2(velocity.X, velocity.Y);
            controller.ViewDirection = ViewDirection.ToNumerics();
            controller.TryJump = TryingToJump;
            if (!controller.Supported)
            {
                // TODO: In-air movement
            }
        }
    }
}
