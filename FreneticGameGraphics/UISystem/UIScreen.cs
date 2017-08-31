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
using FreneticGameGraphics.ClientSystem;
using FreneticGameGraphics.GraphicsHelpers;
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents an entire screen with any kind of graphics.
    /// </summary>
    public class UIScreen : UIElement
    {
        /// <summary>
        /// A reference to the relevant client backing this screen.
        /// Get this using <see cref="Client"/>.
        /// </summary>
        private GameClientWindow _Client;

        /// <summary>
        /// Gets the client game engine this screen is associated with.
        /// </summary>
        public override GameEngineBase Engine
        {
            get
            {
                return _Client.CurrentEngine;
            }
        }

        /// <summary>
        /// Gets the client game window this screen is associated with.
        /// </summary>
        public override GameClientWindow Client
        {
            get
            {
                return _Client;
            }
        }

        /// <summary>
        /// Whether to erase the screen at the beginning of each render call.
        /// <para>Generally only used if this UI is considered the dominant central point of a view.</para>
        /// </summary>
        protected bool ResetOnRender = false;

        /// <summary>
        /// Constructs a screen that covers the entire game window.
        /// </summary>
        /// <param name="client">The client game window.</param>
        public UIScreen(GameClientWindow client) : this(client, UIAnchor.TOP_LEFT, () => 0, () => 0, () => 0, () => 0)
        {
            Width = () => Parent == null ? Engine.Window.Width : Parent.GetWidth();
            Height = () => Parent == null ? Engine.Window.Height : Parent.GetHeight();
        }

        /// <summary>
        /// Constructs a screen that covers a specific portion of the game window.
        /// </summary>
        /// <param name="client">The client game window.</param>
        /// <param name="anchor">The anchor the element will be positioned relative to.</param>
        /// <param name="width">The function that controls the width of the element.</param>
        /// <param name="height">The function that controls the height of the element.</param>
        /// <param name="xOff">The function that controls the X offset of the element.</param>
        /// <param name="yOff">The function that controls the Y offset of the element.</param>
        public UIScreen(GameClientWindow client, UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff) : base(anchor, width, height, xOff, yOff)
        {
            _Client = client;
        }

        /// <summary>
        /// Performs a tick on all children of this screen.
        /// </summary>
        /// <param name="delta">The time since the last tick.</param>
        protected override void TickChildren(double delta)
        {
            base.TickChildren(delta);
        }

        /// <summary>
        /// Performs a render on all children of this screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        /// <param name="xoff">The X offset of this element's parent.</param>
        /// <param name="yoff">The Y offset of this element's parent.</param>
        protected override void RenderChildren(ViewUI2D view, double delta, int xoff, int yoff)
        {
            if (ResetOnRender)
            {
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0.5f, 0.5f, 1f });
                GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                GraphicsUtil.CheckError("RenderScreen - Reset");
            }
            base.RenderChildren(view, delta, xoff, yoff);
            GraphicsUtil.CheckError("RenderScreen - Children");
        }

        /// <summary>
        /// Preps the switch to this screen.
        /// </summary>
        public virtual void SwitchTo()
        {
        }

        /// <summary>
        /// Preps the switch from this screen.
        /// </summary>
        public virtual void SwitchFrom()
        {
        }
    }
}
