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
// TODO: Horizontal scrolling within bounds
public class UIInputLabel : UIClickableElement
{
    /// <summary>An enumeration of <see cref="EditText(EditType, string, string, Action)"/> operations.</summary>
    public enum EditType
    {
        /// <summary>Replaces the space between the indices with the diff.</summary>
        Add,
        /// <summary>
        /// If the indices are not equal, deletes the selection. Otherwise, deletes the character preceding the left index.
        /// The deleted content becomes the diff.
        /// </summary>
        Delete,
        /// <summary>Submits the 'result' text on close (without a diff).</summary>
        Submit
    }

    /// <summary>The text to display when the input is empty.</summary>
    public UIElementText Info;

    /// <summary>The UI style of normal input content.</summary>
    public UIElementStyle InputStyle;

    /// <summary>The UI style of highlighted input content.</summary>
    public UIElementStyle HighlightStyle;

    /// <summary>Whether the input label is currently selected.</summary>
    public bool Selected = false; // TODO: Provide a UIElement-native solution for this

    /// <summary>Data internal to a <see cref="UIInputLabel"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Fired when the text is edited by the user.</summary>
    public Action<string> TextEdited;

    /// <summary>Fired when the user submits the text content.</summary>
    public Action<string> TextSubmitted;

    /// <summary>Fired when the user de-selects the input label.</summary>
    public Action Closed;
    
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
        public string TextContent = string.Empty;

        /// <summary>The left cursor position. Acts as an anchorpoint for the right cursor.</summary>
        public int CursorLeft = 0;

        /// <summary>The right cursor position.</summary>
        public int CursorRight = 0;

        /// <summary>The drawn cursor offset location.</summary>
        public Location CursorOffset;

        /// <summary>The minimum cursor position.</summary>
        public readonly int IndexLeft => CursorLeft < CursorRight ? CursorLeft : CursorRight;

        /// <summary>The maximum cursor position.</summary>
        public readonly int IndexRight => CursorRight > CursorLeft ? CursorRight : CursorLeft;

        /// <summary>Whether a string of text is selected between the indices.</summary>
        public readonly bool IsSelection => CursorLeft != CursorRight;

        /// <summary>The text preceding <see cref="IndexLeft"/>.</summary>
        public UIElementText TextLeft;

        /// <summary>The text between <see cref="IndexLeft"/> and <see cref="IndexRight"/>.</summary>
        public UIElementText TextBetween;

        /// <summary>The text following <see cref="IndexRight"/>.</summary>
        public UIElementText TextRight;

        /// <summary>A chain of <see cref="TextLeft"/>, <see cref="TextBetween"/>, and <see cref="TextRight"/>.</summary>
        public List<UIElementText.ChainPiece> TextChain;
        public UIPositionHelper OriginalBounds;
        
        /// <summary>Sets both cursor positions at a single index.</summary>
        /// <param name="cursorPos">The cursor positions.</param>
        public void SetPosition(int cursorPos) => CursorLeft = CursorRight = cursorPos;
        
        /// <summary>Clamps the cursor positionst to the <see cref="TextContent"/> bounds.</summary>
        public void ClampPositions()
        {
            CursorLeft = Math.Clamp(CursorLeft, 0, TextContent.Length);
            CursorRight = Math.Clamp(CursorRight, 0, TextContent.Length);
        }

        /// <summary>Sets the input text content.</summary>
        /// <param name="content">The new text content.</param>
        public void SetTextContent(string content)
        {
            if (TextContent == content)
            {
                return;
            }
            TextContent = content ?? string.Empty;
            ClampPositions();
        }

        /// <summary>Updates the text components based on the cursor positions.</summary>
        public readonly void UpdateTextComponents()
        {
            TextLeft.Content = TextContent[..IndexLeft];
            TextBetween.Content = TextContent[IndexLeft..IndexRight];
            TextRight.Content = TextContent[IndexRight..];
        }

        /// <summary>Calculates a screen cursor offset given the current <see cref="TextChain"/>.</summary>
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
            return Location.NaN; // Should never happen.
        }
    }

    /// <summary>Constructs an input label.</summary>
    /// <param name="info">The text to display when the input is empty.</param>
    /// <param name="defaultText">The default input text.</param>
    /// <param name="infoStyles">The clickable styles for the info text.</param>
    /// <param name="inputStyle">The style of normal input content.</param>
    /// <param name="highlightStyle">The style of highlighted input content.</param>
    /// <param name="pos">The position of the element.</param>
    public UIInputLabel(string info, string defaultText, StyleGroup infoStyles, UIElementStyle inputStyle, UIElementStyle highlightStyle, UIPositionHelper pos) : base(infoStyles, pos, requireText: info.Length > 0)
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
        if (Enabled)
        {
            Selected = true;
            Enabled = false;
            Pressed = true;
            Position.View.InteractingElement = null;
            TickMouse(false);
        }
    }

    /// <summary>Submits and de-selects the input label.</summary>
    public void HandleClose()
    {
        SubmitText();
        Selected = false;
        Enabled = true;
        Pressed = false;
    }

    /// <inheritdoc/>
    public override void MouseLeftDownOutside() => Closed?.Invoke();

    /// <summary>Updates the text components based on the cursor positions.</summary>
    public void UpdateText()
    {
        Internal.UpdateTextComponents();
        Internal.TextChain = UIElementText.IterateChain([Internal.TextLeft, Internal.TextBetween, Internal.TextRight], Position.Width).ToList();
        Internal.CursorOffset = Internal.IsSelection ? Location.NaN : Internal.GetCursorOffset();
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
        (type == EditType.Submit ? TextSubmitted : TextEdited)?.Invoke(TextContent);
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
        Internal.CursorRight = Internal.CursorLeft += text.Length;
        EditText(EditType.Add, text, result);
    }

    /// <summary>Deletes text between two indices.</summary>
    /// <param name="indexLeft">The left index position.</param>
    /// <param name="indexRight">The right index position.</param>
    public void DeleteText(int indexLeft, int indexRight)
    {
        string diff = TextContent[indexLeft..indexRight];
        string result = TextContent[..indexLeft] + TextContent[indexRight..];
        EditText(EditType.Delete, diff, result, () => Internal.SetPosition(indexLeft));
    }

    /// <summary>Submits the current text content.</summary>
    public void SubmitText() => EditText(EditType.Submit, string.Empty, TextContent);

    /// <summary>Deletes text based on the <see cref="KeyHandlerState.InitBS"/> value.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickBackspaces(KeyHandlerState keys)
    {
        if (keys.InitBS == 0 || TextContent.Length == 0 || Internal.IndexRight == 0)
        {
            return;
        }
        if (Internal.IsSelection)
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
    /// <summary>Modifies the current selection based on the <see cref="KeyHandlerState.LeftRights"/> value.</summary>
    /// <param name="keys">The current keyboard state.</param>
    /// <param name="shiftDown">Whether the shift key is being held.</param>
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
        UpdateText();
    }

    /// <summary>Handles the mouse being pressed at a cursor position.</summary>
    /// <param name="cursorPos">The cursor position.</param>
    /// <param name="shiftDown">Whether the shift key is being held.</param>
    public void TickMousePosition(int cursorPos, bool shiftDown)
    {
        if (Internal.CursorRight == cursorPos && MousePreviouslyDown == shiftDown)
        {
            return;
        }
        Internal.CursorRight = Math.Max(cursorPos, 0);
        if (!MousePreviouslyDown && !shiftDown)
        {
            Internal.CursorLeft = Internal.CursorRight;
        }
        UpdateText();
    }

    /// <summary>Modifies the current selection based on mouse clicks/drags.</summary>
    /// <param name="shiftDown">Whether the shift key is being held.</param>
    public void TickMouse(bool shiftDown)
    {
        if (!MouseDown || Internal.TextChain.Count == 0)
        {
            return;
        }
        float relMouseX = Window.MouseX - X;
        float relMouseY = Window.MouseY - Y;
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
        if (keys.CopyPressed && Internal.IsSelection)
        {
            TextCopy.ClipboardService.SetText(Internal.TextBetween.Content);
        }
        if (keys.AllPressed)
        {
            Internal.CursorLeft = 0;
            Internal.CursorRight = TextContent.Length;
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
        if (keys.Escaped)
        {
            Closed?.Invoke();
            return;
        }
        bool shiftDown = Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        TickBackspaces(keys);
        TickContent(keys);
        TickArrowKeys(keys, shiftDown);
        TickMouse(shiftDown);
        TickControlKeys(keys);
        // TODO: handle ctrl+Z, ctrl+Y
    }

    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        bool isInfo = TextContent.Length == 0;
        bool renderInfo = isInfo && style.CanRenderText(Info);
        if (renderInfo)
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
        Renderer2D.SetColor(InputStyle.BorderColor);
        int lineWidth = InputStyle.BorderThickness / 2;
        int lineHeight = (renderInfo ? Info : Internal.TextLeft).CurrentStyle.TextFont.FontDefault.Height;
        view.Rendering.RenderRectangle(view.UIContext, X + Internal.CursorOffset.XF - lineWidth, Y + Internal.CursorOffset.YF, X + Internal.CursorOffset.XF + lineWidth, Y + Internal.CursorOffset.YF + lineHeight);
        Renderer2D.SetColor(Color4.White);
    }
}
