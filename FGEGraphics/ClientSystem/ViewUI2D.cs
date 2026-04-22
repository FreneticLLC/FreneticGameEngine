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
using FreneticUtilities.FreneticExtensions;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;

namespace FGEGraphics.ClientSystem;

// toggle debug mode: f4
// cycle thru hover info: alt kp5
// scroll tree: alt kp2, kp8
// expand/contract tree: shift scroll
// scroll view modes: alt kp4, kp6

/// <summary>A 2D UI view.</summary>
public class ViewUI2D
{
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

        public Stack<UIElement> DebugHoveredElements = [];

        /// <summary>Whether to draw textual debug information about the hovered elements, if any.</summary>
        public bool ShowDebugInfo = true;

        /// <summary>Whether to draw the debug information at the cursor position.</summary>
        public bool ShowDetailedDebugInfo = false;

        /// <summary>The list position of the entry at the top of the debug info tree.</summary>
        public int DebugInfoStartIndex = 0;

        /// <summary>The number of entries displayed in the debug info tree.</summary>
        public int DebugInfoEntries = 5;

        /// <summary>The total size of the debug info tree.</summary>
        public int DebugInfoTreeSize;
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

    /// <summary>Draws debug information about the hovered elements, if any.</summary>
    public void DrawDebugInfoTree(Stack<(int, IEnumerable<string>)> infoStack)
    {
        Range infoRange = new(Internal.DebugInfoStartIndex, Math.Min(Internal.DebugInfoStartIndex + Internal.DebugInfoEntries, infoStack.Count));
        int numberInfoEntries = infoRange.End.Value - infoRange.Start.Value;
        if (numberInfoEntries <= 0)
        {
            return;
        }
        IEnumerable<(int, IEnumerable<string>)> infoChunk = infoStack.Take(infoRange);
        int minimumTreeLevel = infoChunk.Min(entry => entry.Item1);
        string debugInfo = infoChunk.Select(entry =>
            {
                string prefix = new(' ', (entry.Item1 - minimumTreeLevel) * 2);
                return entry.Item2.Select(line => $"{prefix}{line}").JoinString("\n");
            })
            .JoinString("\n\n");
        debugInfo += $"\n\n{DEBUG_BASE_COLOR}^&[{infoRange.Start}] - ^3[{numberInfoEntries}] ^&- [{infoStack.Count - infoRange.End.Value}]^r";
        RenderableText text = Client.FontSets.Standard.ParseFancyText(debugInfo, "^r^0^e^7");
        Client.FontSets.Standard.DrawFancyText(text, new Location(10, (int)(Client.WindowHeight - text.Height - 10), 0));
    }

    public void DrawDebug()
    {
        List<UIElement> hoveredElements = [];
        foreach (UIElement element in CurrentScreen.AllChildren())
        {
            if (element.ElementInternal.IsMouseHovered && element.AllowDebug)
            {
                hoveredElements.Add(element);
            }
        }
        hoveredElements.Reverse();
        Internal.DebugInfoTreeSize = hoveredElements.Count;
        Internal.DebugInfoStartIndex = Math.Max(0, Math.Min(Internal.DebugInfoStartIndex, Internal.DebugInfoTreeSize - 1));
        DrawDebugOutlines();
        if (Internal.ShowDebugInfo && hoveredElements.Count > 0)
        {
            if (Internal.ShowDetailedDebugInfo)
            {
                UIElement element = hoveredElements.ElementAt(Internal.DebugInfoStartIndex);
                RenderableText text = Client.FontSets.Standard.ParseFancyText(element.GetAllDebugInfo(DEBUG_BASE_COLOR));
                Client.FontSets.Standard.DrawFancyText(text, new Location(10, (int)(Client.WindowHeight - text.Height - 10), 0));
            }
            else
            {
                Range infoRange = new(Internal.DebugInfoStartIndex, Math.Min(Internal.DebugInfoStartIndex + Internal.DebugInfoEntries, hoveredElements.Count));
                int numberInfoEntries = infoRange.End.Value - infoRange.Start.Value;
                if (numberInfoEntries > 0)
                {
                    IEnumerable<UIElement> chunk = hoveredElements.Take(infoRange);
                    int minimumTreeLevel = chunk.Min(element => element.ElementInternal.TreeLevel);
                    string debugInfo = chunk.Select(element =>
                    {
                        string spacing = new(' ', (element.ElementInternal.TreeLevel - minimumTreeLevel) * 2);
                        return element.GetBaseDebugInfo(DEBUG_BASE_COLOR, detailed: false).Select(line => $"^r{spacing}{DEBUG_BASE_COLOR}{line}").JoinString("\n");
                    })
                    .JoinString("\n\n");
                    debugInfo += $"\n\n{DEBUG_BASE_COLOR}^&[{infoRange.Start}] - ^3[{numberInfoEntries}] ^&- [{hoveredElements.Count - infoRange.End.Value}]^r";
                    RenderableText text = Client.FontSets.Standard.ParseFancyText(debugInfo, "^r^0^e^7");
                    Client.FontSets.Standard.DrawFancyText(text, new Location(10, (int)(Client.WindowHeight - text.Height - 10), 0));
                }
            }
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
        while (true) // TODO: this seems reckless
        {
            foreach (UIElement element in CurrentScreen.AllChildren())
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
            DrawDebug();
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
        if (args.Alt)
        {
            if (args.Shift)
            {
                if (args.Key == Keys.KeyPad5)
                {
                    Internal.ShowDetailedDebugInfo = !Internal.ShowDetailedDebugInfo;
                }
                if (args.Key == Keys.KeyPad2 && Internal.DebugInfoEntries > 1)
                {
                    Internal.DebugInfoEntries--;
                }
                else if (args.Key == Keys.KeyPad8 && Internal.DebugInfoEntries < Internal.DebugInfoTreeSize)
                {
                    Internal.DebugInfoEntries++;
                }
            }
            else
            {
                if (args.Key == Keys.KeyPad5)
                {
                    Internal.ShowDebugInfo = !Internal.ShowDebugInfo;
                }
                else if (args.Key == Keys.KeyPad2 && Internal.DebugInfoStartIndex < Internal.DebugInfoTreeSize - 1)
                {
                    Internal.DebugInfoStartIndex++;
                }
                else if (args.Key == Keys.KeyPad8 && Internal.DebugInfoStartIndex > 0)
                {
                    Internal.DebugInfoStartIndex--;
                }
            }
        }
    }
}
