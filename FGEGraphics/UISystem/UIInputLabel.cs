using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.UISystem.InputSystems;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

public class UIInputLabel : UIClickableElement
{
    public UIElementText Text;

    public struct InternalData()
    {
        public int CursorLeft = 0;
        public int CursorRight = 0;
        public readonly int IndexLeft => CursorLeft < CursorRight ? CursorLeft : CursorRight;
        public readonly int IndexRight => CursorRight > CursorLeft ? CursorRight : CursorLeft;
    }

    public bool Selected = false; // TODO: Provide a UIElement-native solution for this
    public InternalData Internal = new();

    public Action<string> TextModified;
    public Action<string> TextSubmitted;
    public Action Closed;

    public UIInputLabel(StyleGroup styles, UIPositionHelper pos) : base(styles, pos)
    {
        Text = new(this, "1234", true);
    }

    /// <inheritdoc/>
    public override void MouseLeftDown()
    {
        Selected = true;
        Enabled = false;
        Pressed = true;
        Position.View.InteractingElement = null;
    }

    /// <inheritdoc/>
    public override void MouseLeftDownOutside()
    {
        Selected = false;
        Enabled = true;
        Pressed = false;
    }

    public void TickBackspaces(KeyHandlerState keys)
    {
        if (keys.InitBS == 0 || Text.Content.Length == 0 || Internal.IndexRight == 0)
        {
            return;
        }
        if (Internal.CursorLeft == Internal.CursorRight)
        {
            Text.Content = Text.Content[..(Internal.IndexLeft - 1)] + Text.Content[Internal.IndexLeft..];
            Internal.CursorRight = --Internal.CursorLeft;
        }
        else
        {
            Text.Content = Text.Content[..Internal.IndexLeft] + Text.Content[Internal.IndexRight..];
            Internal.CursorLeft = Internal.CursorRight = Internal.IndexLeft;
        }
    }

    public void TickContent(KeyHandlerState keys)
    {
        if (keys.KeyboardString.Length > 0)
        {
            Text.Content = Text.Content[..Internal.IndexLeft] + keys.KeyboardString + Text.Content[Internal.IndexRight..];
            Internal.CursorRight = Internal.CursorLeft += keys.KeyboardString.Length;
        }
    }

    public void TickArrowKeys(KeyHandlerState keys, bool shiftDown)
    {
        if (keys.LeftRights < 0)
        {
            Internal.CursorRight = Math.Max(Internal.CursorRight + keys.LeftRights, 0);
        }
        else if (keys.LeftRights > 0)
        {
            Internal.CursorRight = Math.Min(Internal.CursorRight + keys.LeftRights, Text.Content.Length);
        }
        if (keys.LeftRights != 0 && !shiftDown)
        {
            Internal.CursorLeft = Internal.CursorRight;
        }
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (!Selected)
        {
            return;
        }
        KeyHandlerState keys = Window.Keyboard.BuildingState;
        if (keys.Escaped)
        {
            // submitted
            // closed
        }
        bool shiftDown = Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        TickBackspaces(keys);
        TickContent(keys);
        TickArrowKeys(keys, shiftDown);
    }

    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        if (style.CanRenderText(Text))
        {
            style.TextFont.DrawFancyText(Text, Text.GetPosition(X, Y));
        }
        /*Renderer2D.SetColor(Color4F.Red);
        view.Rendering.RenderRectangle(view.UIContext, X + Internal.CursorLeft * 10, Y, X + Internal.CursorLeft * 10 + 2, Y + 2);
        Renderer2D.SetColor(Color4F.Blue);
        view.Rendering.RenderRectangle(view.UIContext, X + Internal.CursorRight * 10, Y, X + Internal.CursorRight * 10 + 2, Y + 2);*/
        style.TextFont.DrawFancyText($"{Internal.CursorLeft} {Internal.CursorRight}", new(X, Y + 30, 0));
    }
}
