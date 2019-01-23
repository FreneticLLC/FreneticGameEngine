using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;
using FreneticGameCore.UtilitySystems;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// Attaches an entity to another entity.
    /// </summary>
    public class EntitySimpleAttachProperty<T, T2> : BasicEntityProperty<T, T2> where T : BasicEntity<T, T2> where T2 : BasicEngine<T, T2>
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
        /// Set the relative offset to the current relative locations and orientation.
        /// </summary>
        public virtual void SetRelativeToCurrent()
        {
            SetRelativeBasedOn(AttachedTo.LastKnownOrientation, AttachedTo.LastKnownPosition);
        }

        /// <summary>
        /// Sets the relative offset based on the attached properties.
        /// </summary>
        /// <param name="orient">The attached orientation.</param>
        /// <param name="pos">The attached position.</param>
        public void SetRelativeBasedOn(Quaternion orient, Location pos)
        {
            Matrix worldTrans = Matrix.CreateFromQuaternion(orient.ToBEPU()) * Matrix.CreateTranslation(pos.ToBVector());
            Matrix.Invert(ref worldTrans, out Matrix inverted);
            RelativeOffset = Matrix.CreateFromQuaternion(Entity.LastKnownOrientation.ToBEPU()) * Matrix.CreateTranslation(Entity.LastKnownPosition.ToBVector()) * inverted;
        }

        /// <summary>
        /// The relative offset matrix offset to maintain.
        /// </summary>
        public Matrix RelativeOffset = Matrix.Identity;

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
            Matrix worldTrans = Matrix.CreateFromQuaternion(orient.ToBEPU()) * Matrix.CreateTranslation(position.ToBVector());
            Matrix tmat = RelativeOffset * worldTrans;
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
