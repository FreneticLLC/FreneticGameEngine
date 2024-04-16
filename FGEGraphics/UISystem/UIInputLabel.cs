//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

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
    public int Lines = 1; // TODO: Implement
    public int MaxLength; // TODO: Implement

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
            Internal.UpdateText();
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
        
        public void SetPosition(int cursorPos) => CursorLeft = CursorRight = cursorPos;

        public void UpdateText()
        {
            TextLeft.Content = TextContent[..IndexLeft];
            TextBetween.Content = TextContent[IndexLeft..IndexRight];
            TextRight.Content = TextContent[IndexRight..];
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
            Internal.SetPosition(Internal.IndexLeft);
            keys.InitBS--;
        }
        if (keys.InitBS > 0)
        {
            int index = Math.Max(Internal.IndexLeft - keys.InitBS, 0);
            ModifyText(TextContent[..index] + TextContent[Internal.IndexRight..]);
            Internal.SetPosition(index);
        }
        Internal.UpdateText();
    }

    public void TickContent(KeyHandlerState keys)
    {
        if (keys.KeyboardString.Length == 0)
        {
            return;
        }
        ModifyText(TextContent[..Internal.IndexLeft] + keys.KeyboardString + TextContent[Internal.IndexRight..]);
        Internal.CursorRight = Internal.CursorLeft += keys.KeyboardString.Length;
        Internal.UpdateText();
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
        Internal.UpdateText();
    }

    public void TickMouse()
    {
        if (!MouseDown || MousePreviouslyDown)
        {
            return;
        }
        List<UIElementText.ChainPiece> pieces = UIElementText.IterateChain(Internal.TextChain).ToList();
        float relMouseX = Window.MouseX - X;
        float relMouseY = Window.MouseY - Y;
        if (pieces[^1].YOffset + pieces[^1].Text.CurrentStyle.FontHeight < relMouseY)
        {
            Internal.SetPosition(TextContent.Length);
            Internal.UpdateText();
            return;
        }
        int indexOffset = 0;
        for (int i = 0; i < pieces.Count; i++)
        {
            UIElementText.ChainPiece piece = pieces[i];
            if (i != 0 && piece.XOffset == 0)
            {
                indexOffset++;
            }    
            string content = piece.Line.ToString();
            if (piece.YOffset + piece.Text.CurrentStyle.FontHeight >= relMouseY)
            {
                if (piece.XOffset + piece.Line.Width < relMouseX && (i == pieces.Count - 1 || pieces[i + 1].XOffset == 0))
                {
                    Internal.SetPosition(indexOffset + content.Length);
                    Internal.UpdateText();
                    return;
                }
                float lastWidth = 0;
                for (int j = 0; j <= content.Length; j++)
                {
                    float width = piece.Text.CurrentStyle.TextFont.MeasureFancyText(content[..j]);
                    if (piece.XOffset + width >= relMouseX)
                    {
                        int diff = relMouseX - (piece.XOffset + lastWidth) >= piece.XOffset + width - relMouseX ? 0 : 1;
                        Internal.SetPosition(indexOffset + j - diff);
                        Internal.UpdateText();
                        return;
                    }
                    lastWidth = width;
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
