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

    /// <inheritdoc/>
    public override string ValidateEdit(EditType type, string diff, string result)
    {
        if (type == EditType.Delete)
        {
            return result;
        }
        if (type == EditType.Submit)
        {
            double value = result.Length > 0 && result != "-" && result != "." ? double.Parse(result) : 0;
            return value.ToString(Format);
        }
        bool hasDecimal = TextContent.Contains('.');
        bool negative = false;
        string cleanDiff = string.Empty;
        foreach (char c in diff)
        {
            if (c == '-')
            {
                negative = true;
                continue;
            }
            if (c >= '0' && c <= '9')
            {
                cleanDiff += c;
            }
            if (c == '.' && !Integer && !hasDecimal)
            {
                cleanDiff += c;
                hasDecimal = true;
            }
        }
        result = result[..(Internal.IndexLeft - diff.Length)] + cleanDiff + result[Internal.IndexLeft..];
        int indexLeft = Internal.IndexLeft;
        if (negative && (result.Length == 0 || result[0] != '-'))
        {
            result = '-' + result;
            indexLeft++;
        }
        Internal.SetPosition(indexLeft - diff.Length + cleanDiff.Length);
        return result;
    }
}
