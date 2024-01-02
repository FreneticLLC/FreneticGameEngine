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
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.ViewRenderSystem;

/// <summary>Data internally used by <see cref="View3D"/>.</summary>
public class View3DInternalData
{
    /// <summary>
    /// Represents a simple orthographic matrix of all 1's and -1's.
    /// <para>Intentionally not readonly for compatibility with 'ref' params.</para>
    /// </summary>
    public static Matrix4 SimpleOrthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);

    /// <summary>
    /// An identity matrix - do not edit!
    /// <para>Intentionally not readonly for compatibility with 'ref' params.</para>
    /// </summary>
    public static Matrix4 IdentityMatrix = Matrix4.Identity;

    /// <summary>
    /// The depth clear float array (a float array with one value: 1.0f).
    /// This is, naturally, for input to "GL.ClearBuffer(ClearBuffer.DEPTH, bufId, DEPTH_CLEAR);"
    /// Always exactly [1f]. Can be reused elsewhere.
    /// </summary>
    public static readonly float[] ARR_FLOAT_1F_1 = [1.0f];

    /// <summary>LL needed info.</summary>
    public const int LL_AB_SIZE = 8 * 1024 * 1024; // TODO: Tweak me!

    /// <summary>LL needed info.</summary>
    public const int LL_P_SIZE = 4;

    /// <summary>
    /// What range to spread DynamicExposure calculations by. Gets squared to estimate graphics cores involved in process.
    /// Have to edit shader hdrpass.fs to adjust this.
    /// </summary>
    public const int DYNAMIC_EXPOSURE_SPREAD = 32;

    /// <summary>Maximum shadow textures at once (Some may be reused as sub-texture based shadows).</summary>
    public const int SHADOW_BITS_MAX = 17;

    /// <summary>Maximum lights at once.</summary>
    public const int LIGHTS_MAX = 38;

    /// <summary>Maximum strength of a light. Used for ortho light radius.</summary>
    public const float LIGHT_MAXIUM_RADIUS = 1E10f;

    /// <summary>
    /// How much distance between camera-relative-positioning jumps.
    /// The camera will never be more than this distance away from the render origin.
    /// </summary>
    public const double CAMERA_POSITION_JUMP_SCALE = 100;

    /// <summary>Transparency FrameBufferObject.</summary>
    public int FBO_Transparents_Main;

    /// <summary>Transparency FrameBufferObject color texture.</summary>
    public int FBO_Transparents_Texture;

    /// <summary>Transparency FrameBufferObject depth texture.</summary>
    public int FBO_Transparents_DepthTexture;

    /// <summary>Out-View FBO.</summary>
    public int FBO_OutView_Main;

    /// <summary>Out-View depth texture.</summary>
    public int FBO_OutView_DepthTexture;

    /// <summary>Current transparent textures for the Linked-List transparency trick.</summary>
    public int[] LL_TransparentTextures = new int[4];

    /// <summary>Tracks the result of a calculation on <see cref="View3DState.PreviousForward"/> from the previous frame.</summary>
    public Vector2 PreviousPFResult;

    /// <summary>FrameBufferObject used for DynamicExposure calculation.</summary>
    public int FBO_DynamicExposure;

    /// <summary>FrameBufferObject texture used for DynamicExposure calculation.</summary>
    public int FBO_DynamicExposure_Texture;

    /// <summary>Main FrameBufferObject texture.</summary>
    public int FBO_Screen_Texture;

    /// <summary>Main FrameBufferObject.</summary>
    public int FBO_Screen_Main;

    /// <summary>God-Ray FrameBufferObject.</summary>
    public int FBO_GodRay_Main;
    /// <summary>God-Ray FrameBufferObject texture.</summary>
    public int FBO_GodRay_Texture;
    /// <summary>God-Ray FrameBufferObject second texture.</summary>
    public int FBO_GodRay_Texture_2;

    /// <summary>Decals FrameBufferObject.</summary>
    public int FBO_Decal;

    /// <summary>Decals FrameBufferObject depth texture.</summary>
    public int FBO_Decal_Depth;

    /// <summary>Decals FrameBufferObject texture.</summary>
    public int FBO_Decal_Texture;

    /// <summary>Shadow FrameBufferObject.</summary>
    public int[] FBO_Shadow = new int[SHADOW_BITS_MAX + 1];
    /// <summary>Shadow FrameBufferObject depth texture.</summary>
    public int FBO_Shadow_DepthTexture;
    /// <summary>Shadow FrameBufferObject color texture.</summary>
    public int FBO_Shadow_ColorTexture;

    /// <summary>The texture the system is using for a simple 'flash bang' effect.</summary>
    public int Flash_Texture;

    /// <summary>The next frame texture.</summary>
    public int FBO_NextFrame_Texture;

    /// <summary>The next frame FBO.</summary>
    public int FBO_NextFrame;

    /// <summary>The next frame depth texture.</summary>
    public int FBO_NextFrame_DepthTexture;

    /// <summary>The current view FBO.</summary>
    public int CurrentFBO;

    /// <summary>The current view FBO texture.</summary>
    public int CurrentFBOTexture;

    /// <summary>The current view FBO depth texture.</summary>
    public int CurrentFBODepth;

    /// <summary>The current FBO, as tracked by <see cref="View3D.BindFramebuffer(FramebufferTarget, int)"/>.</summary>
    public int LastBoundFramebuffer;

    /// <summary>Helper array to calculate dynamic exposure result data.</summary>
    public float[] DynamicExposureResult = new float[DYNAMIC_EXPOSURE_SPREAD * DYNAMIC_EXPOSURE_SPREAD];
}
