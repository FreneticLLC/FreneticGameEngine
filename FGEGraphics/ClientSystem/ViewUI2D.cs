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
using FreneticUtilities.FreneticExtensions;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.StackNoteSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.UISystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.ClientSystem;

/// <summary>A 2D UI view.</summary>
public class ViewUI2D
{
    /// <summary>The backing client window.</summary>
    public GameClientWindow Client;

    /// <summary>Gets the primary engine.</summary>
    public GameEngineBase Engine => Client.CurrentEngine;

    /// <summary>Gets the rendering helper for the engine.</summary>
    public Renderer2D Rendering => Client.Rendering2D;

    /// <summary>The default basic UI screen.</summary>
    public UIScreen DefaultScreen;

    // TODO: move these somewhere else?
    /// <summary>Whether the mouse left button is currently down.</summary>
    public bool MouseDown;

    /// <summary>Whether the mouse left button was previously down.</summary>
    public bool MousePreviouslyDown;

    /// <summary>Data internal to a <see cref="ViewUI2D"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The current main screen.</summary>
        public UIScreen CurrentScreen;

        /// <summary>Debug info about hovered UI elements.</summary>
        public List<string> DebugInfo = [];

        /// <summary>The stack of elements that were rendered.</summary>
        public List<UIElement> RenderStack = [];

        public bool Scrolled;
    }

    /// <summary>Data internal to a <see cref="ViewUI2D"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Constructs the view.</summary>
    /// <param name="gameClient">Backing client window.</param>
    public ViewUI2D(GameClientWindow gameClient)
    {
        Client = gameClient;
        UIContext = new RenderContext2D();
        DefaultScreen = new UIScreen(this);
        CurrentScreen = DefaultScreen;
    }

    /// <summary>Whether this UI view is in 'debug' mode.</summary>
    public bool Debug;

    /// <summary>The UI element currently being pressed and held.</summary>
    public UIElement HeldElement;

    /// <summary>Gets or sets the current main screen.</summary>
    public UIScreen CurrentScreen
    {
        get => Internal.CurrentScreen;
        set
        {
            if (value != Internal.CurrentScreen)
            {
                Internal.CurrentScreen?.SwitchFrom();
                Internal.CurrentScreen = value;
                Internal.CurrentScreen?.SwitchTo();
            }
        }
    }

    /// <summary>The render context (2D) for the UI.</summary>
    public RenderContext2D UIContext;

    /// <summary>Whether this UI is displayed directly onto the screen (as opposed to a temporary GL buffer).</summary>
    public bool DirectToScreen = true;

    /// <summary>Used for <see cref="UIAnchor.RELATIVE"/>.</summary>
    public int RelativeYLast = 0;

    /// <summary>Draw the menu to the relevant back buffer.</summary>
    // TODO: Clean this up
    public void Draw()
    {
        StackNoteHelper.Push("Draw ViewUI2D", this);
        try
        {
            GraphicsUtil.CheckError("ViewUI2D - Draw - Pre");
            if (DirectToScreen)
            {
                UIContext.ZoomMultiplier = Client.Window.ClientSize.X * 0.5f;
                UIContext.Width = Client.Window.ClientSize.X;
                UIContext.Height = Client.Window.ClientSize.Y;
                float aspect = UIContext.Width / (float)UIContext.Height;
                float sc = 1.0f / (UIContext.Zoom * UIContext.ZoomMultiplier);
                UIContext.Scaler = new Vector2(sc, -sc * aspect);
                UIContext.ViewCenter = new Vector2(-Client.Window.ClientSize.X * 0.5f, -Client.Window.ClientSize.Y * 0.5f);
                UIContext.Adder = UIContext.ViewCenter;
                UIContext.AspectHelper = UIContext.Width / (float)UIContext.Height;
                Client.Ortho = Matrix4.CreateOrthographicOffCenter(0, Client.Window.ClientSize.X, Client.Window.ClientSize.Y, 0, -1, 1);
                GL.Viewport(0, 0, UIContext.Width, UIContext.Height);
                GraphicsUtil.CheckError("ViewUI2D - Draw - DirectToScreenPost");
            }
            // TODO: alternate Ortho setting from scaler/adder def!
            Client.Shaders.ColorMult2DShader.Bind();
            Renderer2D.SetColor(Color4F.White);
            GraphicsUtil.CheckError("ViewUI2D - Draw - SetColor");
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(UIContext.Scaler.X, UIContext.Scaler.Y, UIContext.AspectHelper));
            GraphicsUtil.CheckError("ViewUI2D - Draw - SetScaler");
            ShaderLocations.Common2D.ADDER.Set(UIContext.Adder);
            GraphicsUtil.CheckError("ViewUI2D - Draw - SetAdder");
            GL.Disable(EnableCap.DepthTest);
            GraphicsUtil.CheckError("ViewUI2D - Draw - DisableDepth");
            Shader s = Client.FontSets.FixToShader;
            Client.FontSets.FixToShader = Client.Shaders.ColorMult2DShader;
            GraphicsUtil.CheckError("ViewUI2D - Draw - PreUpdate");
            Internal.RenderStack.Clear();
            RelativeYLast = 0;
            foreach (UIElement element in CurrentScreen.AllChildren())
            {
                if (element.IsValid)
                {
                    element.UpdatePosition(Client.Delta, Vector3.Zero);
                }
            }
            GraphicsUtil.CheckError("ViewUI2D - Draw - PreDraw");
            foreach (UIElement elem in Internal.RenderStack)
            {
                if (elem.IsValid)
                {
                    elem.UpdateStyle();
                }
            }
            foreach (UIElement elem in (IEnumerable<UIElement>)Internal.RenderStack)
            {
                StackNoteHelper.Push("Draw UI Element", elem);
                try
                {
                    if (!elem.IsValid)
                    {
                        continue;
                    }
                    if (elem.ShouldRender)
                    {
                        elem.Render(this, Client.Delta);
                    }
                    if (Debug)
                    {
                        Engine.Textures.White.Bind();
                        Color4F outlineColor = elem == HeldElement ? Color4F.Green : elem.ElementInternal.HoverInternal ? Color4F.Yellow : Color4F.Red;
                        Renderer2D.SetColor(outlineColor);
                        Rendering.RenderRectangle(UIContext, elem.X, elem.Y, elem.X + elem.Width, elem.Y + elem.Height, new(-0.5f, -0.5f, elem.LastAbsoluteRotation), true);
                        Renderer2D.SetColor(Color4F.White);
                        if (elem.ElementInternal.HoverInternal)
                        {
                            Internal.DebugInfo.Add(elem.GetDebugInfo().JoinString("\n"));
                        }
                    }
                }
                finally
                {
                    StackNoteHelper.Pop();
                }
            }
            if (Debug)
            {
                string content = Internal.DebugInfo.JoinString("\n\n");
                RenderableText text = Client.FontSets.Standard.ParseFancyText(content, "^r^0^e^7");
                // TODO: This should be in a generic 'tooltip' system somewhere. And also account for RTL text.
                float x = Client.MouseX + text.Width < Client.WindowWidth
                    ? Client.MouseX + 10
                    : Client.MouseX - text.Width - 10;
                float textHeight = text.Lines.Length * Client.FontSets.Standard.Height;
                float y = Client.MouseY + textHeight < Client.WindowHeight
                    ? Client.MouseY + 20
                    : Client.MouseY - textHeight - 20;
                Client.FontSets.Standard.DrawFancyText(text, new((int)x, (int)y, 0));
            }
            GraphicsUtil.CheckError("ViewUI2D - Draw - PostDraw");
            Client.FontSets.FixToShader = s;
            Internal.DebugInfo.Clear();
        }
        finally
        {
            StackNoteHelper.Pop();
        }
    }

    /// <summary>Ticks all elements attached to this view.</summary>
    public void Tick()
    {
        int mouseX = (int)Client.MouseX;
        int mouseY = (int)Client.MouseY;
        Vector2 scrollDelta = Client.CurrentMouse.ScrollDelta;
        MouseDown = Client.CurrentMouse.IsButtonDown(MouseButton.Left);
        CurrentScreen.FullTick(Client.Delta);
        Internal.RenderStack.Reverse();
        foreach (UIElement elem in Internal.RenderStack)
        {
            if (elem.IsValid)
            {
                elem.TickInteraction(mouseX, mouseY, scrollDelta);
            } 
        }
        MousePreviouslyDown = MouseDown;
        Internal.Scrolled = false;
    }
}
