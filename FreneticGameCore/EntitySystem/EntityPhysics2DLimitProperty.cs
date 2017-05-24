using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.SingleEntity;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;

namespace FreneticGameCore.EntitySystem
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
            PhysEnt = BEntity.GetProperty<EntityPhysicsProperty>();
            BEntity.OnSpawnEvent += SpawnHandle;
            BEntity.OnTick += TickHandle;
            PhysEnt.DeSpawnEvent += RemoveJoints;
        }

        private PointOnPlaneJoint POPJ;

        private RevoluteAngularJoint RAJ;

        /// <summary>
        /// Post-spawn handling.
        /// </summary>
        /// <param name="e">The event.</param>
        public void SpawnHandle(EntitySpawnEventArgs e)
        {
            if (ForcePosition)
            {
                POPJ = new PointOnPlaneJoint(null, PhysEnt.SpawnedBody, Vector3.Zero, Vector3.UnitZ, Vector3.Zero);
                PhysEnt.PhysicsWorld.Internal.Add(POPJ);
            }
            RAJ = new RevoluteAngularJoint(null, PhysEnt.SpawnedBody, Vector3.UnitZ);
            PhysEnt.PhysicsWorld.Internal.Add(RAJ);
        }

        /// <summary>
        /// Handles the tick event.
        /// </summary>
        public void TickHandle()
        {
            if (ForcePosition && Math.Abs(PhysEnt.SpawnedBody.Position.Z) > 0.1)
            {
                PhysEnt.SpawnedBody.Position = new Vector3(PhysEnt.SpawnedBody.Position.X, PhysEnt.SpawnedBody.Position.Y, 0.0);
            }
        }

        /// <summary>
        /// Removes the joints from the physics world.
        /// </summary>
        public void RemoveJoints()
        {
            if (ForcePosition)
            {
                PhysEnt.PhysicsWorld.Internal.Remove(POPJ);
                POPJ = null;
            }
            PhysEnt.PhysicsWorld.Internal.Remove(RAJ);
            RAJ = null;
        }

        /// <summary>
        /// Handles the despawn event.
        /// </summary>
        public override void OnDeSpawn()
        {
            if (HandledRemove)
            {
                return;
            }
            HandledRemove = true;
            BEntity.OnSpawnEvent -= SpawnHandle;
            BEntity.OnTick -= TickHandle;
        }

        private bool HandledRemove = false;

        /// <summary>
        /// Handles removal event.
        /// </summary>
        public override void OnRemoved()
        {
            OnDeSpawn();
        }

        /// <summary>
        /// The relevant physics entity.
        /// </summary>
        public EntityPhysicsProperty PhysEnt;
    }
}
