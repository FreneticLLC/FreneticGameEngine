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
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
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

    /// <summary>Data internal to a <see cref="ViewUI2D"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The current main screen.</summary>
        public UIScreen CurrentScreen;

        /// <summary>Debug info about hovered UI elements.</summary>
        public List<string> DebugInfo = [];
    }

    /// <summary>Data internal to a <see cref="ViewUI2D"/> instance.</summary>
    public InternalData Internal;

    /// <summary>Constructs the view.</summary>
    /// <param name="gameClient">Backing client window.</param>
    public ViewUI2D(GameClientWindow gameClient)
    {
        Client = gameClient;
        UIContext = new RenderContext2D();
        DefaultScreen = new UIScreen(this);
        CurrentScreen = DefaultScreen;
        Internal = new();
    }

    /// <summary>Whether this UI view is in 'debug' mode.</summary>
    public bool Debug;

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
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(UIContext.Scaler.X, UIContext.Scaler.Y, UIContext.AspectHelper));
            GL.Uniform2(ShaderLocations.Common2D.ADDER, ref UIContext.Adder);
            GL.Disable(EnableCap.DepthTest);
            Shader s = Client.FontSets.FixToShader;
            Client.FontSets.FixToShader = Client.Shaders.ColorMult2DShader;
            GraphicsUtil.CheckError("ViewUI2D - Draw - PreUpdate");
            LastRenderedSet.Clear();
            RelativeYLast = 0;
            CurrentScreen.UpdatePositions(LastRenderedSet, Client.Delta, 0, 0, Vector3.Zero);
            GraphicsUtil.CheckError("ViewUI2D - Draw - PreDraw");
            foreach (UIElement elem in LastRenderedSet)
            {
                if (elem.IsValid)
                {
                    elem.UpdateStyle();
                }
            }
            foreach (UIElement elem in (SortToPriority ? LastRenderedSet.OrderBy((e) => e.RenderPriority) : (IEnumerable<UIElement>)LastRenderedSet))
            {
                StackNoteHelper.Push("Draw UI Element", elem);
                try
                {
                    if (elem.IsValid && elem.ShouldRender)
                    {
                        elem.Render(this, Client.Delta, elem.ElementInternal.CurrentStyle);
                    }
                    if (Debug)
                    {
                        Engine.Textures.White.Bind();
                        Renderer2D.SetColor(Color4F.Red);

                        // TODO: Not this!
                        Rendering.RenderRectangle(UIContext, elem.X, elem.Y, elem.X + 1, elem.Y + elem.Height, new(-0.5f, -0.5f, elem.LastAbsoluteRotation));
                        Rendering.RenderRectangle(UIContext, elem.X, elem.Y + elem.Height, elem.X + elem.Width, elem.Y + elem.Height + 1, new(-0.5f, -0.5f, elem.LastAbsoluteRotation));
                        Rendering.RenderRectangle(UIContext, elem.X + elem.Width, elem.Y, elem.X + elem.Width + 1, elem.Y + elem.Height, new(-0.5f, -0.5f, elem.LastAbsoluteRotation));
                        Rendering.RenderRectangle(UIContext, elem.X, elem.Y, elem.X + elem.Width, elem.Y + 1, new(-0.5f, -0.5f, elem.LastAbsoluteRotation));

                        /*Rendering.RenderLine((elem.X, elem.Y), (elem.X, elem.Y + elem.Height));
                        Rendering.RenderLine((elem.X, elem.Y + elem.Height), (elem.X + elem.Width, elem.Y + elem.Height));
                        Rendering.RenderLine((elem.X + elem.Width, elem.Y + elem.Height), (elem.X + elem.Width, elem.Y));
                        Rendering.RenderLine((elem.X + elem.Width, elem.Y), (elem.X, elem.Y));*/
                        Renderer2D.SetColor(Color4F.White);

                        if (elem.Hovered)
                        {
                            string name = $"^5^u{elem.GetType()}";
                            string info = $"^r^0^e^7Position: ({elem.X}, {elem.Y}) ^&| ^7Dimensions: ({elem.Width}w, {elem.Height}h) ^&| ^7Rotation: {elem.LastAbsoluteRotation}";
                            Internal.DebugInfo.Add($"{name}\n{info}");
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
                string content = string.Join("\n\n", Internal.DebugInfo);
                RenderableText text = Client.FontSets.Standard.ParseFancyText(content, "^r^0^e^7");
                float x = Client.MouseX + text.Width < Client.WindowWidth
                    ? Client.MouseX + 10.0f
                    : Client.MouseX - text.Width - 10.0f;
                Client.FontSets.Standard.DrawFancyText(text, new((int) x, Client.MouseY + 20, 0));
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

    /// <summary>Whether to sort the view by priority order (if not, will be parent/child logical order).</summary>
    public bool SortToPriority = false;

    /// <summary>The last set of elements that were rendered (not sorted).</summary>
    public List<UIElement> LastRenderedSet = [];

    /// <summary>Ticks all elements attached to this view.</summary>
    public void Tick()
    {
        CurrentScreen.FullTick(Client.Delta);
    }
}
