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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using FGEGraphics.UISystem.Elements;
using FGEGraphics.UISystem;

namespace FGEGraphics.ClientSystem;

/// <summary>A 2D UI view.</summary>
public class ViewUI2D
{
    /// <summary>The base text styling for debug information.</summary>
    public static readonly string DEBUG_BASE_COLOR = "^r^0^h^o^e";

    /// <summary>The backing client window.</summary>
    public GameClientWindow Client;

    /// <summary>The render context (2D) for the UI.</summary>
    public RenderContext2D UIContext = new();

    /// <summary>Gets the primary engine.</summary>
    public GameEngineBase Engine => Client.CurrentEngine;

    /// <summary>Gets the rendering helper for the engine.</summary>
    public Renderer2D Rendering => Client.Rendering2D;

    /// <summary>The default basic UI screen.</summary>
    public UIScreen DefaultScreen;

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

    /// <summary>Whether this UI is displayed directly onto the screen (as opposed to a temporary GL buffer).</summary>
    public bool DirectToScreen = true;

    // TODO: move these somewhere else?
    /// <summary>Whether the mouse left button is currently down.</summary>
    public bool MouseDown;

    /// <summary>Whether the mouse left button was previously down.</summary>
    public bool MousePreviouslyDown;

    /// <summary>The UI element currently being pressed and held.</summary>
    public UIElement HeldElement;

    /// <summary>Whether this UI view is in 'debug' mode.</summary>
    public bool IsDebug;

    /// <summary>Data internal to a <see cref="ViewUI2D"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The current main screen.</summary>
        public UIScreen CurrentScreen;

        /// <summary>Whether scroll input is still available to consume for the current step.</summary>
        public bool Scrolled;
    }

    /// <summary>Data internal to a <see cref="ViewUI2D"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Constructs the view.</summary>
    /// <param name="client">Backing client window.</param>
    public ViewUI2D(GameClientWindow client)
    {
        Client = client;
        DefaultScreen = new UIScreen(this);
        CurrentScreen = DefaultScreen;
        Client.Window.KeyDown += HandleKeyEvent;
    }

    /// <summary>Draws wireframe outlines of the elements on-screen for debugging.</summary>
    public void DrawDebugOutlines()
    {
        foreach (UIElement element in CurrentScreen.AllChildren())
        {
            Engine.Textures.White.Bind();
            Color4F outlineColor = element == HeldElement ? Color4F.Green : element.ElementInternal.IsMouseHovered ? Color4F.Yellow : Color4F.Red;
            Renderer2D.SetColor(outlineColor);
            Rendering.RenderRectangle(UIContext, element.X + 1, element.Y + 1, element.X + element.Width - 1, element.Y + element.Height - 1, new(-0.5f, -0.5f, element.Rotation), true);
            // TODO: rendering stroke thickness
            if (element == HeldElement)
            {
                Rendering.RenderRectangle(UIContext, element.X, element.Y, element.X + element.Width, element.Y + element.Height, new(-0.5f, -0.5f, element.Rotation), true);
                Rendering.RenderRectangle(UIContext, element.X + 2, element.Y + 2, element.X + element.Width - 2, element.Y + element.Height - 2, new(-0.5f, -0.5f, element.Rotation), true);
            }
            Renderer2D.SetColor(Color4F.White);
        }
    }

    /// <summary>Draw the menu to the relevant back buffer.</summary>
    public void Draw()
    {
        using var _push = StackNoteHelper.UsePush("Draw ViewUI2D", this);
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
        Client.Shaders.ColorMult2D.Bind();
        Renderer2D.SetColor(Color4F.White);
        GraphicsUtil.CheckError("ViewUI2D - Draw - SetColor");
        ManagedShader2D.CurrentScaler.Set(UIContext.Scaler.X, UIContext.Scaler.Y, UIContext.AspectHelper);
        GraphicsUtil.CheckError("ViewUI2D - Draw - SetScaler");
        ManagedShader2D.CurrentAdder.Set(UIContext.Adder);
        GraphicsUtil.CheckError("ViewUI2D - Draw - SetAdder");
        GL.Disable(EnableCap.DepthTest);
        GraphicsUtil.CheckError("ViewUI2D - Draw - DisableDepth");
        Shader s = Client.FontSets.FixToShader;
        Client.FontSets.FixToShader = Client.Shaders.ColorMult2D.UnderlyingShader;
        GraphicsUtil.CheckError("ViewUI2D - Draw - PreUpdate");
        foreach (UIElement element in CurrentScreen.AllChildren())
        {
            element.UpdateStyle();
        }
        for (int i = 0; i < 16; i++) // TODO: this seems reckless
        {
            foreach (UIElement element in CurrentScreen.AllChildren(filter: element => element.TransformSelf))
            {
                element.UpdateTransforms(Client.Delta, Vector3.Zero);
            }
            bool anyUpdated = false;
            foreach (UIElement element in CurrentScreen.AllChildren())
            {
                anyUpdated |= element.HandleTransforms();
            }
            if (!anyUpdated)
            {
                break;
            }
            if (i == 15)
            {
                Logs.Warning("WARNING WARNING PASSED 16 TIMES!!!");
            }
        }
        foreach (UIElement element in CurrentScreen.AllChildren())
        {
            if (!element.ElementInternal.HasDrawn)
            {
                element.Init();
                element.ElementInternal.HasDrawn = true;
            }
        }
        GraphicsUtil.CheckError("ViewUI2D - Draw - PreDraw");
        CurrentScreen.RenderAll(Client.Delta);
        if (IsDebug)
        {
            DrawDebugOutlines();
            /*if (HeldElement is not null)
            {
                UIDebugPanel panel = new(HeldElement, UIDebugPanel.Styling, new UILayout().SetSize(300, 200));
                foreach (var child in panel.AllChildren())
                {
                    child.View = this;
                }
                panel.RenderAll(0.0);
            }*/
        }
        GraphicsUtil.CheckError("ViewUI2D - Draw - PostDraw");
        Client.FontSets.FixToShader = s;
        if (UIContext.ScissorStack.Count > 0)
        {
            Logs.Warning($"Scissor stack not empty at end of UI draw: {UIContext.ScissorStack.Count} unpopped scissors");
            UIContext.ScissorStack.Clear();
        }
    }

    /// <summary>Ticks all elements attached to this view.</summary>
    public void Tick()
    {
        int mouseX = (int)Client.MouseX;
        int mouseY = (int)Client.MouseY;
        Vector2 scrollDelta = Client.CurrentMouse.ScrollDelta;
        MouseDown = Client.CurrentMouse.IsButtonDown(MouseButton.Left);
        CurrentScreen.TickAll(Client.Delta);
        // TODO: crude and probably slow
        ICollection<UIElement> elements = [.. CurrentScreen.AllChildren()];
        foreach (UIElement element in elements.Reverse())
        {
            if (element.IsValid)
            {
                element.TickInteraction(mouseX, mouseY, scrollDelta);
            }
        }
        MousePreviouslyDown = MouseDown;
        Internal.Scrolled = false;
    }

    /// <summary>Listens for debug keybinds and updates <see cref="Internal"/> accordingly.</summary>
    public void HandleKeyEvent(KeyboardKeyEventArgs args)
    {
        if (args.Key == Keys.F4)
        {
            IsDebug = !IsDebug;
        }
    }
}
