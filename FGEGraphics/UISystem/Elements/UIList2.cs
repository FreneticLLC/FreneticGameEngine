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

namespace FGEGraphics.UISystem.Elements;

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

    public bool FixedSize = false;

    // TODO: temporary hack!
    public int PaddingLeft;

    public UIAlignment Alignment => Vertical ? Anchor.AlignmentX : Anchor.AlignmentY;

    public bool IsReversed => Vertical && Anchor.AlignmentY == UIAlignment.BOTTOM || !Vertical && Anchor.AlignmentX == UIAlignment.RIGHT;

    /// <summary>Constructs a new list group.</summary>
    /// <param name="layout">The layout of the element.</param>
    public UIList2(UIStyling styling = null, UILayout layout = null) : base(styling, layout)
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

    // TODO: use Style.Inset instead of padding
    public override void UpdateSize(double delta, OpenTK.Mathematics.Vector3 rotation)
    {
        // this doesn't make any sense
        foreach (UIElement child in Children)
        {
            child.UpdateTransforms(delta, rotation, TransformFlags.SCALE | TransformFlags.SIZE);
        }
        // TODO: generic default transform flags on UIElement
        if (FixedSize)
        {
            base.UpdateSize(delta, rotation);
            Layout.SetSize(Size);
            return;
        }
        ElementInternal.LastSize = Size;
        if (Children.Count == 0)
        {
            Size = new();
            Layout.SetSize(Size);
            return;
        }
        int length = Children.Sum(child => Vertical ? child.Height : child.Width) + Spacing * (Children.Count - 1) + Style.Padding * 2;
        int maxDepth = Children.Max(child => Vertical ? child.Width : child.Height) + Style.Padding + PaddingLeft;
        Size = Vertical ? new(maxDepth, length) : new(length, maxDepth);
        Layout.SetSize(Size);
    }

    public override void UpdateChildTransforms(double delta, OpenTK.Mathematics.Vector3 rotation)
    {
        int offset = 0;
        for (int i = 0; i < Children.Count; i++)
        {
            UIElement child = IsReversed ? Children[^(i + 1)] : Children[i];
            int inset = PaddingLeft + Alignment.GetPosition((Vertical ? Width : Height) - (PaddingLeft + Style.Padding), Vertical ? child.Width : child.Height);
            Vector2i pos = Vertical ? new(X + inset, Y + Style.Padding + offset) : new(X + Style.Padding + offset, Y + inset);
            child.ElementInternal.LastPosition = child.Position = pos;
            child.UpdateTransforms(delta, rotation, TransformFlags.CHILDREN);
            offset += (Vertical ? child.Height : child.Width) + Spacing;
        }
    }
}
