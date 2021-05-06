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
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;

namespace FGEGraphics.UISystem
{
    /// <summary>Helper for positioning of UI elements.</summary>
    public class UIPositionHelper
    {
        /// <summary>
        /// Constructs the UI Position Helper.
        /// </summary>
        /// <param name="uiview">The backing view.</param>
        public UIPositionHelper(ViewUI2D uiview)
        {
            View = uiview;
            ConstantXY(0, 0);
            ConstantWidthHeight(0, 0);
            ConstantRotation(0f);
        }

        /// <summary>The view backing this element's positioning logic.</summary>
        public ViewUI2D View;

        /// <summary>The element this is the position for.</summary>
        public UIElement For;

        /// <summary>The main positional anchor.</summary>
        public UIAnchor MainAnchor = UIAnchor.CENTER;

        /// <summary>Internal data for <see cref="UIPositionHelper"/>. Generally, do not access this directly.</summary>
        public struct InternalData
        {
            /// <summary>Position mode for X.</summary>
            public UIPosMode PM_X;

            /// <summary>
            /// Constant value for X, if <see cref="PM_X"/> is set to <see cref="UIPosMode.CONSTANT"/>.
            /// Generally, instead of reading this, use <see cref="X"/>. Instead of setting this, use <see cref="ConstantX(int)"/>.
            /// </summary>
            public int Const_X;

            /// <summary>
            /// Getter value for X, if <see cref="PM_X"/> is set to <see cref="UIPosMode.GETTER"/>.
            /// Generally, instead of reading this, use <see cref="X"/>. Instead of setting this, use <see cref="GetterX(Func{int})"/>.
            /// </summary>
            public Func<int> Getter_X;

            /// <summary>Position mode for Y.</summary>
            public UIPosMode PM_Y;

            /// <summary>
            /// Constant value for Y, if <see cref="PM_Y"/> is set to <see cref="UIPosMode.CONSTANT"/>.
            /// Generally, instead of reading this, use <see cref="Y"/>. Instead of setting this, use <see cref="ConstantY(int)"/>.
            /// </summary>
            public int Const_Y;

            /// <summary>
            /// Getter value for Y, if <see cref="PM_Y"/> is set to <see cref="UIPosMode.GETTER"/>.
            /// Generally, instead of reading this, use <see cref="Y"/>. Instead of setting this, use <see cref="GetterY(Func{int})"/>.
            /// </summary>
            public Func<int> Getter_Y;

            /// <summary>Position mode for Width.</summary>
            public UIPosMode PM_Width;

            /// <summary>
            /// Constant value for Width, if <see cref="PM_Width"/> is set to <see cref="UIPosMode.CONSTANT"/>.
            /// Generally, instead of reading this, use <see cref="Width"/>. Instead of setting this, use <see cref="ConstantWidth(int)"/>.
            /// </summary>
            public int Const_Width;

            /// <summary>
            /// Getter value for Width, if <see cref="PM_Width"/> is set to <see cref="UIPosMode.GETTER"/>.
            /// Generally, instead of reading this, use <see cref="Width"/>. Instead of setting this, use <see cref="GetterWidth(Func{int})"/>.
            /// </summary>
            public Func<int> Getter_Width;

            /// <summary>Position mode for Height.</summary>
            public UIPosMode PM_Height;

            /// <summary>
            /// Constant value for Height, if <see cref="PM_Height"/> is set to <see cref="UIPosMode.CONSTANT"/>.
            /// Generally, instead of reading this, use <see cref="Height"/>. Instead of setting this, use <see cref="ConstantHeight(int)"/>.
            /// </summary>
            public int Const_Height;

            /// <summary>
            /// Getter value for Height, if <see cref="PM_Height"/> is set to <see cref="UIPosMode.GETTER"/>.
            /// Generally, instead of reading this, use <see cref="Height"/>. Instead of setting this, use <see cref="GetterHeight(Func{int})"/>.
            /// </summary>
            public Func<int> Getter_Height;

            /// <summary>Position mode for Rotation.</summary>
            public UIPosMode PM_Rot;

            /// <summary>
            /// Constant value for Rotation, if <see cref="PM_Rot"/> is set to <see cref="UIPosMode.CONSTANT"/>.
            /// Generally, instead of reading this, use <see cref="Rotation"/>. Instead of setting this, use <see cref="ConstantRotation(float)"/>.
            /// </summary>
            public float Const_Rot;

            /// <summary>
            /// Getter value for Rotation, if <see cref="PM_Rot"/> is set to <see cref="UIPosMode.GETTER"/>.
            /// Generally, instead of reading this, use <see cref="Rotation"/>. Instead of setting this, use <see cref="GetterRotation(Func{float})"/>.
            /// </summary>
            public Func<float> Getter_Rot;
        }

        /// <summary>Internal data that should usually not be accessed directly.</summary>
        public InternalData Internal;

        /// <summary>
        /// Sets an anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper Anchor(UIAnchor anchor)
        {
            MainAnchor = anchor;
            return this;
        }

        /// <summary>
        /// Sets a constant X value.
        /// </summary>
        /// <param name="x">The X value.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper ConstantX(int x)
        {
            Internal.PM_X = UIPosMode.CONSTANT;
            Internal.Const_X = x;
            return this;
        }

        /// <summary>
        /// Sets a constant Y value.
        /// </summary>
        /// <param name="y">The Y value.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper ConstantY(int y)
        {
            Internal.PM_Y = UIPosMode.CONSTANT;
            Internal.Const_Y = y;
            return this;
        }

        /// <summary>
        /// Sets a constant X and Y value.
        /// </summary>
        /// <param name="x">The X value.</param>
        /// <param name="y">The Y value.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper ConstantXY(int x, int y)
        {
            Internal.PM_X = UIPosMode.CONSTANT;
            Internal.Const_X = x;
            Internal.PM_Y = UIPosMode.CONSTANT;
            Internal.Const_Y = y;
            return this;
        }

        /// <summary>
        /// Sets a constant Width value.
        /// </summary>
        /// <param name="width">The Width value.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper ConstantWidth(int width)
        {
            Internal.PM_Width = UIPosMode.CONSTANT;
            Internal.Const_Width = width;
            return this;
        }

        /// <summary>
        /// Sets a constant Height value.
        /// </summary>
        /// <param name="height">The Height value.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper ConstantHeight(int height)
        {
            Internal.PM_Height = UIPosMode.CONSTANT;
            Internal.Const_Height = height;
            return this;
        }

        /// <summary>
        /// Sets a constant Width and Height value.
        /// </summary>
        /// <param name="width">The Width value.</param>
        /// <param name="height">The Height value.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper ConstantWidthHeight(int width, int height)
        {
            Internal.PM_Width = UIPosMode.CONSTANT;
            Internal.Const_Width = width;
            Internal.PM_Height = UIPosMode.CONSTANT;
            Internal.Const_Height = height;
            return this;
        }

        /// <summary>
        /// Sets a constant Rotation value.
        /// </summary>
        /// <param name="rotation">The Rotation value.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper ConstantRotation(float rotation)
        {
            Internal.PM_Rot = UIPosMode.CONSTANT;
            Internal.Const_Rot = rotation;
            return this;
        }

        /// <summary>
        /// Sets a getter X value.
        /// </summary>
        /// <param name="x">The X getter.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper GetterX(Func<int> x)
        {
            Internal.PM_X = UIPosMode.GETTER;
            Internal.Getter_X = x;
            return this;
        }

        /// <summary>
        /// Sets a getter Y value.
        /// </summary>
        /// <param name="y">The Y getter.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper GetterY(Func<int> y)
        {
            Internal.PM_Y = UIPosMode.GETTER;
            Internal.Getter_Y = y;
            return this;
        }

        /// <summary>
        /// Sets a getter X and Y value.
        /// </summary>
        /// <param name="x">The X getter.</param>
        /// <param name="y">The Y getter.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper GetterXY(Func<int> x, Func<int> y)
        {
            Internal.PM_X = UIPosMode.GETTER;
            Internal.Getter_X = x;
            Internal.PM_Y = UIPosMode.GETTER;
            Internal.Getter_Y = y;
            return this;
        }

        /// <summary>
        /// Sets a getter Width value.
        /// </summary>
        /// <param name="width">The Width getter.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper GetterWidth(Func<int> width)
        {
            Internal.PM_Width = UIPosMode.GETTER;
            Internal.Getter_Width = width;
            return this;
        }

        /// <summary>
        /// Sets a getter Height value.
        /// </summary>
        /// <param name="height">The Height getter.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper GetterHeight(Func<int> height)
        {
            Internal.PM_Height = UIPosMode.GETTER;
            Internal.Getter_Height = height;
            return this;
        }

        /// <summary>
        /// Sets a constant Width and Height value.
        /// </summary>
        /// <param name="width">The Width getter.</param>
        /// <param name="height">The Height getter.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper GetterWidthHeight(Func<int> width, Func<int> height)
        {
            Internal.PM_Width = UIPosMode.GETTER;
            Internal.Getter_Width = width;
            Internal.PM_Height = UIPosMode.GETTER;
            Internal.Getter_Height = height;
            return this;
        }

        /// <summary>
        /// Sets a getter Rotation value.
        /// </summary>
        /// <param name="rotation">The Rotation getter.</param>
        /// <returns>This object.</returns>
        public UIPositionHelper GetterRotation(Func<float> rotation)
        {
            Internal.PM_Rot = UIPosMode.GETTER;
            Internal.Getter_Rot = rotation;
            return this;
        }

        /// <summary>Gets the X coordinate.</summary>
        public int X
        {
            get
            {
                int anch = For.Parent != null ? MainAnchor.GetX(For) : 0;
                if (Internal.PM_X == UIPosMode.CONSTANT)
                {
                    return anch + Internal.Const_X;
                }
                if (Internal.PM_X == UIPosMode.GETTER)
                {
                    return anch + Internal.Getter_X();
                }
                return anch;
            }
        }

        /// <summary>Gets the Y coordinate.</summary>
        public int Y
        {
            get
            {
                int anch = For.Parent != null ? MainAnchor.GetY(For) : 0;
                if (Internal.PM_Y == UIPosMode.CONSTANT)
                {
                    return anch + Internal.Const_Y;
                }
                if (Internal.PM_Y == UIPosMode.GETTER)
                {
                    return anch + Internal.Getter_Y();
                }
                return anch;
            }
        }

        /// <summary>Gets the width.</summary>
        public int Width
        {
            get
            {
                if (Internal.PM_Width == UIPosMode.CONSTANT)
                {
                    return Internal.Const_Width;
                }
                if (Internal.PM_Width == UIPosMode.GETTER)
                {
                    return Internal.Getter_Width();
                }
                return 0;
            }
        }

        /// <summary>Gets the height.</summary>
        public int Height
        {
            get
            {
                if (Internal.PM_Height == UIPosMode.CONSTANT)
                {
                    return Internal.Const_Height;
                }
                if (Internal.PM_Height == UIPosMode.GETTER)
                {
                    return Internal.Getter_Height();
                }
                return 0;
            }
        }

        /// <summary>Gets the local Rotation value.</summary>
        public float Rotation
        {
            get
            {
                if (Internal.PM_Rot == UIPosMode.CONSTANT)
                {
                    return Internal.Const_Rot;
                }
                if (Internal.PM_Rot == UIPosMode.GETTER)
                {
                    return Internal.Getter_Rot();
                }
                return 0f;
            }
        }

        /// <summary>Gets the X/Y coordinate pair.</summary>
        public Vector2i Position => new Vector2i(X, Y);

        /// <summary>Gets the Width/Height coordinate pair.</summary>
        public Vector2i Size => new Vector2i(Width, Height);

        /// <summary>Converts this position helper's present data to a simplified debug string.</summary>
        public override string ToString()
        {
            return $"UIPositionHelper:PresentState(XY: {X}, {Y} / WH: {Width}, {Height} / Rot: {Rotation})";
        }
    }

    /// <summary>Modes for the <see cref="UIPositionHelper"/>.</summary>
    public enum UIPosMode : byte
    {
        /// <summary>A constant position.</summary>
        CONSTANT = 0,
        /// <summary>A getter function.</summary>
        GETTER = 1
        // TODO: More modes!
    }
}
