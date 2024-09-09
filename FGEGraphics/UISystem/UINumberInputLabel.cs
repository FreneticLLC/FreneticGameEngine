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
using FGECore.CoreSystems;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FGEGraphics.UISystem;

/// <summary>Represents an editable number label.</summary>
// TODO: Cache raw value internally & independent of format
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

    /// <summary>The decimal value of the label.</summary>
    public double Value => double.Parse(TextContent);

    /// <remarks>Constructs a number input label.</remarks>
    /// <param name="initial">The initial number value.</param>
    /// <param name="integer">Whether the label should be an integer.</param>
    /// <param name="format">The format string for the label.</param>
    /// <param name="inputStyle">The style of normal input content.</param>
    /// <param name="highlightStyle">The style of highlighted input content.</param>
    /// <param name="pos">The position of the element.</param>-
    /// <param name="renderBox">Whether to render a box behind the label.</param>
    /// <param name="boxPadding">The padding between the box and the label.</param>
    /// <param name="boxStyles">The box styles for the label.</param>
    public UINumberInputLabel(double initial, bool integer, string format, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos, bool renderBox = false, int boxPadding = 0, UIClickableElement.StyleGroup boxStyles = null) : base("", initial.ToString(format), boxStyles ?? StyleGroup.Empty, inputStyle, highlightStyle, pos, false, renderBox, boxPadding)
    {
        Integer = integer;
        Format = format;
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
        result = new string(filtered.ToArray());
        return double.TryParse(result, out double value) ? value.ToString(Format) : "0";
    }
}
