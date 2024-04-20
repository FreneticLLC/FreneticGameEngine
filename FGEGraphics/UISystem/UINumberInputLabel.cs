using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.CoreSystems;

namespace FGEGraphics.UISystem;

public class UINumberInputLabel : UIInputLabel
{
    public bool Integer;
    public string Format;

    public double Value => double.Parse(TextContent);

    public UINumberInputLabel(double initial, bool integer, string format, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos) : base(string.Empty, initial.ToString(), StyleGroup.Empty, inputStyle, highlightStyle, pos)
    {
        Integer = integer;
        Format = format;
        TextContent = initial.ToString(Format);
    }

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
