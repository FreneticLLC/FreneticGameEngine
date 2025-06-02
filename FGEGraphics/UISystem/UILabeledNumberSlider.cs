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

namespace FGEGraphics.UISystem;

/// <summary>Represents a <see cref="UINumberSlider"/> element with an editable number label alongside the interactable area.</summary>
public class UILabeledNumberSlider : UINumberSlider
{
    // TODO: Vertically center text
    /// <summary>The slider's value label.</summary>
    public UINumberInputLabel Label;

    // TODO: accept a number label instead of having an enormous constructor?
    /// <summary>Constructs a labeled number slider.</summary>
    /// <param name="min">The minimum slider value.</param>
    /// <param name="max">The maximum slider value.</param>
    /// <param name="initial">The initial slider value.</param>
    /// <param name="interval">The grid-snapping interval, if any.</param>
    /// <param name="integer">Whether to use integers instead of decimals.</param>
    /// <param name="baseStyles">The base styles for both the slider and label.</param>
    /// <param name="labelLeft">Whether the label should be on the left of the slider.</param>
    /// <param name="labelPadding">The horizontal spacing between the label and the slider.</param>
    /// <param name="labelInputStyle">The normal label style.</param>
    /// <param name="labelHighlightStyle">The highlighted label style.</param>
    /// <param name="pos">The position of the slider.</param>
    /// <param name="labelFormat">A format string for the label value.</param>
    /// <param name="updateOnEdit">Whether to update the slider value while the user edits the label.</param>
    /// <param name="labelRenderBox">Whether to render a box behind the number label.</param>
    /// <param name="labelBoxPadding">The padding between the box and the number label.</param>
    /// <param name="labelScrollBarStyles">The styles for the label scroll bar.</param>
    /// <param name="labelScrollBarWidth">The width of the label scroll bar.</param>
    /// <param name="labelScrollBarX">Whether to add a horizontal label scroll bar.</param>
    /// <param name="labelScrollBarXAnchor">The anchor of the horizontal label scroll bar.</param>
    public UILabeledNumberSlider(double min, double max, double initial, double interval, bool integer, StyleGroup baseStyles, bool labelLeft, int labelPadding, UIElementStyle labelInputStyle, UIElementStyle labelHighlightStyle, UILayout pos, string labelFormat = null, bool updateOnEdit = true, bool labelRenderBox = false, int labelBoxPadding = 0, StyleGroup labelScrollBarStyles = null, int labelScrollBarWidth = 0, bool labelScrollBarX = false, UIAnchor labelScrollBarXAnchor = null) : base(min, max, initial, interval, integer, baseStyles, pos)
    {
        AddStyle(labelInputStyle, true);
        AddChild(Label = new UINumberInputLabel(integer, baseStyles, labelInputStyle, labelHighlightStyle, pos.AtOrigin(), Value, labelFormat, "", labelRenderBox, labelBoxPadding, labelScrollBarStyles, labelScrollBarWidth, labelScrollBarX, labelScrollBarXAnchor));
        Label.Layout.SetPosition(() => labelLeft ? -labelPadding - Label.Width : pos.Width + labelPadding, () => (pos.Height - Label.Height) / 2);
        OnValueEdit += _ => Label.TextContent = Value.ToString(Label.Format);
        Label.OnTextSubmit += _ =>
        {
            double newValue = GetCorrectedValue(Label.Value, Integer ? 1.0 : 0.0);
            OnValueEdit.Invoke(Value = newValue);
        };
        if (updateOnEdit)
        {
            Label.OnTextEdit += _ => Value = double.TryParse(Label.TextContent, out double value) ? Math.Clamp(value, Min, Max) : initial;
        }
    }
}
