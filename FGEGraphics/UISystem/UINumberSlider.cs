//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the Voxalia source root for the contents of the license.
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
using FGEGraphics.GraphicsHelpers;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents a slider element that can choose between a range of real number values.
/// For a labeled number slider, use <see cref="UILabeledNumberSlider"/>.
/// </summary>
public class UINumberSlider : UIClickableElement
{
    /// <summary>The minimum slider value.</summary>
    public double Min;

    /// <summary>The maximum slider value.</summary>
    public double Max;

    /// <summary>Whether to use an integer grid instead of decimals.</summary>
    public bool IsInt;

    /// <summary>The current slider value.</summary>
    public double Value;

    /// <summary>The current slider progress (<c>0.0</c> to <c>1.0</c>).</summary>
    public double Progress => (Value - Min) / (Max - Min);

    /// <summary>
    /// The box placed at the current slider progress.
    /// Not actually a <see cref="UIButton"/> for better UX.
    /// </summary>
    public UIBox Button;

    /// <summary>Constructs a number slider.</summary>
    /// <param name="isInt">Whether to use integers instead of decimals.</param>
    /// <param name="min">The minimum slider value.</param>
    /// <param name="max">The maximum slider value.</param>
    /// <param name="initial">The initial slider value.</param>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="pos">The position of the element.</param>
    public UINumberSlider(double min, double max, double initial, bool isInt, StyleGroup styles, UIPositionHelper pos) : base(styles, pos, false, null)
    {
        Min = min;
        Max = max;
        IsInt = isInt;
        Value = IsInt ? (int)initial : initial;
        AddChild(Button = new(UIElementStyle.Empty, pos.AtOrigin().ConstantWidth(pos.Height / 2), false));
        Button.Position.GetterX(() => (int)(Progress * Width) - Button.Width / 2);
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (!Pressed)
        {
            return;
        }
        Value = Math.Clamp((Window.MouseX - X) / Width, 0.0, 1.0) * (Max - Min) + Min;
        if (IsInt)
        {
            Value = Math.Round(Value);
        }
    }

    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        Engine.Textures.White.Bind();
        Renderer2D.SetColor(Styles.Normal.BorderColor);
        int lineWidth = style.BorderThickness / 2;
        int centerY = Y + Height / 2;
        view.Rendering.RenderRectangle(view.UIContext, X, centerY - lineWidth, X + Width, centerY + lineWidth);
        if (IsInt)
        {
            int values = (int)(Max - Min);
            int spacing = Width / values;
            for (int i = 0; i < values + 1; i++)
            {
                int x = X + i * spacing;
                int height = Height / 6; // TODO: Make this value customizable
                view.Rendering.RenderRectangle(view.UIContext, x - lineWidth, centerY - height, x + lineWidth, centerY + height);
            }
        }
        Renderer2D.SetColor(Color4F.White);
        Button.Render(view, delta, style);
    }
}
