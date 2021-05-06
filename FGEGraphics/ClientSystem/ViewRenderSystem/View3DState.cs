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
using FGEGraphics.GraphicsHelpers;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>
    /// Represents the current state of <see cref="View3D"/>.
    /// </summary>
    public class View3DState
    {
        /// <summary>
        /// An RS4P used in some locations.
        /// </summary>
        public DeferredRenderTarget DeferredTarget;

        /// <summary>
        /// The camera frustum.
        /// </summary>
        public Frustum CameraFrustum;

        /// <summary>
        /// The position of the camera, relative to rendering origin.
        /// </summary>
        public Vector3 CameraRelativePosition;

        /// <summary>
        /// Whether the system is rendering things where specular value is relevant.
        /// </summary>
        public bool RenderSpecular = false;

        /// <summary>
        /// Whether shadows are the only thing to render.
        /// </summary>
        public bool ShadowsOnly = false;

        /// <summary>
        /// Current lights count.
        /// </summary>
        public int LightCount = 0;

        /// <summary>
        /// Whether the system is rendering lighting effects.
        /// </summary>
        public bool RenderLights = false;

        /// <summary>
        /// Whether to not touch the buffer mode.
        /// </summary>
        public bool BufferDontTouch = false;

        /// <summary>
        /// Whether transparent objects should currently be casting shadows.
        /// </summary>
        public bool TranspShadows = true;

        /// <summary>
        /// The current FBO ID.
        /// </summary>
        public FBOID FBOid;

        /// <summary>
        /// Current DynamicExposure calculated exposure value.
        /// </summary>
        public float CurrentExposure = 1.0f;

        /// <summary>
        /// Duration left on a simple 'flash bang' effect.
        /// </summary>
        public double Flash_DurationLeft = 0.0;

        /// <summary>
        /// The tracked draw buffer mode.
        /// </summary>
        public DrawBufferMode BufferMode = DrawBufferMode.Back;

        /// <summary>
        /// Whether textures are currently rendering.
        /// </summary>
        public bool RenderTextures = false;

        /// <summary>
        /// Whether shadows are currently rendering.
        /// </summary>
        public bool RenderingShadows = false;

        /// <summary>
        /// The current view matrix.
        /// </summary>
        public Matrix4 PrimaryMatrix;

        /// <summary>
        /// The second-eye view frustum.
        /// </summary>
        public Frustum SecondEyeFrustum;

        /// <summary>
        /// The primary frustum.
        /// </summary>
        public Frustum CurrentFrustum;

        /// <summary>
        /// The long-view ("out view") frustum.
        /// </summary>
        public Frustum LongFrustum;

        /// <summary>
        /// The offset of the world matrix.
        /// </summary>
        public Matrix4d OffsetWorld = Matrix4d.Identity;

        /// <summary>
        /// Viewport X reference.
        /// </summary>
        public int ViewportX;

        /// <summary>
        /// Viewport Y reference.
        /// </summary>
        public int ViewportY;

        /// <summary>
        /// Viewport width reference.
        /// </summary>
        public int ViewportWidth;

        /// <summary>
        /// Viewport height reference.
        /// </summary>
        public int ViewportHeight;

        /// <summary>
        /// The primary matrix, as doubles.
        /// </summary>
        public Matrix4d PrimaryMatrixd;

        /// <summary>
        /// The primary matrix in 3D view mode's second eye if needed.
        /// </summary>
        public Matrix4d PrimaryMatrix_OffsetFor3Dd;

        /// <summary>
        /// The matrix for very far distance rendering.
        /// </summary>
        public Matrix4 OutViewMatrix = Matrix4.Identity;

        /// <summary>
        /// The location all rendering is relative to.
        /// </summary>
        public Location RenderRelative;

        /// <summary>
        /// The camera forward vector.
        /// </summary>
        public Location CameraForward = Location.UnitX;

        /// <summary>
        /// The camera's base position (the camera position according to config at frame start, without any adjustments applied).
        /// </summary>
        public Location CameraBasePos;

        /// <summary>
        /// The camera's adjustment value for 3D (shift vector towards the left or right eye).
        /// </summary>
        public Location CameraAdjust;

        /// <summary>
        /// The primary matrix, as offset for 3D logic.
        /// </summary>
        public Matrix4 PrimaryMatrix_OffsetFor3D;

        /// <summary>
        /// The previous forward vector.
        /// </summary>
        public Location PreviousForward = Location.Zero;

        /// <summary>
        /// Outview matrix, offset for VR.
        /// </summary>
        public Matrix4 OutView_OffsetVR;

        /// <summary>
        /// Outview matrix, offset for VR.
        /// </summary>
        public Matrix4d OutView_OffsetVRd;

        /// <summary>
        /// Whether the system is currently rendering the second eye (When VR or 3D enabled).
        /// </summary>
        public bool IsSecondEye = false;

        /// <summary>
        /// The current (or last) transparent light count during deferred rendering.
        /// </summary>
        public int TransparentLightCount = 0;
    }
}
