//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Helper class for placing UI elements relative to sections of the screen.
    /// </summary>
    public class UIAnchor
    {
        /// <summary>
        /// Function for getting the relative X value.
        /// </summary>
        public readonly Func<UIElement, int> GetX;

        /// <summary>
        /// Function for getting the relative Y value.
        /// </summary>
        public readonly Func<UIElement, int> GetY;

        private UIAnchor(Func<UIElement, int> x, Func<UIElement, int> y)
        {
            GetX = x;
            GetY = y;
        }

        private static Func<UIElement, int> LEFT_X = (element) => 0;
        private static Func<UIElement, int> CENTER_X = (element) => (int)(element.Parent.GetWidth() / 2 - element.GetWidth() / 2);
        private static Func<UIElement, int> RIGHT_X = (element) => (int)(element.Parent.GetWidth() - element.GetWidth());
        private static Func<UIElement, int> TOP_Y = (element) => 0;
        private static Func<UIElement, int> CENTER_Y = (element) => (int)(element.Parent.GetHeight() / 2 - element.GetHeight() / 2);
        private static Func<UIElement, int> BOTTOM_Y = (element) => (int)(element.Parent.GetHeight() - element.GetHeight());

        /// <summary>
        /// Top left UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor TOP_LEFT = new UIAnchor(LEFT_X, TOP_Y);
        
        /// <summary>
        /// Top center UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor TOP_CENTER = new UIAnchor(CENTER_X, TOP_Y);

        /// <summary>
        /// Top right UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor TOP_RIGHT = new UIAnchor(RIGHT_X, TOP_Y);

        /// <summary>
        /// Center left UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor CENTER_LEFT = new UIAnchor(LEFT_X, CENTER_Y);

        /// <summary>
        /// Center UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor CENTER = new UIAnchor(CENTER_X, CENTER_Y);

        /// <summary>
        /// Center right UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor CENTER_RIGHT = new UIAnchor(RIGHT_X, CENTER_Y);

        /// <summary>
        /// Bottom left UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor BOTTOM_LEFT = new UIAnchor(LEFT_X, BOTTOM_Y);

        /// <summary>
        /// Bottom center UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor BOTTOM_CENTER = new UIAnchor(CENTER_X, BOTTOM_Y);

        /// <summary>
        /// Bottom right UI Anchor. See <see cref="UIAnchor"/>.
        /// </summary>
        public static readonly UIAnchor BOTTOM_RIGHT = new UIAnchor(RIGHT_X, BOTTOM_Y);
    }
}
