using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// Attaches an entity to another entity.
    /// </summary>
    public class EntitySimpleAttachProperty<T, T2> : BasicEntityProperty<T, T2> where T : BasicEntity<T2> where T2 : BasicEngine<T, T2>
    {
        /// <summary>
        /// The entity this entity is attached to.
        /// </summary>
        public virtual T AttachedTo
        {
            get;
            set;
        }

        /// <summary>
        /// The relative position offset to maintain.
        /// </summary>
        public Location PosOffset;

        /// <summary>
        /// The relative orientation offset to maintain.
        /// </summary>
        public Quaternion OrientOffset;

        /// <summary>
        /// Handles the spawn event.
        /// </summary>
        public override void OnSpawn()
        {
            AttachedTo.OnPositionChanged += FixPosition;
            AttachedTo.OnOrientationChanged += FixOrientation;
            FixPosition(AttachedTo.LastKnownPosition);
            FixOrientation(AttachedTo.LastKnownOrientation);
        }

        /// <summary>
        /// Fixes this entity's position based on its attachment.
        /// </summary>
        public virtual void FixPosition(Location position)
        {
            SetPositionOrientation(position, AttachedTo.LastKnownOrientation);
        }

        /// <summary>
        /// Fixes this entity's orientation based on its attachment.
        /// </summary>
        public virtual void FixOrientation(Quaternion orientation)
        {
            SetPositionOrientation(AttachedTo.LastKnownPosition, orientation);
        }

        /// <summary>
        /// Sets this entity's position and orientation relative to <see cref="AttachedTo"/>.
        /// </summary>
        /// <param name="position">The attached-to entity's position.</param>
        /// <param name="orient">The attached-to entity's orientation.</param>
        public void SetPositionOrientation(Location position, Quaternion orient)
        {
            BEPUutilities.Matrix worldTrans = BEPUutilities.Matrix.CreateFromQuaternion(orient.ToBEPU())
                * BEPUutilities.Matrix.CreateTranslation(position.ToBVector());
            BEPUutilities.Matrix tmat = (BEPUutilities.Matrix.CreateFromQuaternion(OrientOffset.ToBEPU())
                * BEPUutilities.Matrix.CreateTranslation(PosOffset.ToBVector()))
                * worldTrans;
            Location pos = new Location(tmat.Translation);
            Quaternion quat = BEPUutilities.Quaternion.CreateFromRotationMatrix(tmat).ToCore();
            Entity.SetPosition(pos);
            Entity.SetOrientation(quat);
        }

        /// <summary>
        /// Handles the despawn event.
        /// </summary>
        public override void OnDespawn()
        {
            AttachedTo.OnPositionChanged -= FixPosition;
            AttachedTo.OnOrientationChanged -= FixOrientation;
        }
    }
}
