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
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.UISystem.InputSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem
{
    /// <summary>Represents an interactable text input box on a screen.</summary>
    public class UIInputBox : UIElement
    {
        /// <summary>The current text in this input box.</summary>
        public string Text;

        /// <summary>Information about this input box.</summary>
        public string Info;

        /// <summary>The font to use.</summary>
        public FontSet Fonts;

        /// <summary>Whether this input box is currently selected.</summary>
        public bool Selected = false;

        /// <summary>Whether this input box is multi-line.</summary>
        public bool MultiLine = false;

        /// <summary>The current minimum position of the cursor.</summary>
        public int MinCursor = 0;

        /// <summary>The current maximum position of the cursor.</summary>
        public int MaxCursor = 0;

        /// <summary>Whether the user tried to press the escape key.</summary>
        public bool TriedToEscape = false;

        /// <summary>Fired when the text in this input box is modified.</summary>
        public EventHandler<EventArgs> TextModified;

        /// <summary>Fired when the enter key is pressed while this input box is selected.</summary>
        public Action EnterPressed;

        /// <summary>The color of this input box.</summary>
        public Vector4 Color = Vector4.One;

        /// <summary>Constructs a new text input box.</summary>
        /// <param name="text">The default text in the box.</param>
        /// <param name="info">Information about the box.</param>
        /// <param name="fonts">The font to use.</param>
        /// <param name="pos">The position of the element.</param>
        public UIInputBox(string text, string info, FontSet fonts, UIPositionHelper pos)
            : base(pos.Height <= 0 ? pos.ConstantHeight((int)fonts.FontDefault.Height) : pos)
        {
            Text = text;
            Info = info;
            Fonts = fonts;
        }

        /// <summary>Internal values for <see cref="UIInputBox"/>.</summary>
        public struct InternalData
        {
            /// <summary>Whether the mouse left button is currently down.</summary>
            public bool MDown;

            /// <summary>The starting index of a mouse multi-select in-progress.</summary>
            public int MStart;
        }

        /// <summary>Internal values for <see cref="UIInputBox"/>.</summary>
        public InternalData Internal;

        /// <summary>Selects this input box.</summary>
        public override void MouseLeftDown()
        {
            Internal.MDown = true;
            Selected = true;
            // TODO: implement
            // /* KeyHandlerState khs = */KeyHandler.GetKBState();
            int xs = LastAbsolutePosition.X;
            for (int i = 0; i < Text.Length; i++)
            {
                if (xs + Fonts.MeasureFancyText(Text.Substring(0, i)) > Window.MouseX)
                {
                    MinCursor = i;
                    MaxCursor = i;
                    Internal.MStart = i;
                    return;
                }
            }
            MinCursor = Text.Length;
            MaxCursor = Text.Length;
            Internal.MStart = Text.Length;
        }

        /// <summary>Clears this input box.</summary>
        public void Clear()
        {
            Text = "";
            MinCursor = 0;
            MaxCursor = 0;
            TriedToEscape = false;
        }

        /// <summary>Deselects this input box.</summary>
        public override void MouseLeftDownOutside()
        {
            Selected = false;
        }

        /// <summary>Sets the new cursor position.</summary>
        public override void MouseLeftUp()
        {
            AdjustMax();
            Internal.MDown = false;
        }

        /// <summary>Adjusts the cursor position based on the mouse X coordinate.</summary>
        public void AdjustMax()
        {
            int xs = LastAbsolutePosition.X;
            for (int i = 0; i < Text.Length; i++)
            {
                if (xs + Fonts.MeasureFancyText(Text.Substring(0, i)) > Window.MouseX)
                {
                    MinCursor = Math.Min(i, Internal.MStart);
                    MaxCursor = Math.Max(i, Internal.MStart);
                    return;
                }
            }
            MaxCursor = Text.Length;
        }

        /// <summary>Performs a tick on this element.</summary>
        /// <param name="delta">The time since the last tick.</param>
        public override void Tick(double delta)
        {
            if (Internal.MDown)
            {
                AdjustMax();
            }
            if (Selected)
            {
                if (MinCursor > MaxCursor)
                {
                    int min = MinCursor;
                    MinCursor = MaxCursor;
                    MaxCursor = min;
                }
                bool modified = false;
                KeyHandlerState khs = Window.Keyboard.BuildingState;
                if (khs.Escaped)
                {
                    TriedToEscape = true;
                }
                if (khs.InitBS > 0)
                {
                    int end;
                    if (MaxCursor > MinCursor)
                    {
                        khs.InitBS--;
                    }
                    if (khs.InitBS > 0)
                    {
                        end = MinCursor - Math.Min(khs.InitBS, MinCursor);
                    }
                    else
                    {
                        end = MinCursor;
                    }
                    Text = Text.Substring(0, end) + Text[MaxCursor..];
                    MinCursor = end;
                    MaxCursor = end;
                    modified = true;
                }
                if (khs.KeyboardString.Length > 0)
                {
                    Text = Text.Substring(0, MinCursor) + khs.KeyboardString + Text[MaxCursor..];
                    MinCursor += khs.KeyboardString.Length;
                    MaxCursor = MinCursor;
                    modified = true;
                }
                if (!MultiLine && Text.Contains('\n'))
                {
                    Text = Text.Substring(0, Text.IndexOf('\n'));
                    if (MaxCursor > Text.Length)
                    {
                        MaxCursor = Text.Length;
                        if (MinCursor > MaxCursor)
                        {
                            MinCursor = MaxCursor;
                        }
                    }
                    modified = true;
                    EnterPressed?.Invoke();
                }
                if (modified && TextModified != null)
                {
                    TextModified.Invoke(this, null);
                }
            }
        }

        /// <summary>Renders this input box on the screen.</summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            string typed = Text;
            int c = 0;
            int cmax = 0;
            GameEngineBase engine = Engine;
            if (!/*engine.CVars.u_colortyping.ValueB*/false) // TODO: Color Typing option!
            {
                for (int i = 0; i < typed.Length && i < MinCursor; i++)
                {
                    if (typed[i] == '^')
                    {
                        c++;
                    }
                }
                for (int i = 0; i < typed.Length && i < MaxCursor; i++)
                {
                    if (typed[i] == '^')
                    {
                        cmax++;
                    }
                }
                typed = typed.Replace("^", "^^n");
            }
            int x = LastAbsolutePosition.X;
            int y = LastAbsolutePosition.Y;
            int w = LastAbsoluteSize.X;
            engine.Textures.White.Bind();
            Renderer2D.SetColor(Color);
            view.Rendering.RenderRectangle(view.UIContext, x - 1, y - 1, x + w + 1, y + Fonts.FontDefault.Height + 1, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, engine.Window.Size.Y - (y + (int)Fonts.FontDefault.Height), w, (int)Fonts.FontDefault.Height);
            if (Selected)
            {
                float textw = Fonts.MeasureFancyText(typed.Substring(0, MinCursor + c));
                float textw2 = Fonts.MeasureFancyText(typed.Substring(0, MaxCursor + cmax));
                Renderer2D.SetColor(new Color4(0f, 0.2f, 1f, 0.5f));
                view.Rendering.RenderRectangle(view.UIContext, x + textw, y, x + textw2 + 1, y + Fonts.FontDefault.Height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
            }
            Renderer2D.SetColor(Color4.White);
            Fonts.DrawFancyText((typed.Length == 0 ? ("^)^i" + Info) : ("^0" + typed)), new Location(x, y, 0));
            GL.Scissor(0, 0, engine.Window.Size.X, engine.Window.Size.Y); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }
    }
}
