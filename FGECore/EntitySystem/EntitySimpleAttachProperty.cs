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
using BEPUutilities;
using FGECore.UtilitySystems;
using FGECore.PhysicsSystem;
using FGECore.MathHelpers;
using FGECore.CoreSystems;

namespace FGECore.EntitySystem
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
        public void SetRelativeBasedOn(MathHelpers.Quaternion orient, Location pos)
        {
            Matrix worldTrans = Matrix.CreateFromQuaternion(orient.ToBEPU()) * Matrix.CreateTranslation(pos.ToBEPU());
            Matrix.Invert(ref worldTrans, out Matrix inverted);
            RelativeOffset = Matrix.CreateFromQuaternion(Entity.LastKnownOrientation.ToBEPU()) * Matrix.CreateTranslation(Entity.LastKnownPosition.ToBEPU()) * inverted;
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
        public virtual void FixOrientation(MathHelpers.Quaternion orientation)
        {
            SetPositionOrientation(AttachedTo.LastKnownPosition, orientation);
        }
        
        /// <summary>
        /// Sets this entity's position and orientation relative to <see cref="AttachedTo"/>.
        /// </summary>
        /// <param name="position">The attached-to entity's position.</param>
        /// <param name="orient">The attached-to entity's orientation.</param>
        public void SetPositionOrientation(Location position, MathHelpers.Quaternion orient)
        {
            Matrix worldTrans = Matrix.CreateFromQuaternion(orient.ToBEPU()) * Matrix.CreateTranslation(position.ToBEPU());
            Matrix tmat = RelativeOffset * worldTrans;
            Location pos = tmat.Translation.ToLocation();
            MathHelpers.Quaternion quat = BEPUutilities.Quaternion.CreateFromRotationMatrix(tmat).ToCore();
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
