using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents a simple container of several UI elements.
    /// </summary>
    public class UIGroup : UIElement
    {
        /// <summary>
        /// Constructs a new group.
        /// </summary>
        /// <param name="anchor">The anchor the group will be relative to.</param>
        /// <param name="width">The function to get the width.</param>
        /// <param name="height">The function to get the height.</param>
        /// <param name="xOff">The function to get the X offset.</param>
        /// <param name="yOff">The function to get the Y offset.</param>
        public UIGroup(UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff)
            : base(anchor, width, height, xOff, yOff)
        {
        }
    }
}
