using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using FreneticGameCore.EntitySystem;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Represents an entity on the client side.
    /// </summary>
    public class ClientEntity : BasicEntity
    {
        /// <summary>
        /// Get or set the renderer for this entity.
        /// Adding or removing a renderable will set this value.
        /// </summary>
        public EntityRenderableProperty Renderer = null;
        
        // TODO: ClientWindow Client;

        /// <summary>
        /// Constructs a client-side entity.
        /// </summary>
        /// <param name="_ticks">Whether it should tick.</param>
        public ClientEntity(bool _ticks)
            : base(_ticks)
        {
        }

        /// <summary>
        /// Called when a property is added.
        /// </summary>
        /// <param name="prop">The property.</param>
        public override void OnAdded(Property prop)
        {
            base.OnAdded(prop);
            if (Renderer == null && prop is EntityRenderableProperty rnd)
            {
                Renderer = rnd;
            }
        }

        /// <summary>
        /// Called when a property is removed.
        /// </summary>
        /// <param name="prop">The property.</param>
        public override void OnRemoved(Property prop)
        {
            base.OnRemoved(prop);
            if (prop == Renderer)
            {
                Renderer = null;
            }
        }

        /// <summary>
        /// Gets a string debug helper for this entity.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return "ClientEntity of type: " + GetType().Name;
        }
    }
}
