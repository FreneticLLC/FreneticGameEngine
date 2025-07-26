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
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

/// <summary>Represents a slider element that can choose between a range of real number values.</summary>
public class UINumberSlider : UIElement
{
    /// <summary>The minimum slider value.</summary>
    public double Min;

    /// <summary>The maximum slider value.</summary>
    public double Max;

    /// <summary>The default slider value.</summary>
    public double Default;

    /// <summary>The grid-snapping interval. Set to <c>0.0</c> or less for a gridless slider.</summary>
    public double Interval;

    /// <summary>Whether the slider should use integers instead of decimals.</summary>
    public bool Integer;

    /// <summary>The current slider value.</summary>
    public double Value;

    /// <summary>The current slider progress (<c>0.0</c> to <c>1.0</c>).</summary>
    public double Progress => (Value - Min) / (Max - Min);

    /// <summary>The box placed at the current slider progress.</summary>
    public UIBox Button;

    /// <summary>Fired when the user edits the slider value.</summary>
    public Action<double> OnValueEdit;

    /// <summary>Constructs a number slider.</summary>
    /// <param name="min">The minimum slider value.</param>
    /// <param name="max">The maximum slider value.</param>
    /// <param name="defaultValue">The default slider value.</param>
    /// <param name="interval">The grid-snapping interval, if any.</param>
    /// <param name="integer">Whether to use integers instead of decimals.</param>
    /// <param name="styling">The clickable styles.</param>
    /// <param name="layout">The layout of the element.</param>
    public UINumberSlider(double min, double max, double defaultValue, double interval, bool integer, UIStyling styling, UILayout layout) : base(styling,layout)
    {
        Integer = integer;
        Interval = Integer ? Math.Max((int)interval, 1.0) : interval;
        Min = Integer ? (int)min : min;
        Max = Integer ? (int)max : max;
        Default = defaultValue;
        if (Interval > 0.0)
        {
            int maxStep = (int)((Max - Min) / Interval);
            Max = Min + Interval * maxStep;
        }
        Value = Math.Clamp(Integer ? (int)Default : Default, Min, Max); // TODO: is this correct?
        AddChild(Button = new(UIStyle.Empty, layout.AtOrigin().SetWidth(layout.Height / 2)) { RenderSelf = false, IsEnabled = false });
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
        if (!IsPressed)
        {
            return;
        }
        double previousValue = Value;
        double mouseValue = Math.Clamp((View.Client.MouseX - X) / Width, 0.0, 1.0) * (Max - Min) + Min;
        double interval = View.Client.Window.KeyboardState.IsKeyDown(Keys.LeftShift) ? (Integer ? 1.0 : 0.0) : Interval;
        Value = GetCorrectedValue(mouseValue, interval);
        if (Value != previousValue)
        {
            OnValueEdit?.Invoke(Value);
        }
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        View.Engine.Textures.White.Bind();
        Renderer2D.SetColor(style.BorderColor);
        int lineWidth = style.BorderThickness / 2;
        int centerY = Y + Height / 2;
        View.Rendering.RenderRectangle(View.UIContext, X, centerY - lineWidth, X + Width, centerY + lineWidth);
        if (Interval > 0.0)
        {
            double values = (Max - Min) / Interval;
            double spacing = Width / values;
            for (int i = 0; i < (int)values + 1; i++)
            {
                int x = (int)(X + i * spacing);
                int height = Height / 6; // TODO: Make this value customizable
                View.Rendering.RenderRectangle(View.UIContext, x - lineWidth, centerY - height, x + lineWidth, centerY + height);
            }
        }
        Renderer2D.SetColor(Color4F.White);
        Button.Render(delta, style);
    }

    /// <summary>Joins a number slider and a number input label into a single unit.</summary>
    /// <param name="slider">The number slider.</param>
    /// <param name="label">The number input label.</param>
    /// <param name="spacing">The space between the slider and the label.</param>
    /// <param name="layout">The layout of the element.</param>
    /// <param name="listAnchor">The anchor to use when positioning the slider and the label in a list.</param>
    /// <param name="trackLabelEdits">Whether to update the number slider when the label is momentarily edited.</param>
    /// <returns>A list group containing both elements.</returns>
    public static UIListGroup WithLabel(UINumberSlider slider, UINumberInputLabel label, int spacing, UILayout layout, UIAnchor listAnchor = null, bool trackLabelEdits = false)
    {
        UIListGroup list = new(spacing, layout, vertical: false, anchor: listAnchor ?? UIAnchor.TOP_LEFT);
        slider.OnValueEdit += _ => label.TextContent = slider.Value.ToString(label.Format);
        label.OnTextSubmit += _ =>
        {
            double newValue = slider.GetCorrectedValue(label.Value, slider.Integer ? 1.0 : 0.0);
            slider.OnValueEdit?.Invoke(slider.Value = newValue);
        };
        if (trackLabelEdits)
        {
            label.OnTextEdit += _ => slider.Value = double.TryParse(label.TextContent, out double value) ? Math.Clamp(value, slider.Min, slider.Max) : slider.Default;
        }
        label.PlaceholderInfo.Content = null;
        label.TextContent = slider.Default.ToString(label.Format);
        list.AddListItem(slider);
        list.AddListItem(label);
        return list;
    }
}
