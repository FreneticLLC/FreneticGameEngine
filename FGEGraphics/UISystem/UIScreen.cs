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
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.ClientSystem.ViewRenderSystem;
using FGEGraphics.GraphicsHelpers;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.UISystem;

/// <summary>Represents an entire screen with any kind of graphics.</summary>
public class UIScreen : UIElement
{
    /// <summary>The width of the element or window containing this screen.</summary>
    public int ParentWidth => Parent?.Width ?? View.Engine.Window.ClientSize.X;

    /// <summary>The height of the element or window containing this screen.</summary>
    public int ParentHeight => Parent?.Height ?? View.Engine.Window.ClientSize.Y;

    /// <summary>
    /// Whether to erase the screen at the beginning of each render call.
    /// <para>Generally only used if this UI is considered the dominant central point of a view.</para>
    /// </summary>
    public bool ResetOnRender = false;

    /// <summary>Constructs a <see cref="UIScreen"/>.</summary>
    /// <param name="view">The client UI view.</param>
    /// <param name="layout">The layout of the element. If <c>null</c>, defaults to a layout covering the parent view.</param>
    public UIScreen(ViewUI2D view, UILayout layout = null) : base(UIStyling.Empty, layout)
    {
        View = view;
        IsEnabled = false;
        ScaleSize = false;
        if (layout is null)
        {
            Layout = new();
            InternalConfigureLayout();
            Layout.Element = this;
        }
    }

    /// <summary>Internal method to configure the position of the screen as fully covering the actual screen space. Called by the standard constructor.</summary>
    public void InternalConfigureLayout()
    {
        Layout.SetAnchor(UIAnchor.TOP_LEFT);
        Layout.SetPosition(0, 0);
        Layout.SetSize(() => ParentWidth, () => ParentHeight);
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        if (ResetOnRender)
        {
            GL.ClearBuffer(ClearBuffer.Color, 0, [0f, 0.5f, 0.5f, 1f]);
            GL.ClearBuffer(ClearBuffer.Depth, 0, View3DInternalData.ARR_FLOAT_1F_1);
            GraphicsUtil.CheckError("RenderScreen - Reset");
        }
    }

    /// <summary>Preps the switch to this screen.</summary>
    public virtual void SwitchTo()
    {
    }

    /// <summary>Preps the switch from this screen.</summary>
    public virtual void SwitchFrom()
    {
    }

    /// <inheritdoc/>
    public override bool CanInteract(int x, int y) => true;

    /// <inheritdoc/>
    public override void Init()
    {
        SwitchTo();
    }

    /// <inheritdoc/>
    public override void Destroy()
    {
        SwitchFrom();
    }
}
