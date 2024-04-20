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
public class UIInputLabel : UIClickableElement
{
    /// <summary>An enumeration of <see cref="EditText(EditType, string, string)"/> operations.</summary>
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
    public int Lines = 1; // TODO: Implement
    public int MaxLength; // TODO: Implement

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
            if (Internal.TextContent == value)
            {
                return;
            }
            Internal.TextContent = value ?? string.Empty;
            Internal.ClampPositions();
            Internal.UpdateText();
        }
    }

    /// <summary>Data internal to a <see cref="UIInputLabel"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The raw text content.</summary>
        public string TextContent = string.Empty;

        /// <summary>The left cursor position. Acts as an anchorpoint for the right cursor.</summary>
        public int CursorLeft = 0;

        /// <summary>The right cursor position.</summary>
        public int CursorRight = 0;

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

        /// <summary>A list containing <see cref="TextLeft"/>, <see cref="TextBetween"/>, and <see cref="TextRight"/>.</summary>
        public List<UIElementText> TextChain;
        public UIPositionHelper OriginalBounds;
        public int Lines;
        
        /// <summary>Sets both cursor positions at a single index.</summary>
        /// <param name="cursorPos">The cursor positions.</param>
        public void SetPosition(int cursorPos) => CursorLeft = CursorRight = cursorPos;
        
        /// <summary>Clamps the cursor positionst to the <see cref="TextContent"/> bounds.</summary>
        public void ClampPositions()
        {
            CursorLeft = Math.Clamp(CursorLeft, 0, TextContent.Length);
            CursorRight = Math.Clamp(CursorRight, 0, TextContent.Length);
        }

        /// <summary>Updates the <see cref="TextChain"/> values based on the cursor positions.</summary>
        public readonly void UpdateText()
        {
            TextLeft.Content = TextContent[..IndexLeft];
            TextBetween.Content = TextContent[IndexLeft..IndexRight];
            TextRight.Content = TextContent[IndexRight..];
        }
    }

    /// <summary>Constructs an input label.</summary>
    /// <param name="info">The text to display when the input is empty.</param>
    /// <param name="defaultText">The default input text.</param>
    /// <param name="infoStyles">The clickable styles for the info text.</param>
    /// <param name="inputStyle">The style of normal input content.</param>
    /// <param name="highlightStyle">The style of highlighted input content.</param>
    /// <param name="pos">The position of the element.</param>
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

    /// <summary>Performs a user edit on the text content.</summary>
    /// <param name="type">The edit operation.</param>
    /// <param name="diff">The added or deleted text.</param>
    /// <param name="result">The result of the operation pre-validation.</param>
    public void EditText(EditType type, string diff, string result)
    {
        TextContent = ValidateEdit(type, diff, result);
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
        EditText(EditType.Delete, diff, result);
        Internal.SetPosition(indexLeft);
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
        Internal.UpdateText();
    }

    /// <summary>Adds text based on the <see cref="KeyHandlerState.KeyboardString"/> value.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickContent(KeyHandlerState keys)
    {
        if (keys.KeyboardString.Length > 0)
        {
            AddText(keys.KeyboardString, Internal.IndexLeft, Internal.IndexRight);
            Internal.UpdateText();
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
        Internal.UpdateText();
    }

    /// <summary>Handles the mouse being pressed at a cursor position.</summary>
    /// <param name="cursorPos">The cursor position.</param>
    /// <param name="shiftDown">Whether the shift key is being held.</param>
    public void TickMousePosition(int cursorPos, bool shiftDown)
    {
        Internal.CursorRight = Math.Max(cursorPos, 0);
        if (!MousePreviouslyDown && !shiftDown)
        {
            Internal.CursorLeft = Internal.CursorRight;
        }
        Internal.UpdateText();
    }

    /// <summary>Modifies the current selection based on mouse clicks/drags.</summary>
    /// <param name="shiftDown">Whether the shift key is being held.</param>
    public void TickMouse(bool shiftDown)
    {
        if (!MouseDown)
        {
            return;
        }
        List<UIElementText.ChainPiece> pieces = UIElementText.IterateChain(Internal.TextChain).ToList();
        if (pieces.Count == 0)
        {
            return;
        }
        float relMouseX = Window.MouseX - X;
        float relMouseY = Window.MouseY - Y;
        if (pieces[^1].YOffset + pieces[^1].Text.CurrentStyle.FontHeight < relMouseY)
        {
            TickMousePosition(TextContent.Length, shiftDown);
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
                    TickMousePosition(indexOffset + content.Length, shiftDown);
                    return;
                }
                float lastWidth = 0;
                for (int j = 0; j <= content.Length; j++)
                {
                    float width = piece.Text.CurrentStyle.TextFont.MeasureFancyText(content[..j]);
                    if (piece.XOffset + width >= relMouseX)
                    {
                        int diff = relMouseX - (piece.XOffset + lastWidth) >= piece.XOffset + width - relMouseX ? 0 : 1;
                        TickMousePosition(indexOffset + j - diff, shiftDown);
                        return;
                    }
                    lastWidth = width;
                }
            }
            indexOffset += content.Length;
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
            Internal.UpdateText();
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
