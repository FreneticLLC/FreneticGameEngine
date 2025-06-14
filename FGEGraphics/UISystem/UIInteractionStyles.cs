//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGEGraphics.GraphicsHelpers.Textures;

namespace FGEGraphics.UISystem;

/// <summary>Represents styles that display an element's interaction state.</summary>
/// <param name="normal">The default style to use.</param>
/// <param name="hover">The style to use on hover.</param>
/// <param name="press">The style to use on press.</param>
/// <param name="disabled">The style to use when disabled.</param>
public class UIInteractionStyles(UIStyle normal, UIStyle hover, UIStyle press, UIStyle disabled)
{
    /// <summary>Empty interaction styles.</summary>
    public static readonly UIInteractionStyles Empty = new(UIStyle.Empty, UIStyle.Empty, UIStyle.Empty, UIStyle.Empty);

    /// <summary>The style of an element not being interacted with.</summary>
    public UIStyle Normal = normal;

    /// <summary>The style of an element about to be interacted with.</summary>
    public UIStyle Hover = hover;

    /// <summary>The style of an element being interacted with.</summary>
    public UIStyle Press = press;

    /// <summary>The style of a disabled element.</summary>
    public UIStyle Disabled = disabled;

    /// <summary>The styling logic for a <see cref="UIElement.Styler"/>.</summary>
    public UIStyle Styler(UIElement element) => element.IsPressed ? Press
        : element.IsHovered ? Hover
        : !element.IsEnabled ? Disabled
        : Normal;

    /// <summary>Creates interaction styles based on a standard texture set.</summary>
    /// <param name="baseStyle">The base interaction style.</param>
    /// <param name="textures">The engine to get textures from.</param>
    /// <param name="textureSet">The name of the texture set.</param>
    public static UIInteractionStyles Textured(UIStyle baseStyle, TextureEngine textures, string textureSet)
    {
        UIStyle normal = new(baseStyle) { BaseTexture = textures.GetTexture($"{textureSet}_none") };
        UIStyle hover = new(baseStyle) { BaseTexture = textures.GetTexture($"{textureSet}_hover") };
        UIStyle press = new(baseStyle) { BaseTexture = textures.GetTexture($"{textureSet}_press") };
        UIStyle disabled = new(baseStyle) { BaseTexture = textures.GetTexture($"{textureSet}_disabled") };
        return new(normal, hover, press, disabled);
    }
}
