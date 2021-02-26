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
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Character;
using BEPUutilities.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using FGECore.CoreSystems;
using FGECore.PropertySystem;

namespace FGECore.EntitySystem
{
    /// <summary>
    /// Represents a physics character's advanced options.
    /// <para>ONLY ADD TO CHARACTER PHYSICS ENTITIES.</para>
    /// </summary>
    public class EntityPhysicsCharacterProperty : BasicEntityProperty
    {
        /// <summary>
        /// Whether to automatically correct friction.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool AutoFriction = true;

        /// <summary>
        /// Handles the spawn event.
        /// </summary>
        public override void OnSpawn()
        {
            PhysEnt = Entity.GetProperty<EntityPhysicsProperty>();
            Entity.OnSpawnEvent.AddEvent(SpawnHandle, this, 0);
            Entity.OnTick += TickHandle;
        }

        /// <summary>
        /// The ground attachment force of the character.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double StandardGroundForce = double.NaN;
        
        /// <summary>
        /// Post-spawn handling.
        /// </summary>
        /// <param name="e">The event.</param>
        public void SpawnHandle(EntitySpawnEventArgs e)
        {
            Character = PhysEnt.OriginalObject as CharacterController;
            if (double.IsNaN(StandardGroundForce))
            {
                StandardGroundForce = Character.TractionForce;
            }
        }

        /// <summary>
        /// Handles the tick event.
        /// </summary>
        public void TickHandle()
        {
            if (AutoFriction)
            {
                HandleFriction();
            }
        }

        /// <summary>
        /// Handles the friction update in a tick.
        /// </summary>
        public void HandleFriction()
        {
            double fricto = 0;
            int c = 0;
            ReadOnlyList<CollidablePairHandler> list = Character.Body.CollisionInformation.Pairs;
            foreach (CollidablePairHandler cph in list)
            {
                if (cph.Colliding && cph is StandardPairHandler sph)
                {
                    if (cph.EntityA == Character.Body)
                    {
                        fricto += cph.EntityB.Material.KineticFriction;
                        c++;
                    }
                    else if (cph.EntityB == Character.Body)
                    {
                        fricto += cph.EntityA.Material.KineticFriction;
                        c++;
                    }
                }
            }
            if (c > 0)
            {
                // TODO: Estimate Traction from Friction better?
                Character.TractionForce = StandardGroundForce * 2.0 * fricto / c;
            }
            else
            {
                Character.TractionForce = StandardGroundForce;
            }
        }
        
        /// <summary>
        /// Handles the despawn event.
        /// </summary>
        public override void OnDespawn()
        {
            Entity.OnSpawnEvent.RemoveBySource(this);
            Entity.OnTick -= TickHandle;
        }

        /// <summary>
        /// The internal character.
        /// </summary>
        public CharacterController Character;

        /// <summary>
        /// The relevant physics entity.
        /// </summary>
        public EntityPhysicsProperty PhysEnt;
    }
}
