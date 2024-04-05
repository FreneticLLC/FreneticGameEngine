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
    public UIElementText Info;

    public UIElementStyle InputStyle;
    public UIElementStyle HighlightStyle;

    public bool Selected = false; // TODO: Provide a UIElement-native solution for this
    public InternalData Internal = new();

    public Action<string> TextEdited;
    public Action<string> TextSubmitted;
    public Action Closed;

    public string TextContent
    {
        get => Internal.TextContent;
        set
        {
            if (Internal.TextContent == value)
            {
                return;
            }
            Internal.TextContent = value ?? string.Empty;
            Internal.CursorLeft = Math.Clamp(Internal.IndexLeft, 0, Internal.TextContent.Length);
            Internal.CursorRight = Math.Clamp(Internal.IndexRight, 0, Internal.TextContent.Length);
            UpdateInternalText();
        }
    }

    public struct InternalData()
    {
        public string TextContent = string.Empty;
        public int CursorLeft = 0;
        public int CursorRight = 0;
        public readonly int IndexLeft => CursorLeft < CursorRight ? CursorLeft : CursorRight;
        public readonly int IndexRight => CursorRight > CursorLeft ? CursorRight : CursorLeft;
        public UIElementText TextLeft;
        public UIElementText TextBetween;
        public UIElementText TextRight;

        public IEnumerable<UIElementText> TextPieces()
        {
            yield return TextLeft;
            yield return TextBetween;
            yield return TextRight;
        }
    }

    public UIInputLabel(string info, string defaultText, StyleGroup infoStyles, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos) : base(infoStyles, pos, requireText: true)
    {
        InputStyle = inputStyle ?? infoStyles.Normal;
        HighlightStyle = highlightStyle ?? infoStyles.Click;
        Info = new(this, info, true);
        Internal.TextLeft = new(this, null, false, style: InputStyle);
        Internal.TextBetween = new(this, null, false, style: HighlightStyle);
        Internal.TextRight = new(this, null, false, style: InputStyle);
        TextContent = defaultText;
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
        Internal.TextLeft.Content = TextContent[..Internal.IndexLeft];
        Internal.TextBetween.Content = TextContent[Internal.IndexLeft..Internal.IndexRight];
        Internal.TextRight.Content = TextContent[Internal.IndexRight..];
    }

    public void ModifyText(string text, bool edit = true)
    {
        TextContent = edit ? ValidateEdit(text) : ValidateSubmission(text);
        (edit ? TextEdited : TextSubmitted)?.Invoke(TextContent);
    }

    // TODO: Cap length
    public virtual string ValidateEdit(string text) => text;

    public virtual string ValidateSubmission(string text) => text;

    public void TickBackspaces(KeyHandlerState keys)
    {
        if (keys.InitBS == 0 || TextContent.Length == 0 || Internal.IndexRight == 0)
        {
            return;
        }
        if (Internal.CursorLeft != Internal.CursorRight)
        {
            ModifyText(TextContent[..Internal.IndexLeft] + TextContent[Internal.IndexRight..]);
            Internal.CursorLeft = Internal.CursorRight = Internal.IndexLeft;
            keys.InitBS--;
        }
        if (keys.InitBS > 0)
        {
            int index = Math.Max(Internal.IndexLeft - keys.InitBS, 0);
            ModifyText(TextContent[..index] + TextContent[Internal.IndexRight..]);
            Internal.CursorLeft = Internal.CursorRight = index;
        }
        UpdateInternalText();
    }

    public void TickContent(KeyHandlerState keys)
    {
        if (keys.KeyboardString.Length == 0)
        {
            return;
        }
        ModifyText(TextContent[..Internal.IndexLeft] + keys.KeyboardString + TextContent[Internal.IndexRight..]);
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
            Internal.CursorRight = Math.Min(Internal.CursorRight + keys.LeftRights, TextContent.Length);
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
            ModifyText(TextContent, false);
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
        if (TextContent.Length == 0)
        {
            style.TextFont.DrawFancyText(Info, Info.GetPosition(X, Y));
            return;
        }
        float textX = X;
        foreach (UIElementText text in Internal.TextPieces())
        {
            if (text.CurrentStyle.CanRenderText(text))
            {
                text.CurrentStyle.TextFont.DrawFancyText(text, text.GetPosition(textX, Y));
            }
            textX += text.Width;
        }
        if (!Selected)
        {
            return;
        }
        Engine.Textures.White.Bind();
        Renderer2D.SetColor(Color4F.Red);
        view.Rendering.RenderRectangle(view.UIContext, X + Internal.TextLeft.Width - 2, Y, X + Internal.TextLeft.Width + 2, Y + Internal.TextLeft.Height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
        Renderer2D.SetColor(Color4.White);
        style.TextFont.DrawFancyText($"{Internal.IndexLeft} {Internal.IndexRight}", new(X, Y + 30, 0));
    }
}
