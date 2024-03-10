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

/// <summary>
/// Represents a <see cref="UINumberSlider"/> element with an editable number
/// label alongside the interactable area.
/// </summary>
public class UILabeledNumberSlider : UINumberSlider
{
    /// <summary>The slider's value label.</summary>
    // TODO: Make editable
    public UILabel Label;

    /// <summary>Constructs a labeled number slider.</summary>
    /// <param name="min">The minimum slider value.</param>
    /// <param name="max">The maximum slider value.</param>
    /// <param name="initial">The initial slider value.</param>
    /// <param name="isInt">Whether to use integers instead of decimals.</param>
    /// <param name="sliderStyles">The slider styles.</param>
    /// <param name="labelLeft">Whether the label should be on the left of the slider.</param>
    /// <param name="labelPadding">The horizontal spacing between the label and the slider.</param>
    /// <param name="labelStyle">The label style.</param>
    /// <param name="pos">The position of the slider.</param>
    public UILabeledNumberSlider(double min, double max, double initial, bool isInt, StyleGroup sliderStyles, bool labelLeft, int labelPadding, UIElementStyle labelStyle, UIPositionHelper pos) : base(min, max, initial, isInt, sliderStyles, pos)
    {
        AddStyle(labelStyle, true);
        AddChild(Label = new UILabel(string.Empty, labelStyle, pos.AtOrigin()));
        // FIXME: Using labelLeft, when dimensions change, pos not updated until one frame later
        // (This won't be an issue with the TextAlignment replacement in UIPositionHelper, presumably)
        Label.Position.GetterXY(() => labelLeft ? -labelPadding - Label.Width : pos.Width + labelPadding, () => (pos.Height - Label.Height) / 2);
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        Label.Text.Content = $"{Value:0.0}";
    }
}
