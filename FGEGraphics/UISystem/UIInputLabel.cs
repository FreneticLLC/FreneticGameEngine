using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
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
            Internal.CursorLeft = Math.Clamp(Internal.CursorLeft, 0, Internal.TextContent.Length);
            Internal.CursorRight = Math.Clamp(Internal.CursorRight, 0, Internal.TextContent.Length);
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
        public readonly bool IsSelection => CursorLeft != CursorRight;
        public UIElementText TextLeft;
        public UIElementText TextBetween;
        public UIElementText TextRight;
        public List<UIElementText> TextChain;
    }

    public UIInputLabel(string info, string defaultText, StyleGroup infoStyles, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos) : base(infoStyles, pos, requireText: true)
    {
        InputStyle = inputStyle ?? infoStyles.Normal;
        HighlightStyle = highlightStyle ?? infoStyles.Click;
        Info = new(this, info, true);
        Internal.TextLeft = new(this, null, false, style: InputStyle);
        Internal.TextBetween = new(this, null, false, style: HighlightStyle);
        Internal.TextRight = new(this, null, false, style: InputStyle);
        Internal.TextChain = [Internal.TextLeft, Internal.TextBetween, Internal.TextRight];
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
        if (Internal.IsSelection)
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

    // TODO: Handle multiline properly
        // have option for it at all and disallow \n in validate
        // with: need fix in RenderChain (render lines separately? needs DrawFancyText update)
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

    // TODO: Handle ctrl left/right, handle up/down arrows
    public void TickArrowKeys(KeyHandlerState keys, bool shiftDown)
    {
        if (keys.LeftRights == 0)
        {
            return;
        }
        bool wasSelection = Internal.IsSelection && !shiftDown;
        Internal.CursorRight = keys.LeftRights < 0
            ? (wasSelection ? Internal.IndexLeft : Math.Max(Internal.CursorRight + keys.LeftRights, 0))
            : (wasSelection ? Internal.IndexRight : Math.Min(Internal.CursorRight + keys.LeftRights, TextContent.Length));
        if (!shiftDown)
        {
            Internal.CursorLeft = Internal.CursorRight;
        }
        UpdateInternalText();
    }

    public void TickMouse()
    {
        if (!MouseDown || MousePreviouslyDown)
        {
            return;
        }
        int indexOffset = 0;
        float relMouseX = Window.MouseX - X;
        float relMouseY = Window.MouseY - Y;
        foreach (UIElementText.ChainPiece piece in UIElementText.IterateChain(Internal.TextChain))
        {
            string content = piece.Line.ToString();
            if (piece.YOffset + piece.Text.CurrentStyle.TextFont.FontDefault.Height >= relMouseY)
            {
                for (int i = 0; i < content.Length; i++)
                {
                    float width = piece.Text.CurrentStyle.TextFont.MeasureFancyText(content[..i]);
                    if (piece.XOffset + width >= relMouseX)
                    {
                        Internal.CursorLeft = Internal.CursorRight = indexOffset + i - 1;
                        UpdateInternalText();
                        return;
                    }
                }
            }
            indexOffset += content.Length;
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
            ModifyText(TextContent, false);
            Closed?.Invoke();
        }
        bool shiftDown = Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        TickBackspaces(keys);
        TickContent(keys);
        TickArrowKeys(keys, shiftDown);
        TickMouse();
        // TODO: handle ctrl+A
        // TODO: handle ctrl+Z, ctrl+Y
        // TODO: handle mouse clicking
    }

    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        bool info = TextContent.Length == 0;
        if (info)
        {
            style.TextFont.DrawFancyText(Info, Info.GetPosition(X, Y));
        }
        else
        {
            UIElementText.RenderChain(Internal.TextChain, X, Y);
        }
        if (!Selected || Internal.IsSelection)
        {
            return;
        }
        // TODO: Cursor blink modes
        Engine.Textures.White.Bind();
        Renderer2D.SetColor(style.BorderColor);
        RenderableTextLine[] lines = Internal.TextLeft.Renderable.Lines;
        int lineWidth = style.BorderThickness / 2;
        int lineX = X + (lines?.Last().Width ?? 0);
        int lineCount = lines?.Length ?? 1;
        int lineHeight = (info ? Info : Internal.TextLeft).CurrentStyle.TextFont.FontDefault.Height;
        view.Rendering.RenderRectangle(view.UIContext, lineX - lineWidth, Y + (lineCount - 1) * lineHeight, lineX + lineWidth, Y + lineCount * lineHeight, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
        Renderer2D.SetColor(Color4.White);
    }
}
