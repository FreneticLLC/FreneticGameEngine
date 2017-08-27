using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameGraphics.ClientSystem;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents a single generic item in a UI.
    /// <para>Sub-classes implement rendering and general logic for a specific type of UI element.</para>
    /// </summary>
    public abstract class UIElement
    {
        /// <summary>
        /// Render the element.
        /// </summary>
        /// <param name="view">The UI view.</param>
        public abstract void Render(ViewUI2D view);
    }
}
