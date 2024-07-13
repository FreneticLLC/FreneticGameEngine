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

namespace FGEGraphics.UISystem;

/// <summary>Represents a <see cref="UINumberSlider"/> element with an editable number label alongside the interactable area.</summary>
public class UILabeledNumberSlider : UINumberSlider
{
    /// <summary>The slider's value label.</summary>
    public UINumberInputLabel Label;

    /// <summary>Constructs a labeled number slider.</summary>
    /// <param name="min">The minimum slider value.</param>
    /// <param name="max">The maximum slider value.</param>
    /// <param name="initial">The initial slider value.</param>
    /// <param name="interval">The grid-snapping interval, if any.</param>
    /// <param name="integer">Whether to use integers instead of decimals.</param>
    /// <param name="sliderStyles">The slider styles.</param>
    /// <param name="labelLeft">Whether the label should be on the left of the slider.</param>
    /// <param name="labelPadding">The horizontal spacing between the label and the slider.</param>
    /// <param name="labelInputStyle">The normal label style.</param>
    /// <param name="labelHighlightStyle">The highlighted label style.</param>
    /// <param name="pos">The position of the slider.</param>
    /// <param name="labelFormat">A format string for the label value.</param>
    /// <param name="updateOnEdit">Whether to update the slider value while the user edits the label.</param>
    /// <param name="labelRenderBox">Whether to render a box behind the number label.</param>
    /// <param name="labelBoxPadding">The padding between the box and the number label.</param>
    /// <param name="labelBoxStyles">The box styles for the number label.</param>
    public UILabeledNumberSlider(double min, double max, double initial, double interval, bool integer, StyleGroup sliderStyles, bool labelLeft, int labelPadding, UIElementStyle labelInputStyle, UIElementStyle labelHighlightStyle, UIPositionHelper pos, string labelFormat = null, bool updateOnEdit = true, bool labelRenderBox = false, int labelBoxPadding = 0, StyleGroup labelBoxStyles = null) : base(min, max, initial, interval, integer, sliderStyles, pos)
    {
        labelFormat ??= Integer ? "0" : "0.0";
        AddStyle(labelInputStyle, true);
        AddChild(Label = new UINumberInputLabel(Value, integer, labelFormat, labelInputStyle, labelHighlightStyle, pos.AtOrigin(), labelRenderBox, labelBoxPadding, labelBoxStyles));
        // FIXME: Using labelLeft, when dimensions change, pos not updated until one frame later
        // (This won't be an issue with the TextAlignment replacement in UIPositionHelper, presumably)
        Label.Position.GetterXY(() => labelLeft ? -labelPadding - Label.Width : pos.Width + labelPadding, () => (pos.Height - Label.Height) / 2);
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
