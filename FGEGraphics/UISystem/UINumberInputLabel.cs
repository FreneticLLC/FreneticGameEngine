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
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.CoreSystems;

namespace FGEGraphics.UISystem;

/// <summary>Represents an editable number label.</summary>
public class UINumberInputLabel : UIInputLabel
{
    /// <summary>Character matcher for integer number labels.</summary>
    public static readonly AsciiMatcher IntegerMatcher = new(AsciiMatcher.Digits + "-");

    /// <summary>Character matcher for decimal number labels.</summary>
    public static readonly AsciiMatcher DecimalMatcher = new(AsciiMatcher.Digits + "-.e");

    /// <summary>Whether the label should be an integer instead of a decimal.</summary>
    public bool Integer;

    /// <summary>The format string to apply to the label on submission.</summary>
    public string Format;

    /// <summary>The character matcher for this number label type.</summary>
    public AsciiMatcher CharacterMatcher => Integer ? IntegerMatcher : DecimalMatcher;

    /// <summary>Data internal to a <see cref="UINumberInputLabel"/> instance.</summary>
    public struct NumberLabelInternalData
    {
        // FIXME: Cached value recalculated when selected/deselected w/o edit
        /// <summary>The raw decimal value of the label.</summary>
        public double Value;
    }

    /// <summary>Data internal to a <see cref="UINumberInputLabel"/> instance.</summary>
    public NumberLabelInternalData NumberLabelInternal = new();

    /// <summary>Gets or sets the decimal value of the label.</summary>
    public double Value
    {
        get => NumberLabelInternal.Value;
        set
        {
            NumberLabelInternal.Value = value;
            TextContent = value.ToString(Format);
        }
    }

    /// <remarks>Constructs a number input label.</remarks>
    /// <param name="integer">Whether the label should be an integer.</param>
    /// <param name="baseStyles">The clickable styles for the box and info text.</param>
    /// <param name="inputStyle">The style of normal input content.</param>
    /// <param name="highlightStyle">The style of highlighted input content.</param>
    /// <param name="pos">The position of the element.</param>
    /// <param name="initial">The initial number value.</param>
    /// <param name="format">The format string for the label.</param>
    /// <param name="placeholderInfo">The text to display when the input is empty.</param>
    /// <param name="renderBox">Whether to render a box behind the label.</param>
    /// <param name="boxPadding">The padding between the box and the label.</param>
    /// <param name="scrollBarStyles">The styles for the scroll bar.</param>
    /// <param name="scrollBarWidth">The width of the scroll bar.</param>
    /// <param name="scrollBarX">Whether to add a horizontal scroll bar.</param>
    /// <param name="scrollBarXAnchor">The anchor of the horizontal scroll bar.</param>
    public UINumberInputLabel(bool integer, StyleGroup baseStyles, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos, double initial = 0, string format = null, string placeholderInfo = "", bool renderBox = false, int boxPadding = 0, StyleGroup scrollBarStyles = null, int scrollBarWidth = 0, bool scrollBarX = false, UIAnchor scrollBarXAnchor = null) : base(placeholderInfo, placeholderInfo.Length == 0 ? initial.ToString(format) : "", baseStyles, inputStyle, highlightStyle, pos, false, renderBox, boxPadding, scrollBarStyles, scrollBarWidth, scrollBarX, false, scrollBarXAnchor, null)
    {
        Integer = integer;
        Format = format ?? (integer ? "0" : "0.0");
        Multiline = false;
    }

    /// <inheritdoc/>
    public override string ValidateEdit(EditType type, string diff, string result)
    {
        if (type == EditType.DELETE)
        {
            return result;
        }
        if (type == EditType.ADD)
        {
            string toAdd = CharacterMatcher.TrimToMatches(diff);
            // FIXME: range errors when replacing text. maybe need EditType.Replace
            result = result[..(Internal.IndexLeft - diff.Length)] + toAdd + result[Internal.IndexRight..];
            Internal.SetPosition(Internal.IndexLeft - diff.Length + toAdd.Length);
            return result;
        }
        if (result.Length == 0 && !PlaceholderInfo.Empty)
        {
            return "";
        }
        int expIndex = result.LastIndexOf('e');
        if (expIndex != -1 && (expIndex == result.Length - 1 || !result[..expIndex].Any(char.IsAsciiDigit)))
        {
            expIndex = -1;
        }
        int signIndex = result.IndexOf('-');
        if (signIndex != -1 && expIndex == -1 && result[..signIndex].Any(char.IsAsciiDigit))
        {
            signIndex = -1;
        }
        int decimalIndex = (signIndex != -1 && (expIndex == -1 || signIndex < expIndex))
            ? result[signIndex..].IndexOf('.') + signIndex
            : result.IndexOf('.');
        if (decimalIndex != -1 && expIndex != -1 && decimalIndex > expIndex)
        {
            decimalIndex = -1;
        }
        IEnumerable<char> filtered = result.Where((c, index) => c switch
        {
            'e' => index == expIndex,
            '.' => index == decimalIndex,
            '-' => index == signIndex || (expIndex != -1 && index == expIndex + 1),
            _ => true
        });
        result = new string([.. filtered]);
        if (double.TryParse(result, out double value))
        {
            NumberLabelInternal.Value = value;
            return value.ToString(Format);
        }
        NumberLabelInternal.Value = 0;
        return "0";
    }

    /// <inheritdoc/>
    public override List<string> GetDebugInfo()
    {
        List<string> info = base.GetDebugInfo();
        info.Add($"^7Value: ^3{Value}");
        return info;
    }
}
