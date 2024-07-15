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

/// <summary>Represents an editable text area.</summary>
// TODO: Text alignment
// TODO: Horizontal scrolling within bounds, with option
// TODO: Vertical scrolling when bounds exceeded
// TODO: Cap text length
public class UIInputLabel : UIClickableElement
{
    /// <summary>An enumeration of <see cref="EditText(EditType, string, string, Action)"/> operations.</summary>
    public enum EditType
    {
        /// <summary>Replaces the space between the indices with the diff.</summary>
        ADD,
        /// <summary>
        /// If the indices are not equal, deletes the selection. Otherwise, deletes the character preceding the left index.
        /// The deleted content becomes the diff.
        /// </summary>
        DELETE,
        /// <summary>Submits the 'result' text on close (without a diff).</summary>
        SUBMIT
    }

    /// <summary>The box behind the input label.</summary>
    public UIBox Box = null;

    /// <summary>The scroll group containing the label text.</summary>
    public UIScrollGroup ScrollGroup;

    /// <summary>The text to display when the input is empty.</summary>
    public UIElementText PlaceholderInfo;

    /// <summary>The UI style of normal input content.</summary>
    public UIElementStyle InputStyle;

    /// <summary>The UI style of highlighted input content.</summary>
    public UIElementStyle HighlightStyle;

    /// <summary>Data internal to a <see cref="UIInputLabel"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Fired when the text is edited by the user.</summary>
    public Action<string> OnTextEdit;

    /// <summary>Fired when the user submits the text content.</summary>
    public Action<string> OnTextSubmit;
    
    /// <summary>Gets or sets the input text content.</summary>
    public string TextContent
    {
        get => Internal.TextContent;
        set
        {
            Internal.SetTextContent(value);
            UpdateText();
        }
    }

    /// <summary>The current number of input text lines.</summary>
    public int Lines => Internal.TextChain.Sum(piece => piece.Text.Lines.Length);

    /// <summary>Data internal to a <see cref="UIInputLabel"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The raw text content.</summary>
        public string TextContent = "";

        /// <summary>The padding between the <see cref="Box"/> and the label.</summary>
        public int BoxPadding;

        /// <summary>The start cursor position. Acts as an anchorpoint for the end cursor.</summary>
        public int CursorStart = 0;

        /// <summary>The end cursor position.</summary>
        public int CursorEnd = 0;

        /// <summary>The drawn cursor offset location.</summary>
        public Location CursorOffset;

        /// <summary>The minimum cursor position.</summary>
        public readonly int IndexLeft => CursorStart < CursorEnd ? CursorStart : CursorEnd;

        /// <summary>The maximum cursor position.</summary>
        public readonly int IndexRight => CursorEnd > CursorStart ? CursorEnd : CursorStart;

        /// <summary>Whether a string of text is selected between the indices.</summary>
        public readonly bool HasSelection => CursorStart != CursorEnd;

        /// <summary>The text preceding <see cref="IndexLeft"/>.</summary>
        public UIElementText TextLeft;

        /// <summary>The text between <see cref="IndexLeft"/> and <see cref="IndexRight"/>.</summary>
        public UIElementText TextBetween;

        /// <summary>The text following <see cref="IndexRight"/>.</summary>
        public UIElementText TextRight;

        /// <summary>A text chain generated from <see cref="TextLeft"/>, <see cref="TextBetween"/>, and <see cref="TextRight"/>.</summary>
        public List<UIElementText.ChainPiece> TextChain;
        
        /// <summary>Sets both cursor positions at a single index.</summary>
        /// <param name="cursorPos">The cursor positions.</param>
        public void SetPosition(int cursorPos) => CursorStart = CursorEnd = cursorPos;
        
        /// <summary>Clamps the cursor positions to the <see cref="TextContent"/> bounds.</summary>
        public void ClampPositions()
        {
            CursorStart = Math.Clamp(CursorStart, 0, TextContent.Length);
            CursorEnd = Math.Clamp(CursorEnd, 0, TextContent.Length);
        }

        /// <summary>Sets the input text content.</summary>
        /// <param name="content">The new text content.</param>
        public void SetTextContent(string content)
        {
            if (TextContent != content)
            {
                TextContent = content ?? "";
                ClampPositions();
            }
        }

        /// <summary>Updates the text components based on the cursor positions.</summary>
        public readonly void UpdateTextComponents()
        {
            TextLeft.Content = TextContent[..IndexLeft];
            TextBetween.Content = TextContent[IndexLeft..IndexRight];
            TextRight.Content = TextContent[IndexRight..];
        }

        /// <summary>Calculates a screen cursor offset given the current <see cref="TextChain"/>, or <see cref="Location.NaN"/> if none.</summary>
        // TODO: Account for formatting codes
        // TODO: out of range exception here somewhere
        public readonly Location GetCursorOffset()
        {
            double xOffset = 0;
            int cursorIndex = IndexLeft;
            int currentIndex = 0;
            cursorIndex -= TextChain.Sum(piece => piece.SkippedIndices.Where(index => index <= cursorIndex).Count());
            for (int i = 0; i < TextChain.Count; i++)
            {
                UIElementText.ChainPiece piece = TextChain[i];
                for (int j = 0; j < piece.Text.Lines.Length; j++)
                {
                    RenderableTextPart[] parts = piece.Text.Lines[j].Parts;
                    if (parts.Length == 0 && i == TextChain.Count - 1)
                    {
                        return new Location(0, piece.YOffset, 0);
                    }
                    foreach (RenderableTextPart part in parts)
                    {
                        if (currentIndex + part.Text.Length < cursorIndex)
                        {
                            currentIndex += part.Text.Length;
                            xOffset += part.Width;
                            continue;
                        }
                        int relIndex = cursorIndex - currentIndex;
                        double x = xOffset + (part.Text.Length > 0 ? piece.Font.MeasureFancyText(part.Text[..relIndex]) : 0);
                        double y = piece.YOffset + j * piece.Font.FontDefault.Height;
                        return new Location(x, y, 0);
                    }
                    xOffset = 0;
                }
                currentIndex++;
            }
            return Location.NaN;
        }
    }

    /// <summary>Constructs an input label.</summary>
    /// <param name="placeholderInfo">The text to display when the input is empty.</param>
    /// <param name="defaultText">The default input text.</param>
    /// <param name="styles">The clickable styles for the box and info text.</param>
    /// <param name="inputStyle">The style of normal input content.</param>
    /// <param name="highlightStyle">The style of highlighted input content.</param>
    /// <param name="pos">The position of the element.</param>
    /// <param name="renderBox">Whether to render a box behind the input label.</param>
    /// <param name="boxPadding">The padding between the box and the label.</param>
    public UIInputLabel(string placeholderInfo, string defaultText, StyleGroup styles, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos, bool renderBox = false, int boxPadding = 0, StyleGroup scrollBarStyles = null, int scrollBarWidth = 0) : base(styles, pos, requireText: placeholderInfo.Length > 0)
    {
        if (renderBox)
        {
            Internal.BoxPadding = boxPadding;
            pos.ConstantWidthHeight(pos.Width + boxPadding * 2, pos.Height + boxPadding * 2);
            AddChild(Box = new(UIElementStyle.Empty, pos.AtOrigin()) { Enabled = false });
        }
        int outline = renderBox ? styles.Hover.BorderThickness : 0;
        ScrollGroup.AddChild(new Renderable(this, new UIPositionHelper(pos.View).Anchor(UIAnchor.TOP_LEFT)));
        ScrollGroup = new(pos.AtOrigin().ConstantXY(outline, outline).ConstantWidthHeight(pos.Width - outline * 2, pos.Height - outline * 2), scrollBarStyles, scrollBarWidth);
        AddChild(ScrollGroup);
        InputStyle = inputStyle ?? styles.Normal;
        HighlightStyle = highlightStyle ?? styles.Click;
        PlaceholderInfo = new(this, placeholderInfo, true);
        Internal.TextLeft = new(this, null, false, style: InputStyle);
        Internal.TextBetween = new(this, null, false, style: HighlightStyle);
        Internal.TextRight = new(this, null, false, style: InputStyle);
        TextContent = defaultText;
    }

    /// <inheritdoc/>
    public override void OnSelect()
    {
        Enabled = false;
        Pressed = true;
        Position.View.InteractingElement = null;
        TickMouse();
    }

    /// <inheritdoc/>
    public override void OnDeselect()
    {
        SubmitText();
        Internal.SetPosition(0);
        UpdateText();
        Enabled = true;
        Pressed = false;
        Hovered = false;
    }

    /// <summary>Updates the text components based on the cursor positions.</summary>
    public void UpdateText()
    {
        Internal.UpdateTextComponents();
        Internal.TextChain = UIElementText.IterateChain([Internal.TextLeft, Internal.TextBetween, Internal.TextRight], Position.Width).ToList();
        Internal.CursorOffset = (!Selected || Internal.HasSelection) ? Location.NaN : Internal.GetCursorOffset();
        if (Internal.TextChain.Count > 1)
        {
            ScrollGroup.ScrollY.MaxValue = Math.Max((int)Internal.TextChain[^1].YOffset, 0);
            // TODO: Clamp scroll value to cursor view
        }
        else
        {
            ScrollGroup.ScrollY.MaxValue = 0;
            ScrollGroup.ScrollY.Value = 0;
        }
    }

    /// <summary>Performs a user edit on the text content.</summary>
    /// <param name="type">The edit operation.</param>
    /// <param name="diff">The added or deleted text.</param>
    /// <param name="result">The result of the operation pre-validation.</param>
    /// <param name="beforeUpdate">Fires after the text content is set but before internal values are updated.</param>
    public void EditText(EditType type, string diff, string result, Action beforeUpdate = null)
    {
        Internal.SetTextContent(ValidateEdit(type, diff, result));
        beforeUpdate?.Invoke();
        UpdateText();
        (type == EditType.SUBMIT ? OnTextSubmit : OnTextEdit)?.Invoke(TextContent);
    }

    // TODO: Cap length
    /// <summary>Validates a user edit of the text content.</summary>
    /// <param name="type">The edit operation.</param>
    /// <param name="diff">The added or deleted text.</param>
    /// <param name="result">The result of the operation pre-validation.</param>
    /// <returns>A validated <see cref="TextContent"/> string.</returns>
    public virtual string ValidateEdit(EditType type, string diff, string result) => result;

    /// <summary>Adds text given two selection indices.</summary>
    /// <param name="text">The text to add.</param>
    /// <param name="indexLeft">The left index position.</param>
    /// <param name="indexRight">The right index position.</param>
    public void AddText(string text, int indexLeft, int indexRight)
    {
        string result = TextContent[..indexLeft] + text + TextContent[indexRight..];
        Internal.CursorEnd = Internal.CursorStart += text.Length;
        EditText(EditType.ADD, text, result);
    }

    /// <summary>Deletes text between two indices.</summary>
    /// <param name="indexLeft">The left index position.</param>
    /// <param name="indexRight">The right index position.</param>
    public void DeleteText(int indexLeft, int indexRight)
    {
        string diff = TextContent[indexLeft..indexRight];
        string result = TextContent[..indexLeft] + TextContent[indexRight..];
        EditText(EditType.DELETE, diff, result, () => Internal.SetPosition(indexLeft));
    }

    /// <summary>Submits the current text content.</summary>
    public void SubmitText() => EditText(EditType.SUBMIT, "", TextContent);

    /// <summary>Deletes text based on the <see cref="KeyHandlerState.InitBS"/> value.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickBackspaces(KeyHandlerState keys)
    {
        if (keys.InitBS == 0 || TextContent.Length == 0 || Internal.IndexRight == 0)
        {
            return;
        }
        if (Internal.HasSelection)
        {
            DeleteText(Internal.IndexLeft, Internal.IndexRight);
            keys.InitBS--;
        }
        if (keys.InitBS > 0)
        {
            int index = Math.Max(Internal.IndexLeft - keys.InitBS, 0);
            DeleteText(index, Internal.IndexRight);
        }
    }

    /// <summary>Adds text based on the <see cref="KeyHandlerState.KeyboardString"/> value.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickContent(KeyHandlerState keys)
    {
        if (keys.KeyboardString.Length > 0)
        {
            AddText(keys.KeyboardString, Internal.IndexLeft, Internal.IndexRight);
        }
    }

    // TODO: Handle ctrl left/right, handle up/down arrows
    /// <inheritdoc/>
    public override void NavigateLeftRight(int value)
    {
        bool shiftDown = Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        if (Internal.HasSelection && !shiftDown)
        {
            Internal.CursorEnd = value < 0 ? Internal.IndexLeft : Internal.IndexRight;
        }
        else
        {
            Internal.CursorEnd += value;
        }
        if (!shiftDown)
        {
            Internal.CursorStart = Internal.CursorEnd;
        }
        Internal.ClampPositions();
        UpdateText();
    }

    /// <summary>Handles the mouse being pressed at a cursor position.</summary>
    /// <param name="cursorPos">The cursor position.</param>
    /// <param name="shiftDown">Whether the shift key is being held.</param>
    // TODO: Account for formatting codes
    public void TickMousePosition(int cursorPos, bool shiftDown)
    {
        if (Internal.CursorEnd == cursorPos && MousePreviouslyDown == shiftDown)
        {
            return;
        }
        Internal.CursorEnd = Math.Max(cursorPos, 0);
        if (!MousePreviouslyDown && !shiftDown)
        {
            Internal.CursorStart = Internal.CursorEnd;
        }
        UpdateText();
    }

    /// <summary>Modifies the current selection based on mouse clicks/drags.</summary>
    public void TickMouse()
    {
        if (!MouseDown || Internal.TextChain.Count == 0)
        {
            return;
        }
        bool shiftDown = Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        float relMouseX = Window.MouseX - X;
        float relMouseY = Window.MouseY - Y + ScrollGroup.ScrollY.Value;
        UIElementText.ChainPiece lastPiece = Internal.TextChain[^1];
        if (lastPiece.YOffset + (lastPiece.Font.FontDefault.Height * lastPiece.Text.Lines.Length) < relMouseY)
        {
            TickMousePosition(TextContent.Length, shiftDown);
            return;
        }
        int indexOffset = 0;
        foreach (UIElementText.ChainPiece piece in Internal.TextChain)
        {
            for (int i = 0; i < piece.Text.Lines.Length; i++)
            {
                RenderableTextLine line = piece.Text.Lines[i];
                string content = line.ToString();
                if (piece.YOffset + (i + 1) * piece.Font.FontDefault.Height >= relMouseY)
                {
                    if (line.Width < relMouseX)
                    {
                        TickMousePosition(indexOffset + content.Length, shiftDown);
                        return;
                    }
                    float lastWidth = 0;
                    for (int j = 0; j <= content.Length; j++)
                    {
                        float width = piece.Font.MeasureFancyText(content[..j]);
                        if (width >= relMouseX)
                        {
                            int diff = relMouseX - lastWidth >= width - relMouseX ? 0 : 1;
                            TickMousePosition(indexOffset + j - diff, shiftDown);
                            return;
                        }
                        lastWidth = width;
                    }
                }
                indexOffset += content.Length;
            }
            indexOffset++;
        }
    }

    /// <summary>Handles various control key combinations.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickControlKeys(KeyHandlerState keys)
    {
        if (keys.CopyPressed && Internal.HasSelection)
        {
            TextCopy.ClipboardService.SetText(Internal.TextBetween.Content);
        }
        if (keys.AllPressed)
        {
            Internal.CursorStart = 0;
            Internal.CursorEnd = TextContent.Length;
            UpdateText();
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
        TickBackspaces(keys);
        TickContent(keys);
        TickMouse();
        TickControlKeys(keys);
        // TODO: Handle ctrl+Z, ctrl+Y
        // TODO: Handle HOME, END
    }

    public class Renderable(UIInputLabel label, UIPositionHelper pos) : UIElement(pos)
    {
        public UIInputLabel Label = label;

        public override void Render(ViewUI2D view, double delta, UIElementStyle style)
        {
            int inset = Label.Box is not null ? Label.Styles.Hover.BorderThickness : 0;
            int x = X + Label.Internal.BoxPadding - inset;
            int y = Y + Label.Internal.BoxPadding - inset;
            bool isInfo = Label.TextContent.Length == 0;
            bool renderInfo = isInfo && style.CanRenderText(Label.PlaceholderInfo);
            if (renderInfo)
            {
                style.TextFont.DrawFancyText(Label.PlaceholderInfo, new Location(x, y, 0));
            }
            else
            {
                UIElementText.RenderChain(Label.Internal.TextChain, x, y);
            }
            if (Label.Internal.CursorOffset.IsNaN())
            {
                return;
            }
            // TODO: Cursor blink modes
            Engine.Textures.White.Bind();
            Renderer2D.SetColor(Label.InputStyle.BorderColor);
            int lineWidth = Label.InputStyle.BorderThickness / 2;
            int lineHeight = (renderInfo ? Label.PlaceholderInfo : Label.Internal.TextLeft).CurrentStyle.TextFont.FontDefault.Height;
            view.Rendering.RenderRectangle(view.UIContext, x + Label.Internal.CursorOffset.XF - lineWidth, y + Label.Internal.CursorOffset.YF, x + Label.Internal.CursorOffset.XF + lineWidth, y + Label.Internal.CursorOffset.YF + lineHeight);
            Renderer2D.SetColor(Color4.White);
        }
    }

    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        Box?.Render(view, delta, style);
    }

    /// <inheritdoc/>
    public override List<string> GetDebugInfo()
    {
        List<string> info = base.GetDebugInfo();
        info.Add($"^7Indices: ^3[{Internal.IndexLeft} {Internal.IndexRight}] ^&| ^7Cursors: ^3[{Internal.CursorStart} {Internal.CursorEnd}]");
        return info;
    }
}
