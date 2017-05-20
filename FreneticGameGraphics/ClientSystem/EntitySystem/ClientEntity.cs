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
        /// The property that renders this entity.
        /// </summary>
        private EntityRenderableProperty Renderable = null;

        /// <summary>
        /// Get or set the renderer for this entity. Setting will automatically add renderer as a property (if non-null).
        /// </summary>
        public EntityRenderableProperty Renderer
        {
            get
            {
                return Renderable;
            }
            set
            {
                if (Renderable != null)
                {
                    RemoveProperty(Renderable.GetType());
                }
                Renderable = value;
                if (Renderable != null)
                {
                    AddProperty(Renderable);
                }
            }
        }

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
        /// Gets a string debug helper for this entity.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return "ClientEntity of type: " + GetType().Name;
        }
    }
}
