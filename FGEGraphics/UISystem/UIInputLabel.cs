using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.UISystem.InputSystems;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

public class UIInputLabel : UIClickableElement
{
    public UIElementText Text;
    public UIElementText Info;

    public struct InternalData()
    {
        public int CursorLeft = 0;
        public int CursorRight = 0;
        public readonly int IndexLeft => CursorLeft < CursorRight ? CursorLeft : CursorRight;
        public readonly int IndexRight => CursorRight > CursorLeft ? CursorRight : CursorLeft;
        public UIElementText TextLeft;
        public UIElementText TextRight;
    }

    public bool Selected = false; // TODO: Provide a UIElement-native solution for this
    public InternalData Internal = new();

    public Action<string> TextEdited;
    public Action<string> TextSubmitted;
    public Action Closed;

    public UIInputLabel(string text, string info, StyleGroup styles, UIPositionHelper pos) : base(styles, pos, requireText: true)
    {
        Text = new(this, text, false);
        Info = new(this, info, true);
        Internal.TextLeft = new(this, null, false);
        Internal.TextRight = new(this, null, false);
        Closed += HandleClose;
    }

    /// <inheritdoc/>
    public override void MouseLeftDown()
    {
        Selected = true;
        Enabled = false;
        Pressed = true;
        Position.View.InteractingElement = null;
    }

    public void HandleClose()
    {
        Selected = false;
        Enabled = true;
        Pressed = false;
    }

    /// <inheritdoc/>
    public override void MouseLeftDownOutside() => HandleClose();

    public void UpdateInternalText()
    {
        Internal.TextLeft.Content = Text.Content[..Internal.IndexLeft];
        Internal.TextRight.Content = Internal.IndexLeft != Internal.IndexRight ? Text.Content[..Internal.IndexRight] : null;
    }

    public void ModifyText(string text, bool edit = true)
    {
        string result = edit ? ValidateEdit(text) : ValidateSubmission(text);
        Text.Content = result;
        (edit ? TextEdited : TextSubmitted)?.Invoke(result);
    }

    // TODO: Cap length
    public virtual string ValidateEdit(string text) => text;

    public virtual string ValidateSubmission(string text) => text;

    public void TickBackspaces(KeyHandlerState keys)
    {
        if (keys.InitBS == 0 || Text.Content.Length == 0 || Internal.IndexRight == 0)
        {
            return;
        }
        if (Internal.CursorLeft != Internal.CursorRight)
        {
            ModifyText(Text.Content[..Internal.IndexLeft] + Text.Content[Internal.IndexRight..]);
            Internal.CursorLeft = Internal.CursorRight = Internal.IndexLeft;
            keys.InitBS--;
        }
        if (keys.InitBS > 0)
        {
            ModifyText(Text.Content[..Math.Max(Internal.IndexLeft - keys.InitBS, 0)] + Text.Content[Internal.IndexRight..]);
            Internal.CursorRight = --Internal.CursorLeft;
        }
        UpdateInternalText();
    }

    public void TickContent(KeyHandlerState keys)
    {
        if (keys.KeyboardString.Length == 0)
        {
            return;
        }
        ModifyText(Text.Content[..Internal.IndexLeft] + keys.KeyboardString + Text.Content[Internal.IndexRight..]);
        Internal.CursorRight = Internal.CursorLeft += keys.KeyboardString.Length;
        UpdateInternalText();
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
        if (keys.LeftRights == 0)
        {
            return;
        }
        if (!shiftDown)
        {
            Internal.CursorLeft = Internal.CursorRight;
        }
        UpdateInternalText();
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
            ModifyText(Text.Content, false);
            Closed?.Invoke();
        }
        bool shiftDown = Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        TickBackspaces(keys);
        TickContent(keys);
        TickArrowKeys(keys, shiftDown);
    }

    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        UIElementText renderText = Text.Empty ? Info : Text;
        if (style.CanRenderText(renderText))
        {
            style.TextFont.DrawFancyText(renderText, renderText.GetPosition(X, Y));
        }
        if (!Selected)
        {
            return;
        }
        Engine.Textures.White.Bind();
        Renderer2D.SetColor(Color4F.Red);
        view.Rendering.RenderRectangle(view.UIContext, X + Internal.TextLeft.Width - 2, Y, X + Internal.TextLeft.Width + 2, Y + Text.Height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
        Renderer2D.SetColor(Color4.White);
        style.TextFont.DrawFancyText($"{Internal.IndexLeft} {Internal.IndexRight}", new(X, Y + 30, 0));
    }
}
