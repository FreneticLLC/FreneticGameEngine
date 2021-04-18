using BepuPhysics.CollisionDetection;
using FGECore.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem.PhysicsHelpers
{
    /// <summary>Event that represents the collision between two physics entities.</summary>
    public abstract class CollisionEvent : EventArgs
    {
        /// <summary>The two entities colliding. One or both can potentially be null.</summary>
        public EntityPhysicsProperty One, Two;

        /// <summary>Can be set 'true' to deny the collision.</summary>
        public bool Cancel = false;

        /// <summary>How many points of contact have been generated between the two entities.</summary>
        public abstract int ContactCount { get; }

        /// <summary>Gets the normal vector from a specific context index.</summary>
        public abstract Location GetNormal(int contact);

        /// <summary>Gets the offset vector from a specific context index.</summary>
        public abstract Location GetOffset(int contact);

        /// <summary>Gets the depth from a specific context index.</summary>
        public abstract float GetDepth(int contact);
    }

    /// <summary>Implements <see cref="CollisionEvent"/>.</summary>
    public class CollisionEvent<TManifold> : CollisionEvent where TManifold : struct, IContactManifold<TManifold>
    {
        /// <summary>The internal contact manifold.</summary>
        public TManifold Manifold;

        /// <summary>Implements <see cref="CollisionEvent.ContactCount"/>.</summary>
        public override int ContactCount => Manifold.Count;

        /// <summary>Implements <see cref="CollisionEvent.GetNormal"/>.</summary>
        public override Location GetNormal(int contact)
        {
            return Manifold.GetNormal(ref Manifold, contact).ToLocation();
        }

        /// <summary>Implements <see cref="CollisionEvent.GetOffset"/>.</summary>
        public override Location GetOffset(int contact)
        {
            return Manifold.GetOffset(ref Manifold, contact).ToLocation();
        }

        /// <summary>Implements <see cref="CollisionEvent.GetDepth"/>.</summary>
        public override float GetDepth(int contact)
        {
            return Manifold.GetDepth(ref Manifold, contact);
        }
    }
}
