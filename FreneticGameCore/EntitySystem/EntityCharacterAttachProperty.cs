using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Character;

namespace FreneticGameCore.EntitySystem
{   
     /// <summary>
     /// Attaches an entity to a character.
     /// </summary>
    public class EntityCharacterAttachProperty<T, T2, T3> : EntitySimpleAttachProperty<T, T2> where T : BasicEntity<T2> where T2 : BasicEngine<T, T2> where T3 : EntityPhysicsProperty<T, T2>
    {
        /// <summary>
        /// The entity this entity is attached to.
        /// </summary>
        public override T AttachedTo
        {
            get
            {
                return base.AttachedTo;
            }
            set
            {
                base.AttachedTo = value;
                Character = AttachedTo.GetProperty<T3>().OriginalObject as CharacterController;
            }
        }

        /// <summary>
        /// The character controller of <see cref="AttachedTo"/>.
        /// </summary>
        public CharacterController Character;

        /// <summary>
        /// Handles the spawn event.
        /// </summary>
        public override void OnSpawn()
        {
            base.OnSpawn();
            Entity.OnTick += Tick;
        }

        /// <summary>
        /// Fixes this entity's position based on its attachment.
        /// </summary>
        public override void FixPosition(Location position)
        {
            SetPositionOrientation(position, Quaternion.GetQuaternionBetween(Location.UnitX, new Location(Character.ViewDirection)));
        }

        /// <summary>
        /// Fixes this entity's orientation based on its attachment.
        /// Does nothing for this property.
        /// </summary>
        public override void FixOrientation(Quaternion orientation)
        {
        }

        /// <summary>
        /// Handles the tick event.
        /// </summary>
        public void Tick()
        {
            FixPosition(AttachedTo.LastKnownPosition);
        }

        /// <summary>
        /// Handles the despawn event.
        /// </summary>
        public override void OnDespawn()
        {
            base.OnDespawn();
            Entity.OnTick -= Tick;
        }
    }
}
