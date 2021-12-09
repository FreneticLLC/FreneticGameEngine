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
using FGECore.CoreSystems;
using FGEGraphics.ClientSystem;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem.InputSystems
{
    /// <summary>Helper class for handling keyboard input.</summary>
    public class KeyHandler
    {
        /// <summary>
        /// Fake <see cref="Keys"/> values used for various mouse buttons and actions.
        /// Values are well outside the range of default Keys values.
        /// </summary>
        public static Keys
            KEY_MOUSE_LEFT_BUTTON = (Keys)1001, KEY_MOUSE_RIGHT_BUTTON = (Keys)1002, KEY_MOUSE_MIDDLE_BUTTON = (Keys)1003,
             KEY_MOUSE_BUTTON_4 = (Keys)1004, KEY_MOUSE_BUTTON_5 = (Keys)1005,
            KEY_MOUSE_WHEEL_DOWN = (Keys)1501, KEY_MOUSE_WHEEL_UP = (Keys)1502;

        /// <summary>Map of key names to key enum value.</summary>
        public static Dictionary<string, Keys> NamesToKeys;

        /// <summary>Map of key enum value to key name.</summary>
        public static Dictionary<Keys, string> KeysToNames;

        /// <summary>Helper for static init.</summary>
        static void RegKey(string name, Keys key)
        {
            NamesToKeys.Add(name, key);
            KeysToNames.Add(key, name);
        }

        /// <summary>Builds key map data.</summary>
        static KeyHandler()
        {
            NamesToKeys = new Dictionary<string, Keys>();
            KeysToNames = new Dictionary<Keys, string>();
            RegKey("a", Keys.A); RegKey("b", Keys.B); RegKey("c", Keys.C);
            RegKey("d", Keys.D); RegKey("e", Keys.E); RegKey("f", Keys.F);
            RegKey("g", Keys.G); RegKey("h", Keys.H); RegKey("i", Keys.I);
            RegKey("j", Keys.J); RegKey("k", Keys.K); RegKey("l", Keys.L);
            RegKey("m", Keys.M); RegKey("n", Keys.N); RegKey("o", Keys.O);
            RegKey("p", Keys.P); RegKey("q", Keys.Q); RegKey("r", Keys.R);
            RegKey("s", Keys.S); RegKey("t", Keys.T); RegKey("u", Keys.U);
            RegKey("v", Keys.V); RegKey("w", Keys.W); RegKey("x", Keys.X);
            RegKey("y", Keys.Y); RegKey("z", Keys.Z); RegKey("1", Keys.D1);
            RegKey("2", Keys.D2); RegKey("3", Keys.D3); RegKey("4", Keys.D4);
            RegKey("5", Keys.D5); RegKey("6", Keys.D6); RegKey("7", Keys.D7);
            RegKey("8", Keys.D8); RegKey("9", Keys.D9); RegKey("0", Keys.D0);
            RegKey("lalt", Keys.LeftAlt); RegKey("ralt", Keys.RightAlt);
            RegKey("f1", Keys.F1); RegKey("f2", Keys.F2); RegKey("f3", Keys.F3);
            RegKey("f4", Keys.F4); RegKey("f5", Keys.F5); RegKey("f6", Keys.F6);
            RegKey("f7", Keys.F7); RegKey("f8", Keys.F8); RegKey("f9", Keys.F9);
            RegKey("f10", Keys.F10); RegKey("f11", Keys.F11); RegKey("f12", Keys.F12);
            RegKey("enter", Keys.Enter); RegKey("end", Keys.End); RegKey("home", Keys.Home);
            RegKey("insert", Keys.Insert); RegKey("delete", Keys.Delete); RegKey("pause", Keys.Pause);
            RegKey("lshift", Keys.LeftShift); RegKey("rshift", Keys.RightShift); RegKey("tab", Keys.Tab);
            RegKey("caps", Keys.CapsLock); RegKey("lctrl", Keys.LeftControl); RegKey("rctrl", Keys.RightControl);
            RegKey("comma", Keys.Comma); RegKey("dot", Keys.Period); RegKey("slash", Keys.Slash);
            RegKey("backslash", Keys.Backslash); RegKey("dash", Keys.Minus); RegKey("equals", Keys.Equal);
            RegKey("backspace", Keys.Backspace); RegKey("semicolon", Keys.Semicolon); RegKey("quote", Keys.Apostrophe);
            RegKey("lbracket", Keys.LeftBracket); RegKey("rbracket", Keys.RightBracket); RegKey("kp1", Keys.KeyPad1);
            RegKey("kp2", Keys.KeyPad2); RegKey("kp3", Keys.KeyPad3); RegKey("kp4", Keys.KeyPad4);
            RegKey("kp5", Keys.KeyPad5); RegKey("kp6", Keys.KeyPad6); RegKey("kp7", Keys.KeyPad7);
            RegKey("kp8", Keys.KeyPad8); RegKey("kp9", Keys.KeyPad9); RegKey("kp0", Keys.KeyPad0);
            RegKey("kpenter", Keys.KeyPadEnter); RegKey("kpmultiply", Keys.KeyPadMultiply);
            RegKey("kpadd", Keys.KeyPadAdd); RegKey("kpsubtract", Keys.KeyPadSubtract);
            RegKey("kpdivide", Keys.KeyPadDivide); RegKey("kpperiod", Keys.KeyPadDecimal);
            RegKey("space", Keys.Space); RegKey("escape", Keys.Escape);
            RegKey("pageup", Keys.PageUp); RegKey("pagedown", Keys.PageDown);
            RegKey("left", Keys.Left); RegKey("right", Keys.Right);
            RegKey("up", Keys.Up); RegKey("down", Keys.Down);
            RegKey("mouse1", KEY_MOUSE_LEFT_BUTTON);
            RegKey("mouse2", KEY_MOUSE_RIGHT_BUTTON);
            RegKey("mouse3", KEY_MOUSE_MIDDLE_BUTTON);
            RegKey("mousewheelup", KEY_MOUSE_WHEEL_UP);
            RegKey("mousewheeldown", KEY_MOUSE_WHEEL_DOWN);
            RegKey("mouse4", KEY_MOUSE_BUTTON_4);
            RegKey("mouse5", KEY_MOUSE_BUTTON_5);
        }

        /// <summary>Gets the <see cref="Keys"/> instance for the given input name, or <see cref="Keys.Unknown"/> if none.</summary>
        public static Keys GetKeyForName(string name)
        {
            if (NamesToKeys.TryGetValue(name.ToLowerFast(), out Keys key))
            {
                return key;
            }
            return Keys.Unknown;
        }

        /// <summary>The backing game client window.</summary>
        public GameClientWindow Window;

        /// <summary>The <see cref="KeyHandlerState"/> currently being built.</summary>
        public KeyHandlerState BuildingState = new();

        /// <summary>The set of keys pressed since last tick.</summary>
        public Queue<Keys> KeyPresses = new();

        /// <summary>The set of keys released since last tick.</summary>
        public Queue<Keys> KeyUps = new();

        /// <summary>Initialize and register the key handler into the window.</summary>
        public KeyHandler(GameClientWindow _window)
        {
            Window = _window;
            Window.Window.TextInput += PrimaryGameWindow_KeyPress;
            Window.Window.KeyDown += PrimaryGameWindow_KeyDown;
            Window.Window.KeyUp += PrimaryGameWindow_KeyUp;
            Window.Window.MouseWheel += Mouse_Wheel;
            Window.Window.MouseDown += Mouse_ButtonDown;
            Window.Window.MouseUp += Mouse_ButtonUp;
        }

        /// <summary>Unlinks the key handler from the backing window.</summary>
        public void Shutdown()
        {
            Window.Window.TextInput -= PrimaryGameWindow_KeyPress;
            Window.Window.KeyDown -= PrimaryGameWindow_KeyDown;
            Window.Window.KeyUp -= PrimaryGameWindow_KeyUp;
            Window.Window.MouseWheel -= Mouse_Wheel;
            Window.Window.MouseDown -= Mouse_ButtonDown;
            Window.Window.MouseUp -= Mouse_ButtonUp;
        }

        /// <summary>Called every time a key is pressed, adds to the Keyboard String.</summary>
        /// <param name="e">Holds the pressed Keys.</param>
        public void PrimaryGameWindow_KeyPress(TextInputEventArgs e)
        {
            if (!Window.Window.IsFocused)
            {
                return;
            }
            char c = Convert.ToChar(e.Unicode);
            if (char.IsControl(c))
            {
                return;
            }
            BuildingState.KeyboardString += c;
        }

        /// <summary>Called every time a mouse button is pressed.</summary>
        public void Mouse_ButtonDown(MouseButtonEventArgs e)
        {
            if (!Window.Window.IsFocused)
            {
                return;
            }
            switch (e.Button)
            {
                case MouseButton.Left:
                    KeyPresses.Enqueue(KEY_MOUSE_LEFT_BUTTON);
                    break;
                case MouseButton.Right:
                    KeyPresses.Enqueue(KEY_MOUSE_RIGHT_BUTTON);
                    break;
                case MouseButton.Middle:
                    KeyPresses.Enqueue(KEY_MOUSE_MIDDLE_BUTTON);
                    break;
                case MouseButton.Button4:
                    KeyPresses.Enqueue(KEY_MOUSE_BUTTON_4);
                    break;
                case MouseButton.Button5:
                    KeyPresses.Enqueue(KEY_MOUSE_BUTTON_5);
                    break;
                    // TODO: More mouse buttons?
            }
        }

        /// <summary>Called every time the mouse wheel is moved.</summary>
        public void Mouse_Wheel(MouseWheelEventArgs e)
        {
            if (!Window.Window.IsFocused)
            {
                return;
            }
            if (e.OffsetY != 0)
            {
                Keys k = e.OffsetY < 0 ? KEY_MOUSE_WHEEL_DOWN : KEY_MOUSE_WHEEL_UP;
                if (!KeyPresses.Contains(k))
                {
                    KeyPresses.Enqueue(k);
                }
            }
        }

        /// <summary>Called every time a mouse button is released.</summary>
        public void Mouse_ButtonUp(MouseButtonEventArgs e)
        {
            if (!Window.Window.IsFocused)
            {
                return;
            }
            switch (e.Button)
            {
                case MouseButton.Left:
                    KeyUps.Enqueue(KEY_MOUSE_LEFT_BUTTON);
                    break;
                case MouseButton.Right:
                    KeyUps.Enqueue(KEY_MOUSE_RIGHT_BUTTON);
                    break;
                case MouseButton.Middle:
                    KeyUps.Enqueue(KEY_MOUSE_MIDDLE_BUTTON);
                    break;
                case MouseButton.Button4:
                    KeyUps.Enqueue(KEY_MOUSE_BUTTON_4);
                    break;
                case MouseButton.Button5:
                    KeyUps.Enqueue(KEY_MOUSE_BUTTON_5);
                    break;
                    // TODO: More mouse buttons?
            }
        }

        /// <summary>Called every time a key is pressed down, handles control codes for the Keyboard String.</summary>
        /// <param name="e">Holds the pressed Keys.</param>
        public void PrimaryGameWindow_KeyDown(KeyboardKeyEventArgs e)
        {
            if (!Window.Window.IsFocused)
            {
                return;
            }
            switch (e.Key)
            {
                case Keys.Escape:
                    BuildingState.Escaped = true;
                    break;
                case Keys.Enter:
                    BuildingState.KeyboardString += "\n";
                    break;
                case Keys.Tab:
                    BuildingState.KeyboardString += "\t";
                    break;
                case Keys.PageUp:
                    BuildingState.Pages++;
                    break;
                case Keys.PageDown:
                    BuildingState.Pages--;
                    break;
                case Keys.Up:
                    BuildingState.Scrolls++;
                    break;
                case Keys.Down:
                    BuildingState.Scrolls--;
                    break;
                case Keys.Left:
                    BuildingState.LeftRights--;
                    break;
                case Keys.Right:
                    BuildingState.LeftRights++;
                    break;
                case Keys.End:
                    BuildingState.LeftRights = 9000;
                    break;
                case Keys.Home:
                    BuildingState.LeftRights = -9000;
                    break;
                case Keys.LeftControl:
                case Keys.RightControl:
                    BuildingState.ControlDown = true;
                    break;
                case Keys.C:
                    if (BuildingState.ControlDown)
                    {
                        BuildingState.CopyPressed = true;
                    }
                    break;
                case Keys.Backspace:
                    if (BuildingState.KeyboardString.Length == 0)
                    {
                        BuildingState.InitBS++;
                    }
                    else
                    {
                        BuildingState.KeyboardString = BuildingState.KeyboardString[0..^1];
                    }
                    break;
                case Keys.Delete:
                    BuildingState.EndDelete++;
                    break;
                case Keys.V:
                    if (BuildingState.ControlDown)
                    {
                        string copied;
                        copied = TextCopy.ClipboardService.GetText().Replace('\r', ' ');
                        if (copied.Length > 0 && copied.EndsWith("\n"))
                        {
                            copied = copied[0..^1];
                        }
                        BuildingState.KeyboardString += copied;
                        for (int i = 0; i < BuildingState.KeyboardString.Length; i++)
                        {
                            if (BuildingState.KeyboardString[i] < 32 && BuildingState.KeyboardString[i] != '\n')
                            {
                                BuildingState.KeyboardString = BuildingState.KeyboardString[..i] +
                                    BuildingState.KeyboardString[(i + 1)..];
                                i--;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            KeyPresses.Enqueue(e.Key);
        }

        /// <summary>Called every time a key is released, handles control codes for the Keyboard String.</summary>
        /// <param name="e">Holds the pressed Keys.</param>
        public void PrimaryGameWindow_KeyUp(KeyboardKeyEventArgs e)
        {
            if (!Window.Window.IsFocused)
            {
                return;
            }
            switch (e.Key)
            {
                case Keys.LeftControl:
                case Keys.RightControl:
                    BuildingState.ControlDown = false;
                    break;
                default:
                    break;
            }
            KeyUps.Enqueue(e.Key);
        }

        /// <summary>Resets the building <see cref="KeyHandlerState"/> at the end of a frame.</summary>
        public void ResetState()
        {
            BuildingState.KeyboardString = "";
            BuildingState.InitBS = 0;
            BuildingState.CopyPressed = false;
            BuildingState.EndDelete = 0;
            BuildingState.LeftRights = 0;
            BuildingState.Pages = 0;
            BuildingState.Scrolls = 0;
            BuildingState.Escaped = false;
        }
    }
}
