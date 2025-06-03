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
using FGEGraphics.GraphicsHelpers;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents a slider element that can choose between a range of real number values.
/// For a labeled number slider, use <see cref="UILabeledNumberSlider"/>.
/// </summary>
public class UINumberSlider : UIElement
{
    /// <summary>The minimum slider value.</summary>
    public double Min;

    /// <summary>The maximum slider value.</summary>
    public double Max;

    /// <summary>
    /// The grid-snapping interval. 
    /// Set to <c>0.0</c> or less for a gridless slider.
    /// </summary>
    public double Interval;

    /// <summary>Whether the slider should use integers instead of decimals.</summary>
    public bool Integer;

    /// <summary>The current slider value.</summary>
    public double Value;

    /// <summary>The current slider progress (<c>0.0</c> to <c>1.0</c>).</summary>
    public double Progress => (Value - Min) / (Max - Min);

    /// <summary>Fired when the user edits the slider value.</summary>
    public Action<double> OnValueEdit;

    /// <summary>
    /// The box placed at the current slider progress.
    /// Not actually a <see cref="UIButton"/> for better UX.
    /// </summary>
    public UIBox Button;

    /// <summary>Constructs a number slider.</summary>
    /// <param name="min">The minimum slider value.</param>
    /// <param name="max">The maximum slider value.</param>
    /// <param name="initial">The initial slider value.</param>
    /// <param name="interval">The grid-snapping interval, if any.</param>
    /// <param name="integer">Whether to use integers instead of decimals.</param>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="layout">The layout of the element.</param>
    public UINumberSlider(double min, double max, double initial, double interval, bool integer, UIInteractionStyles styles, UILayout layout) : base(layout)
    {
        Styler = styles.Styler;
        Integer = integer;
        Interval = Integer ? Math.Max((int)interval, 1.0) : interval;
        Min = Integer ? (int)min : min;
        Max = Integer ? (int)max : max;
        if (Interval > 0.0)
        {
            int maxStep = (int)((Max - Min) / Interval);
            Max = Min + Interval * maxStep;
        }
        Value = Math.Clamp(Integer ? (int)initial : initial, Min, Max);
        AddChild(Button = new(UIStyle.Empty, layout.AtOrigin().SetWidth(layout.Height / 2)) { ShouldRender = false, Enabled = false });
        Button.Layout.SetX(() => (int)(Progress * Width) - Button.Width / 2);
    }

    /// <summary>Corrects a slider value to the closest valid position.</summary>
    /// <param name="value">The uncorrected slider value.</param>
    /// <param name="interval">The grid-snapping interval.</param>
    /// <returns>The corrected slider value.</returns>
    public double GetCorrectedValue(double value, double interval)
    {
        value = Math.Clamp(value, Min, Max);
        if (interval <= 0.0)
        {
            return value;
        }
        int step = (int)Math.Round((value - Min) / interval);
        double lower = Min + interval * step;
        double higher = Min + interval * (step + 1);
        return (value - lower) <= (higher - value) ? lower : higher;
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (!Pressed)
        {
            return;
        }
        double previousValue = Value;
        double mouseValue = Math.Clamp((Window.MouseX - X) / Width, 0.0, 1.0) * (Max - Min) + Min;
        double interval = Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift) ? (Integer ? 1.0 : 0.0) : Interval;
        Value = GetCorrectedValue(mouseValue, interval);
        if (Value != previousValue)
        {
            OnValueEdit?.Invoke(Value);
        }
    }

    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIStyle style)
    {
        Engine.Textures.White.Bind();
        Renderer2D.SetColor(style.BorderColor);
        int lineWidth = style.BorderThickness / 2;
        int centerY = Y + Height / 2;
        view.Rendering.RenderRectangle(view.UIContext, X, centerY - lineWidth, X + Width, centerY + lineWidth);
        if (Interval > 0.0)
        {
            double values = (Max - Min) / Interval;
            double spacing = Width / values;
            for (int i = 0; i < (int)values + 1; i++)
            {
                int x = (int)(X + i * spacing);
                int height = Height / 6; // TODO: Make this value customizable
                view.Rendering.RenderRectangle(view.UIContext, x - lineWidth, centerY - height, x + lineWidth, centerY + height);
            }
        }
        Renderer2D.SetColor(Color4F.White);
        Button.Render(view, delta, style);
    }
}
