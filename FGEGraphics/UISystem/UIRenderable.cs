using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGEGraphics.ClientSystem;

namespace FGEGraphics.UISystem;

public class UIRenderable(ViewUI2D view, Action<UIElement, ViewUI2D, double> renderer) : UIElement(new UIPositionHelper(view).Anchor(UIAnchor.TOP_LEFT))
{
    public override void Render(ViewUI2D view, double delta, UIElementStyle style) => renderer(this, view, delta);
}
