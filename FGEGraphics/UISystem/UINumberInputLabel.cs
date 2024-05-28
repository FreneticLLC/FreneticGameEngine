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

namespace FGEGraphics.UISystem;

/// <summary>Represents an editable number label.</summary>
public class UINumberInputLabel : UIInputLabel
{
    /// <summary>Whether the label should be an integer instead of a decimal.</summary>
    public bool Integer;

    /// <summary>The format string to apply to the label on submission.</summary>
    public string Format;

    /// <summary>The decimal value of the label.</summary>
    public double Value => double.Parse(TextContent);

    /// <summary>Constructs a number input label.</summary>
    /// <param name="initial">The initial number value.</param>
    /// <param name="integer">Whether the label should be an integer.</param>
    /// <param name="format">The format string for the label.</param>
    /// <param name="inputStyle">The style of normal input content.</param>
    /// <param name="highlightStyle">The style of highlighted input content.</param>
    /// <param name="pos">The position of the element.</param>
    public UINumberInputLabel(double initial, bool integer, string format, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos) : base(string.Empty, initial.ToString(), StyleGroup.Empty, inputStyle, highlightStyle, pos)
    {
        Integer = integer;
        Format = format;
        TextContent = initial.ToString(Format);
    }

    /// <summary>Returns whether a character can be added to this label.</summary>
    /// <param name="c">The character to add.</param>
    public bool IsValidCharacter(char c) => c == '-' || char.IsAsciiDigit(c) || (!Integer && c == '.' || c == 'e');

    /// <inheritdoc/>
    public override string ValidateEdit(EditType type, string diff, string result)
    {
        if (type == EditType.Delete)
        {
            return result;
        }
        if (type == EditType.Add)
        {
            string toAdd = new(diff.ToLower().Where(IsValidCharacter).ToArray());
            result = result[..(Internal.IndexLeft - diff.Length)] + toAdd + result[Internal.IndexLeft..];
            Internal.SetPosition(Internal.IndexLeft - diff.Length + toAdd.Length);
            return result;
        }
        int expIndex = result.LastIndexOf('e');
        if (expIndex != -1 && (expIndex == result.Length - 1 || result[..expIndex].Where(char.IsAsciiDigit).IsEmpty()))
        {
            expIndex = -1;
        }
        int signIndex = result.IndexOf('-');
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
