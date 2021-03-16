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
using FGECore.PropertySystem;
using BepuPhysics.Constraints;

namespace FGECore.EntitySystem
{
    /// <summary>
    /// Restricts a physics entity to 2D only.
    /// </summary>
    public class EntityPhysics2DLimitProperty : BasicEntityProperty
    {
        /// <summary>
        /// Whether to force the position (in addition to rotation).
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool ForcePosition = true;

        /// <summary>
        /// Handles the spawn event.
        /// </summary>
        public override void OnSpawn()
        {
            PhysEnt = Entity.GetProperty<EntityPhysicsProperty>();
            Entity.OnSpawnEvent.AddEvent(SpawnHandle, this, 0);
            Entity.OnTick += TickHandle;
            PhysEnt.DespawnEvent += RemoveJoints;
        }

        // TODO: Reimplement.
        //private PointOnPlaneJoint POPJ;

        //private RevoluteAngularJoint RAJ;

        /// <summary>
        /// Post-spawn handling.
        /// </summary>
        /// <param name="e">The event.</param>
        public void SpawnHandle(EntitySpawnEventArgs e)
        {
            if (ForcePosition)
            {
                //POPJ = new PointOnPlaneJoint(null, PhysEnt.SpawnedBody, Vector3.Zero, Vector3.UnitZ, Vector3.Zero);
                //PhysEnt.PhysicsWorld.Internal.Add(POPJ);
            }
            //RAJ = new RevoluteAngularJoint(null, PhysEnt.SpawnedBody, Vector3.UnitZ);
            //PhysEnt.PhysicsWorld.Internal.Add(RAJ);
        }

        /// <summary>
        /// Handles the tick event.
        /// </summary>
        public void TickHandle()
        {
            if (ForcePosition && Math.Abs(PhysEnt.SpawnedBody.Pose.Position.Z) > 0.1)
            {
                PhysEnt.SpawnedBody.Pose.Position = new Vector3(PhysEnt.SpawnedBody.Pose.Position.X, PhysEnt.SpawnedBody.Pose.Position.Y, 0.0f);
            }
        }

        /// <summary>
        /// Removes the joints from the physics world.
        /// </summary>
        public void RemoveJoints()
        {
            if (ForcePosition)
            {
                //PhysEnt.PhysicsWorld.Internal.Remove(POPJ);
                //POPJ = null;
            }
            //PhysEnt.PhysicsWorld.Internal.Remove(RAJ);
            //RAJ = null;
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
        /// The relevant physics entity.
        /// </summary>
        public EntityPhysicsProperty PhysEnt;
    }
}
