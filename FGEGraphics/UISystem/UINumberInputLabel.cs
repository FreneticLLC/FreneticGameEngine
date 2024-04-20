using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.CoreSystems;

namespace FGEGraphics.UISystem;

public class UINumberInputLabel : UIInputLabel
{


    public UINumberInputLabel(string info, string defaultText, StyleGroup infoStyles, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos) : base(info, defaultText, infoStyles, inputStyle, highlightStyle, pos)
    {
    }

    public override string ValidateEdit(EditType type, string diff, string result)
    {
        if (type != EditType.Add)
        {
            return result;
        }
        bool negative = false;
        string cleanDiff = string.Empty;
        foreach (char c in diff)
        {
            if (c == '-')
            {
                negative = true;
                continue;
            }
            if (c >= 48 && c <= 59)
            {
                cleanDiff += c;
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
