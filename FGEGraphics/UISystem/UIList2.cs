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
    [UIDebug]
    public int Spacing;

    /// <summary>The anchor that the list will expand from.</summary>
    [UIDebug]
    public UIAnchor Anchor;

    /// <summary>Data internal to a <see cref="UIList"/> instance.</summary>
    /*public struct InternalData()
    {
        /// <summary>Maps items to their positional offsets relative to this list's origin.</summary>
        public Dictionary<UIElement, int> Offsets = [];

        /// <summary>Maps items to their positional updating logic.</summary>
        public Dictionary<UIElement, Action<Vector2i, Vector2i>> Updaters = [];
    }

    /// <summary>Data internal to a <see cref="UIList"/> instance.</summary>
    public InternalData Internal = new();*/

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
        int offset = 0;
        int maxExtension = 50;
        for (int i = 0; i < ElementInternal.Children.Count; i++)
        {
            UIElement child = ElementInternal.Children[i];
            child.UpdateTransforms(delta, rotation);
            int pos = (Vertical ? Y : X) + offset;
            if (Vertical)
            {
                // TODO: this is a bandage
                child.ElementInternal.LastPosition = child.Position = new(X, pos);
            }
            else
            {
                child.ElementInternal.LastPosition = child.Position = new(pos, Y);
            }
            offset += Vertical ? child.Height : child.Width;
            if (i < ElementInternal.Children.Count - 1)
            {
                offset += Spacing;
            }
            int extension = Vertical ? child.Width : child.Height;
            if (extension > maxExtension)
            {
                maxExtension = extension;
            }
        }
        Size = Vertical ? new(maxExtension, offset) : new(offset, maxExtension);
    }
}
