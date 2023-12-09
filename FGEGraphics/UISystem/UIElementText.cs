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
using FGEGraphics.GraphicsHelpers.FontSets;
using FreneticUtilities.FreneticExtensions;

namespace FGEGraphics.UISystem;

public class UIElementText
{
    public struct InternalData
    {
        public UIElement ParentElement;
        public string RawContent;
        public Dictionary<UIElementStyle, RenderableText> RenderableContent;
    }

    public InternalData Internal;

    public UIElementText(UIElement parent, string content)
    {
        Internal = new InternalData()
        {
            ParentElement = parent,
            RawContent = content,
            RenderableContent = new Dictionary<UIElementStyle, RenderableText>()
        };
        RefreshRenderables();
    }

    private void RefreshRenderables()
    {
        foreach (UIElementStyle style in Internal.ParentElement.ElementInternal.Styles)
        {
            if (style.TextFont is not null)
            {
                Internal.RenderableContent[style] = style.TextFont.ParseFancyText(Internal.RawContent, style.TextStyling);
            }
        }
    }

    public string Content
    {
        get => Internal.RawContent;
        set
        {
            Internal.RawContent = value;
            RefreshRenderables();
        }
    }

    public RenderableText Renderable => Internal.RenderableContent[Internal.ParentElement.GetStyle()];

    public static implicit operator RenderableText(UIElementText text)
    {
        return text.Renderable;
    }
}
