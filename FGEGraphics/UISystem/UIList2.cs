//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.StackNoteSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BepuPhysics.Collidables.CompoundBuilder;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents an expandable list container of UI elements.
/// List expansion is in the opposite direction of the supplied <see cref="UIAnchor"/>. 
/// </summary>
public class UIList2 : UIElement, IStylingAcceptor<UIList2.ListStyling>
{
    public record ListStyling : StylingComponent
    {
        public UIStyleValue<bool> Vertical = true;

        public UIStyleValue<int> Spacing;

        public UIStyleValue<UIAnchor> Anchor;
    }

    /// <inheritdoc/>
    public override string Name => "List";

    /// <summary>Whether the list should expand vertically.</summary>
    [UIDebug]
    public bool Vertical;

    /// <summary>The spacing between each list item.</summary>
    [UIDebug, SettingNumeric(SettingNumericDisplayType.Slider, 0, 100)]
    public int Spacing;

    /// <summary>The anchor that the list will expand from.</summary>
    [UIDebug]
    public UIAnchor Anchor;

    public bool IsReversed => Vertical && Anchor.AlignmentY == UIAlignment.BOTTOM || !Vertical && Anchor.AlignmentX == UIAlignment.RIGHT;

    /// <summary>Constructs a new list group.</summary>
    /// <param name="layout">The layout of the element.</param>
    public UIList2(UIStyling styling, UILayout layout) : base(styling, layout)
    {
        // ScaleSize = false?
        Vertical = true;
        Anchor = Layout.Anchor;
    }

    public override void AddChildInternal(UIElement element)
    {
        base.AddChildInternal(element);
        element.Layout.SetAnchor(Anchor).SetPosition(0, 0);
        element.TransformSelf = false;
    }

    public void AcceptStyling(ListStyling styling)
    {
        Vertical = styling.Vertical.Get(this);
        Spacing = styling.Spacing.Get(this);
        Anchor = styling.Anchor.Get(this) ?? Layout.Anchor;
    }

    public override void UpdateTransforms(double delta, OpenTK.Mathematics.Vector3 rotation)
    {
        UpdateScale();
        UpdatePosition(rotation);
        ElementInternal.LastSize = Size;
        int maxDepth = 0;
        foreach (UIElement child in ElementInternal.Children)
        {
            child.UpdateTransforms(delta, rotation);
            int childDepth = Vertical ? child.Width : child.Height;
            if (childDepth > maxDepth)
            {
                maxDepth = childDepth;
            }
        }
        int length = 0;
        for (int i = 0; i < ElementInternal.Children.Count; i++)
        {
            UIElement child = IsReversed ? ElementInternal.Children[^(i + 1)] : ElementInternal.Children[i];
            child.UpdateTransforms(delta, rotation);
            int inset = Style.Padding + (Vertical ? Anchor.AlignmentX : Anchor.AlignmentY).GetPosition(maxDepth, Vertical ? child.Width : child.Height);
            Vector2i pos = Vertical ? new(X + inset, Y + Style.Padding + length) : new(X + Style.Padding + length, Y + inset);
            child.ElementInternal.LastPosition = child.Position = pos;
            length += Vertical ? child.Height : child.Width;
            if (i < ElementInternal.Children.Count - 1)
            {
                length += Spacing;
            }
        }
        maxDepth += Style.Padding * 2;
        length += Style.Padding * 2;
        Size = Vertical ? new(maxDepth, length) : new(length, maxDepth);
    }
}
