using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGEGraphics.ClientSystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple renderer that can be attached to any element.</summary>
/// <param name="view">The UI view.</param>
/// <param name="renderer">The renderer method. See <see cref="UIElement.Render(ViewUI2D, double)"/>.</param>
public class UIRenderable(ViewUI2D view, Action<UIElement, ViewUI2D, double> renderer) : UIElement(new UIPositionHelper(view).Anchor(UIAnchor.TOP_LEFT))
{
    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style) => renderer(this, view, delta);
}
