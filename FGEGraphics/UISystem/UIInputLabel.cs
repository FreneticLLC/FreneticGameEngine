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
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
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
// TODO: Cap text length
// TODO: HasEdited
public class UIInputLabel : UIElement
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

    /// <summary>Wraps a <see cref="UIInteractionStyles"/> instance with logic specific to input labels.</summary>
    /// <param name="styles">The base interaction styles.</param>
    public struct Styles(UIInteractionStyles styles)
    {
        /// <summary>The styling logic for an input label.</summary>
        public readonly UIStyle Styling(UIElement element) => element.IsFocused ? styles.Press : styles.Styling(element);

        /// <summary>Calls <see cref="UIStyling(System.Func{UIElement, UIStyle})"/>.</summary>
        public static implicit operator UIStyling(Styles styles) => new(styles.Styling);
    }

    /// <inheritdoc/>
    public override string Name => "Input Label";

    /// <summary>The paragraph to display the state of this input label.</summary>
    public UIInputParagraph Paragraph;

    /// <summary>The box behind the input label.</summary>
    public UIBox Box = null;

    /// <summary>The scroll group containing the label text.</summary>
    public UIScrollGroup ScrollGroup;

    /// <summary>The text to display when the input is empty.</summary>
    public UILabel PlaceholderInfo;

    /// <summary>Whether the input label supports multiple lines.</summary>
    [UIDebug]
    public bool Multiline = true;

    /// <summary>The max length of the text, or 0 if uncapped.</summary>
    [UIDebug]
    public int MaxLength = 0;

    /// <summary>Data internal to a <see cref="UIInputLabel"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Fired when the text is edited by the user.</summary>
    public Action<string> OnTextEdit;

    /// <summary>Fired when the user submits the text content.</summary>
    public Action<string> OnTextSubmit;
    
    /// <summary>Gets or sets the input text content.</summary>
    public string Content
    {
        get => Paragraph.Content;
        set
        {
            Paragraph.Content = value;
            UpdateRenderState();
        }
    }

    /// <summary>The current number of input text lines.</summary>
    public int Lines => Paragraph.Internal.Renderables.Sum(piece => piece.Text.Lines.Length);

    /// <summary>The padding offset for the rendered text, if any.</summary>
    public int TextPadding => Box is not null ? (Internal.BoxPadding - ElementInternal.Style.BorderThickness) : 0;

    /// <summary>Data internal to a <see cref="UIInputLabel"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The padding between the <see cref="Box"/> and the label.</summary>
        public int BoxPadding = 0;

        /// <summary>Whether to enforce max width or use a horizontal scroll group.</summary>
        public bool HasMaxWidth;
    }

    /// <summary>Constructs an input label.</summary>
    /// <param name="placeholderInfo">The text to display when the input is empty.</param>
    /// <param name="defaultText">The default input text.</param>
    /// <param name="styling">The styling logic of the element.</param>
    /// <param name="inputStyling">The style of normal input content.</param>
    /// <param name="highlightStyling">The style of highlighted input content.</param>
    /// <param name="layout">The layout of the element.</param>
    /// <param name="maxWidth">Whether to enforce a max width. If false, will use horizontal scrolling.</param>
    /// <param name="renderBox">Whether to render a box behind the input label.</param>
    /// <param name="boxPadding">The padding between the box and the label.</param>
    /// <param name="scrollBarStyles">The styles for the scroll bar.</param>
    /// <param name="scrollBarWidth">The width of the scroll bar.</param>
    /// <param name="scrollBarX">Whether to add a horizontal scroll bar.</param>
    /// <param name="scrollBarY">Whether to add a vertical scroll bar.</param>
    /// <param name="scrollBarXAnchor">The anchor of the horizontal scroll bar.</param>
    /// <param name="scrollBarYAnchor">The anchor of the vertical scroll bar.</param>
    public UIInputLabel(string placeholderInfo, string defaultText, UIStyling styling, UIStyling inputStyling, UIStyling highlightStyling, UILayout layout, bool maxWidth = true, bool renderBox = false, int boxPadding = 0, UIInteractionStyles scrollBarStyles = null, int scrollBarWidth = 0, bool scrollBarX = false, bool scrollBarY = false, UIAnchor scrollBarXAnchor = null, UIAnchor scrollBarYAnchor = null) : base(styling, layout)
    {
        if (renderBox)
        {
            Internal.BoxPadding = boxPadding;
            // TODO: properly handle padding
            //UILayout baseLayout = new(layout);
            //layout.SetSize(() => baseLayout.Width + boxPadding * 2, () => baseLayout.Height + boxPadding * 2);
            AddChild(Box = new(styling.Bind(this), new UILayout().SetSize(() => layout.Width, () => layout.Height)) { IsEnabled = false });
        }
        int Inset() => Box is not null ? ElementInternal.Style.BorderThickness : 0; // there should definitely be a system for this
        UILayout scrollGroupLayout = new UILayout().SetPosition(Inset, Inset).SetSize(() => layout.Width - Inset() * 2, () => layout.Height - Inset() * 2);
        ScrollGroup = new(scrollGroupLayout, scrollBarStyles ?? UIStyling.Empty, scrollBarWidth, !maxWidth && scrollBarX, scrollBarY, scrollBarXAnchor, scrollBarYAnchor) { IsEnabled = false };
        ScrollGroup.AddScrollableChild(PlaceholderInfo = new UILabel(placeholderInfo, styling.Bind(this), new UILayout().SetPosition(() => TextPadding, () => TextPadding)) { IsEnabled = false });
        ScrollGroup.AddScrollableChild(Paragraph = new UIInputParagraph(highlightStyling, inputStyling, highlightStyling, new UILayout().SetPosition(() => TextPadding, () => TextPadding)) { IsEnabled = false });
        AddChild(ScrollGroup);
        Internal.HasMaxWidth = maxWidth;
        Paragraph.SetContent(defaultText);
    }

    /// <inheritdoc/>
    public override void Focused()
    {
        TickMouse();
        Paragraph.RenderCursor = true;
        UpdateRenderState();
    }

    /// <inheritdoc/>
    public override void Unfocused()
    {
        if ((ScrollGroup.ScrollX.ScrollBar?.IsPressed | ScrollGroup.ScrollY.ScrollBar?.IsPressed) ?? false)
        {
            IsFocused = true;
            return;
        }
        SubmitText();
        Paragraph.SetCursorPosition(0);
        Paragraph.RenderCursor = false;
        Paragraph.UpdateRenderState();
        ScrollGroup.ScrollX.Reset();
        ScrollGroup.ScrollY.Reset();
        PlaceholderInfo.RenderSelf = Content.Length == 0;
    }

    // FIXME: Paragraph.Width still retains last value when deleting all, incorrect MaxValue calculation 
    /// <summary>Updates the horizontal scroll values based on the text width and cursor position.</summary>
    public void UpdateScrollGroupX()
    {
        if (Internal.HasMaxWidth)
        {
            return;
        }
        ScrollGroup.ScrollX.MaxValue = Math.Max(Paragraph.Width + TextPadding * 2 - ScrollGroup.Width, 0);
        ScrollGroup.ScrollX.ScrollToPos((int)Paragraph.InputInternal.CursorRenderOffset.X, (int)Paragraph.InputInternal.CursorRenderOffset.X + TextPadding * 2 - ScrollGroup.ScrollX.Value);
        ScrollGroup.ScrollX.Clamp();
    }

    /// <summary>Updates the vertical scroll values based on the text height and cursor position.</summary>
    public void UpdateScrollGroupY()
    {
        if (Paragraph.Internal.Renderables.Count == 0)
        {
            ScrollGroup.ScrollY.Reset();
            return;
        }
        int lastLineHeight = Paragraph.InputInternal.LabelRight.Style.FontHeight + TextPadding * 2;
        ScrollGroup.ScrollY.MaxValue = Math.Max((int)Paragraph.Internal.Renderables[^1].YOffset + lastLineHeight - ScrollGroup.Height, 0);
        ScrollGroup.ScrollY.ScrollToPos((int)Paragraph.InputInternal.CursorRenderOffset.Y, (int)Paragraph.InputInternal.CursorRenderOffset.Y + lastLineHeight - ScrollGroup.ScrollY.Value);
        ScrollGroup.ScrollX.Clamp();
    }

    /// <summary>Updates the <see cref="ScrollGroup"/> values.</summary>
    public void UpdateScrollGroup()
    {
        if (!Paragraph.InputInternal.CursorRenderOffset.IsNaN())
        {
            UpdateScrollGroupX();
            UpdateScrollGroupY();
        }    
    }

    /// <summary>Updates the text components based on the cursor positions.</summary>
    public void UpdateRenderState()
    {
        Paragraph.UpdateRenderState();
        UpdateScrollGroup();
        PlaceholderInfo.RenderSelf = Content.Length == 0;
    }

    /// <summary>Performs a user edit on the text content.</summary>
    /// <param name="type">The edit operation.</param>
    /// <param name="diff">The added or deleted text.</param>
    /// <param name="result">The result of the operation pre-validation.</param>
    /// <param name="beforeUpdate">Fires after the text content is set but before internal values are updated.</param>
    public void EditText(EditType type, string diff, string result, Action beforeUpdate = null)
    {
        Paragraph.SetContent(ValidateEdit(type, diff, result));
        beforeUpdate?.Invoke();
        UpdateRenderState();
        (type == EditType.SUBMIT ? OnTextSubmit : OnTextEdit)?.Invoke(Content);
    }

    /// <summary>Validates a user edit of the text content.</summary>
    /// <param name="type">The edit operation.</param>
    /// <param name="diff">The added or deleted text.</param>
    /// <param name="result">The result of the operation pre-validation.</param>
    /// <returns>A validated <see cref="Content"/> string.</returns>
    public virtual string ValidateEdit(EditType type, string diff, string result)
    {
        if (type != EditType.ADD)
        {
            return result;
        }
        int originalLength = result.Length;
        int overflow = result.Length - MaxLength;
        if (MaxLength > 0 && overflow > 0)
        {
            string newDiff = overflow < diff.Length ? diff[..(diff.Length - overflow)] : "";
            result = result[..(Paragraph.CursorLeft - diff.Length)] + newDiff + result[Paragraph.CursorRight..];
        }
        if (!Multiline && result.Contains('\n'))
        {
            result = result.Replace("\n", "");
        }
        // TODO: this shouldn't be here
        Paragraph.SetCursorPosition(Paragraph.CursorLeft - (originalLength - result.Length));
        return result;
    }

    /// <summary>Adds text given two selection indices.</summary>
    /// <param name="text">The text to add.</param>
    /// <param name="indexLeft">The left index position.</param>
    /// <param name="indexRight">The right index position.</param>
    public void AddText(string text, int indexLeft, int indexRight)
    {
        string result = Content[..indexLeft] + text + Content[indexRight..];
        Paragraph.SetCursorPosition(Paragraph.CursorLeft + text.Length);
        EditText(EditType.ADD, text, result);
    }

    /// <summary>Deletes text between two indices.</summary>
    /// <param name="indexLeft">The left index position.</param>
    /// <param name="indexRight">The right index position.</param>
    public void DeleteText(int indexLeft, int indexRight)
    {
        string diff = Content[indexLeft..indexRight];
        string result = Content[..indexLeft] + Content[indexRight..];
        EditText(EditType.DELETE, diff, result, () => Paragraph.SetCursorPosition(indexLeft));
    }

    /// <summary>Submits the current text content.</summary>
    public void SubmitText() => EditText(EditType.SUBMIT, "", Content);

    /// <summary>Deletes text based on the <see cref="KeyHandlerState.InitBS"/> value.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickBackspaces(KeyHandlerState keys)
    {
        if (keys.InitBS == 0 || Content.Length == 0 || Paragraph.CursorRight == 0)
        {
            return;
        }
        if (Paragraph.HasSelection)
        {
            DeleteText(Paragraph.CursorLeft, Paragraph.CursorRight);
            keys.InitBS--;
        }
        if (keys.InitBS > 0)
        {
            int index = Math.Max(Paragraph.CursorLeft - keys.InitBS, 0);
            DeleteText(index, Paragraph.CursorRight);
        }
    }

    /// <summary>Adds text based on the <see cref="KeyHandlerState.KeyboardString"/> value.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickContent(KeyHandlerState keys)
    {
        if (keys.KeyboardString.Length > 0)
        {
            AddText(keys.KeyboardString, Paragraph.CursorLeft, Paragraph.CursorRight);
        }
    }

    // TODO: Handle ctrl left/right, handle up/down arrows
    /// <inheritdoc/>
    public override void Navigated(int horizontal, int vertical)
    {
        if (horizontal == 0)
        {
            return;
        }
        bool shiftDown = View.Client.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        if (Paragraph.HasSelection && !shiftDown)
        {
            Paragraph.CursorEnd = horizontal < 0 ? Paragraph.CursorLeft : Paragraph.CursorRight;
        }
        else
        {
            Paragraph.CursorEnd += horizontal;
        }
        if (!shiftDown)
        {
            Paragraph.CursorStart = Paragraph.CursorEnd;
        }
        Paragraph.ClampCursorPositions();
        UpdateRenderState();
    }

    /// <summary>Handles the mouse being pressed at a cursor position.</summary>
    /// <param name="cursorPos">The cursor position.</param>
    /// <param name="shiftDown">Whether the shift key is being held.</param>
    // TODO: Account for formatting codes
    public void TickMousePosition(int cursorPos, bool shiftDown)
    {
        if (Paragraph.CursorEnd == cursorPos && View.MousePreviouslyDown == shiftDown)
        {
            return;
        }
        Paragraph.CursorEnd = Math.Max(cursorPos, 0);
        if (!View.MousePreviouslyDown && !shiftDown)
        {
            Paragraph.CursorStart = Paragraph.CursorEnd;
        }
        UpdateRenderState();
    }

    /// <summary>Modifies the current selection based on mouse clicks/drags.</summary>
    public void TickMouse()
    {
        if (!View.MouseDown || Paragraph.Internal.Renderables.Count == 0)
        {
            return;
        }
        bool BarPressed(UIBox bar) => (bar?.IsPressed | bar?.SelfContains((int)View.Client.MouseX, (int)View.Client.MouseY)) ?? false;
        if (BarPressed(ScrollGroup.ScrollX.ScrollBar) || BarPressed(ScrollGroup.ScrollY.ScrollBar))
        {
            return;
        }
        bool shiftDown = View.Client.Window.KeyboardState.IsKeyDown(Keys.LeftShift);
        int relMouseX = (int)View.Client.MouseX - Paragraph.X;
        int relMouseY = (int)View.Client.MouseY - Paragraph.Y;
        int mouseCursorPosition = Paragraph.GetIndexForLocation(relMouseX, relMouseY);
        if (mouseCursorPosition != -1)
        {
            TickMousePosition(mouseCursorPosition, shiftDown);
        }
    }

    /// <summary>Handles various control key combinations.</summary>
    /// <param name="keys">The current keyboard state.</param>
    public void TickControlKeys(KeyHandlerState keys)
    {
        if (keys.CopyPressed && Paragraph.HasSelection)
        {
            // TODO: Paragraph.SelectedContent
            TextCopy.ClipboardService.SetText(Paragraph.InputInternal.LabelCenter.Content);
        }
        if (keys.AllPressed)
        {
            // TODO: Paragraph.SelectAll()
            Paragraph.CursorStart = 0;
            Paragraph.CursorEnd = Content.Length;
            UpdateRenderState();
        }
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (!IsFocused)
        {
            return;
        }
        KeyHandlerState keys = View.Client.Keyboard.BuildingState;
        TickBackspaces(keys);
        TickContent(keys);
        TickMouse();
        TickControlKeys(keys);
        // TODO: Handle ctrl+Z, ctrl+Y
        // TODO: Handle HOME, END
    }

    /// <inheritdoc/>
    public override void Init()
    {
        UpdateRenderState();
    }
}
