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
using System.Runtime.InteropServices;
using Valve.VR;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers.Textures;
using FGEGraphics.GraphicsHelpers.Models;

namespace FGEGraphics.GraphicsHelpers
{
    /// <summary>
    /// The system to support VR on a 3D client engine.
    /// </summary>
    public class VRSupport
    {
        /// <summary>
        /// The internal VR System.
        /// </summary>
        public CVRSystem VR = null;

        /// <summary>
        /// The game client window.
        /// </summary>
        public GameClientWindow TheClient;

        /// <summary>
        /// The internal VR Compositor.
        /// </summary>
        public CVRCompositor Compositor;

        /// <summary>
        /// Returns whether VR is available at all (not necessarily loaded).
        /// </summary>
        /// <returns>VR readiness.</returns>
        public static bool Available()
        {
            return OpenVR.IsHmdPresent();
        }

        /// <summary>
        /// Tries to initialize VR - returns the VR support object, or null if loading failed.
        /// </summary>
        /// <param name="tclient">The game client.</param>
        /// <returns>VR, if any.</returns>
        public static VRSupport TryInit(GameClientWindow tclient)
        {
            if (!Available())
            {
                return null;
            }
            EVRInitError err = EVRInitError.None;
            VRSupport vrs = new VRSupport() { TheClient = tclient, VR = OpenVR.Init(ref err) };
            if (err != EVRInitError.None)
            {
                SysConsole.Output(OutputType.INFO, "VR error: " + err + ": " + OpenVR.GetStringForHmdError(err));
                return null;
            }
            vrs.Start();
            return vrs;
        }

        /// <summary>
        /// The currently running VR model, EG 'oculus'. Can be 'unknown'.
        /// </summary>
        public string VRModel = "Unknown";
        
        /// <summary>
        /// Internal start call for VR setup.
        /// </summary>
        public void Start()
        {
            uint w = 0;
            uint h = 0;
            VR.GetRecommendedRenderTargetSize(ref w, ref h);
            if (w <= 0 || h <= 0)
            {
                throw new Exception("Failed to start VR: Invalid render target size!");
            }
            w *= 2;
            TheClient.Engine3D.MainView.Generate(TheClient.Engine3D, (int)w, (int)h);
            TheClient.Engine3D.MainView.GenerateFBO();
            StringBuilder val = new StringBuilder(256);
            ETrackedPropertyError errx = ETrackedPropertyError.TrackedProp_Success;
            VR.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String, val, 256, ref errx);
            SysConsole.Output(OutputType.INIT, "Switching to VR mode: " + w + "/" + h + "... " + val.ToString());
            VRModel = val.ToString();
            Compositor = OpenVR.Compositor;
            Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseStanding);
            Compositor.CompositorBringToFront();
            LeftTexture = new VRControllerTextureEngine();
            RightTexture = new VRControllerTextureEngine();
            LeftTexture.BaseTexture = TheClient.Textures.GetTexture("vr/controller/vive_circle_left");
            RightTexture.BaseTexture = TheClient.Textures.GetTexture("vr/controller/vive_circle_right");
            LeftTexture.GenerateFirst();
            RightTexture.GenerateFirst();
            RightControllerModel = TheClient.Models.GetModel("vr/controller/vive");
            LeftControllerModel = TheClient.Models.GetModel("vr/controller/vive");
        }

        /// <summary>
        /// Gets the eye view point (boolean to specify left or right).
        /// </summary>
        /// <param name="lefteye">Left or right.</param>
        /// <returns>The eye view point matrix.</returns>
        public Matrix4 Eye(bool lefteye)
        {
            HmdMatrix34_t temp = VR.GetEyeToHeadTransform(lefteye ? EVREye.Eye_Left : EVREye.Eye_Right);
            Matrix4 eye = new Matrix4(temp.m0, temp.m1, temp.m2, temp.m3, temp.m4, temp.m5, temp.m6, temp.m7, temp.m8, temp.m9, temp.m10, temp.m11, 0, 0, 0, 1);
            eye.Transpose();
            eye = eye.ClearTranslation() * Matrix4.CreateTranslation(eye.ExtractTranslation() * VRScale);
            return headMat * eye;
        }

        /// <summary>
        /// Gets the projection for an eye view point.
        /// </summary>
        /// <param name="lefteye">Left or right.</param>
        /// <param name="znear">Near plane.</param>
        /// <param name="zfar">Far plane.</param>
        /// <returns>Eye projection.</returns>
        public Matrix4 GetProjection(bool lefteye, float znear, float zfar)
        {
            HmdMatrix44_t temp = VR.GetProjectionMatrix(!lefteye ? EVREye.Eye_Left : EVREye.Eye_Right, znear, zfar, EGraphicsAPIConvention.API_OpenGL);
            Matrix4 proj = new Matrix4(temp.m0, temp.m1, temp.m2, temp.m3, temp.m4, temp.m5, temp.m6, temp.m7, temp.m8, temp.m9, temp.m10, temp.m11, temp.m12, temp.m13, temp.m14, temp.m15);
            proj.Transpose();
            return proj;
        }

        /// <summary>
        /// Stops the VR system. Will likely require additionally steps to fully remove VR from the client.
        /// </summary>
        public void Stop()
        {
            OpenVR.Shutdown();
            LeftTexture.Destroy();
            RightTexture.Destroy();
        }

        /// <summary>
        /// The head matrix rotationj.
        /// </summary>
        public Matrix4 HeadMatRot = Matrix4.Identity;

        /// <summary>
        /// The head matrix.
        /// </summary>
        public Matrix4 headMat = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);

        /// <summary>
        /// Gets the current controller state.
        /// </summary>
        /// <param name="left">Left or right hand controller.</param>
        /// <returns>The controller state.</returns>
        public VRController GetController(bool left)
        {
            VRControllerState_t vrcont = new VRControllerState_t();
            TrackedDevicePose_t vrpose = new TrackedDevicePose_t();

            bool valid = VR.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding, VR.GetTrackedDeviceIndexForControllerRole(left ? ETrackedControllerRole.LeftHand : ETrackedControllerRole.RightHand), ref vrcont, ref vrpose);
            if (!valid || !vrpose.bPoseIsValid)
            {
                return null;
            }
            HmdMatrix34_t tmat = vrpose.mDeviceToAbsoluteTracking;
            Matrix4 resp = new Matrix4(tmat.m0, tmat.m1, tmat.m2, tmat.m3, tmat.m4, tmat.m5, tmat.m6, tmat.m7, tmat.m8, tmat.m9, tmat.m10, tmat.m11, 0, 0, 0, 1);
            resp.Transpose();
            resp = resp.ClearTranslation() * Matrix4.CreateTranslation(resp.ExtractTranslation() * VRScale);
            resp = resp * Matrix4.CreateRotationX((float)(Math.PI * 0.5));
            VRController res = new VRController()
            {
                Position = resp,
                Touched = (VRButtons)vrcont.ulButtonTouched,
                Pressed = (VRButtons)vrcont.ulButtonPressed
            };
            res.Axes[0] = new Vector2(vrcont.rAxis0.x, vrcont.rAxis0.y);
            res.Axes[1] = new Vector2(vrcont.rAxis1.x, vrcont.rAxis1.y);
            res.Axes[2] = new Vector2(vrcont.rAxis2.x, vrcont.rAxis2.y);
            res.Axes[3] = new Vector2(vrcont.rAxis3.x, vrcont.rAxis3.y);
            res.Axes[4] = new Vector2(vrcont.rAxis4.x, vrcont.rAxis4.y);
            return res;
        }

        /// <summary>
        /// Current left controller.
        /// </summary>
        public VRController Left;

        /// <summary>
        /// Current right controller.
        /// </summary>
        public VRController Right;

        /// <summary>
        /// Model for the left controller.
        /// </summary>
        public Model LeftControllerModel;

        /// <summary>
        /// Model for the right controller.
        /// </summary>
        public Model RightControllerModel;

        /// <summary>
        /// Helper for VR left controller textures.
        /// </summary>
        public VRControllerTextureEngine LeftTexture;

        /// <summary>
        /// Helper for VR right controller textures.
        /// </summary>
        public VRControllerTextureEngine RightTexture;

        /// <summary>
        /// The scaling from VR space to game space.
        /// </summary>
        public float VRScale = 1f;

        /// <summary>
        /// Submits the VR view to the screen.
        /// </summary>
        public void Submit()
        {
            VREvent_t evt = new VREvent_t();
            while (VR.PollNextEvent(ref evt, (uint)Marshal.SizeOf(typeof(VREvent_t))))
            {
                // No need to do anything here!
            }
            TrackedDevicePose_t[] rposes = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            TrackedDevicePose_t[] gposes = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            EVRCompositorError merr = Compositor.WaitGetPoses(rposes, gposes);
            if (rposes[OpenVR.k_unTrackedDeviceIndex_Hmd].bPoseIsValid)
            {
                HmdMatrix34_t tmat = rposes[OpenVR.k_unTrackedDeviceIndex_Hmd].mDeviceToAbsoluteTracking;
                headMat = new Matrix4(tmat.m0, tmat.m1, tmat.m2, tmat.m3, tmat.m4, tmat.m5, tmat.m6, tmat.m7, tmat.m8, tmat.m9, tmat.m10, tmat.m11, 0, 0, 0, 1);
                headMat.Transpose();
                HeadMatRot = headMat * Matrix4.CreateRotationX((float)(Math.PI * 0.5));
                headMat = headMat * Matrix4.CreateRotationX((float)(Math.PI * 0.5));
                headMat = headMat.ClearTranslation() * Matrix4.CreateTranslation(headMat.ExtractTranslation() * VRScale);
                headMat.Invert();
            }
            if (merr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.WARNING, "Posing error: " + merr);
            }
            Left = GetController(true);
            Right = GetController(false);
            if (!Compositor.CanRenderScene())
            {
                SysConsole.Output(OutputType.WARNING, "Can't render VR scene!");
            }
            Texture_t left = new Texture_t()
            {
                eColorSpace = EColorSpace.Auto,
                eType = EGraphicsAPIConvention.API_OpenGL,
                handle = new IntPtr(TheClient.Engine3D.MainView.CurrentFBOTexture)
            };
            VRTextureBounds_t bounds = new VRTextureBounds_t()
            {
                uMin = 0f,
                uMax = 0.5f,
                vMin = 0f,
                vMax = 1f
            };
            EVRCompositorError lerr = Compositor.Submit(EVREye.Eye_Left, ref left, ref bounds, EVRSubmitFlags.Submit_Default);
            if (lerr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.WARNING, "Left eye error: " + lerr);
            }
            Texture_t right = new Texture_t()
            {
                eColorSpace = EColorSpace.Auto,
                eType = EGraphicsAPIConvention.API_OpenGL,
                handle = new IntPtr(TheClient.Engine3D.MainView.CurrentFBOTexture)
            };
            VRTextureBounds_t rbounds = new VRTextureBounds_t()
            {
                uMin = 0.5f,
                uMax = 1f,
                vMin = 0f,
                vMax = 1f
            };
            EVRCompositorError rerr = Compositor.Submit(EVREye.Eye_Right, ref right, ref rbounds, EVRSubmitFlags.Submit_Default);
            if (rerr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.WARNING, "Right eye error: " + rerr);
            }
        }
    }

    /// <summary>
    /// Represents a VR Controller.
    /// </summary>
    public class VRController
    {
        /// <summary>
        /// Gets the forward vector of this controller.
        /// </summary>
        /// <returns></returns>
        public Location ForwardVector()
        {
            OpenTK.Quaternion loquat = Position.ExtractRotation(true);
            BEPUutilities.Quaternion lquat = new BEPUutilities.Quaternion(loquat.X, loquat.Y, loquat.Z, loquat.W);
            BEPUutilities.Vector3 lforw = -BEPUutilities.Quaternion.Transform(BEPUutilities.Vector3.UnitZ, lquat);
            return new Location(lforw);
        }

        /// <summary>
        /// Trackpad axis ID.
        /// </summary>
        public const int AXIS_TRACKPAD = 0;

        /// <summary>
        /// Trigger axis ID.
        /// </summary>
        public const int AXIS_TRIGGER = 1;

        /// <summary>
        /// Side grip axis ID.
        /// </summary>
        public const int AXIS_SIDEGRIP = 2;

        /// <summary>
        /// The side-grip axis.
        /// </summary>
        public float SideGrip
        {
            get
            {
                return Axes[AXIS_SIDEGRIP].X;
            }
        }

        /// <summary>
        /// Gets the trackpad positioning.
        /// </summary>
        public Vector2 TrackPad
        {
            get
            {
                return Axes[AXIS_TRACKPAD];
            }
        }

        /// <summary>
        /// Gets the trigger positioning.
        /// </summary>
        public float Trigger
        {
            get
            {
                return Axes[AXIS_TRIGGER].X;
            }
        }

        /// <summary>
        /// The 3D position and rotation matrix for this controller.
        /// </summary>
        public Matrix4 Position;

        /// <summary>
        /// The axes of control potential.
        /// </summary>
        public Vector2[] Axes = new Vector2[5];

        /// <summary>
        /// All currently touched buttons, as a flag enum.
        /// </summary>
        public VRButtons Touched;

        /// <summary>
        /// All currently pressed buttons, as a flag enum.
        /// </summary>
        public VRButtons Pressed;

        /// <summary>
        /// Retrusn a simple string of the buttons pressed and axial motions on this controller.
        /// </summary>
        /// <returns>The buttons and axes as a string.</returns>
        public override string ToString()
        {
            return "Controller{" + Axes[0] + ", " + Axes[1] + ", " + Axes[2] + ", " + Axes[3] + ", " + Axes[4] + "... " + Touched + "... " + Pressed + "}";
        }
    }

    /// <summary>
    /// A helper to modify controller textures based on touches.
    /// </summary>
    public class VRControllerTextureEngine
    {
        /// <summary>
        /// The FBO.
        /// </summary>
        public int FBO;

        /// <summary>
        /// The texture.
        /// </summary>
        public int Texture;

        /// <summary>
        /// The base texture.
        /// </summary>
        public Texture BaseTexture;

        /// <summary>
        /// The color of the touched spot.
        /// </summary>
        public Color4 TouchSpotColor = Color4.Red;

        /// <summary>
        /// The color of the pressed spot.
        /// </summary>
        public Color4 PressSpotColor = Color4.DarkRed;

        /// <summary>
        /// The half-size of the touched spot.
        /// </summary>
        public Vector2 TouchSpotHalfSize = new Vector2(0.05f, 0.05f);

        /// <summary>
        /// The half-size of the pressed spot.
        /// </summary>
        public Vector2 PressSpotHalfSize = new Vector2(0.1f, 0.1f);

        /// <summary>
        /// Initial set up and generation.
        /// </summary>
        public void GenerateFirst()
        {
            FBO = GL.GenFramebuffer();
            Texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 512, 512, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Identity.
        /// </summary>
        static Matrix4 id = Matrix4.Identity;

        /// <summary>
        /// Current time.
        /// </summary>
        double CTime;

        /// <summary>
        /// Calculates the texture update.
        /// </summary>
        /// <param name="cont">The controller.</param>
        /// <param name="timeNow">The current time.</param>
        /// <param name="tclient">The game client.</param>
        public void CalcTexture(VRController cont, double timeNow, GameClientWindow tclient)
        {
            if (timeNow == CTime)
            {
                return;
            }
            CTime = timeNow;
            tclient.Shaders.ColorMultShader.Bind();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.Disable(EnableCap.CullFace);
            GL.Viewport(0, 0, 512, 512);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.5f, 0.5f, 0.5f, 1.0f });
            GL.ActiveTexture(TextureUnit.Texture0);
            BaseTexture.Bind();
            Matrix4 basic = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
            GL.UniformMatrix4(1, false, ref basic);
            GL.UniformMatrix4(2, false, ref id);
            tclient.Engine3D.Rendering.RenderRectangle(-1, -1, 1, 1);
            bool touched = cont.Touched.HasFlag(VRButtons.TRACKPAD);
            bool pressed = cont.Pressed.HasFlag(VRButtons.TRACKPAD);
            if (touched || pressed)
            {
                BaseTexture.Engine.White.Bind();
                tclient.Engine3D.Rendering.SetColor(pressed ? PressSpotColor : TouchSpotColor, tclient.Engine3D.MainView);
                Vector2 hsize = pressed ? PressSpotHalfSize : TouchSpotHalfSize;
                tclient.Engine3D.Rendering.RenderRectangle(cont.TrackPad.X - hsize.X, cont.TrackPad.Y - hsize.X, cont.TrackPad.X + hsize.X, cont.TrackPad.Y + hsize.Y);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
            tclient.Engine3D.Rendering.SetColor(Color4.White, tclient.Engine3D.MainView);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, tclient.Engine3D.MainView.cFBO);
            GL.Enable(EnableCap.CullFace);
            tclient.Engine3D.MainView.FixVP();
        }

        /// <summary>
        /// Destroys this object.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteFramebuffer(FBO);
            GL.DeleteTexture(Texture);
        }
    }

    /// <summary>
    /// An enumeration of buttons known to the VR system.
    /// </summary>
    [Flags]
    public enum VRButtons : ulong // TODO: Less weird enum values/names. Also, Vive/Rift controller separation and explicit support.
    {
        /// <summary>
        /// No buttons down.
        /// </summary>
        NONE = 0,
        /// <summary>Unknown button.</summary>
        __A__ONE = 1,
        /// <summary>
        /// The menu button (Or "B", or "Y").
        /// </summary>
        MENU_BUTTON_BY = 2,
        /// <summary>
        /// The side grip.
        /// </summary>
        SIDE_GRIP = 4,
        /// <summary>Unknown button.</summary>
        __A__EIGHT = 8,
        /// <summary>Unknown button.</summary>
        __A__SIXTEEN = 16,
        /// <summary>Unknown button.</summary>
        __A__THIRTY_TWO = 32,
        /// <summary>Unknown button.</summary>
        __A__SIXTY_FOUR = 64,
        /// <summary>
        /// The "A" or "X" button.
        /// </summary>
        BUTTON_AX = 128,
        /// <summary>Unknown button.</summary>
        __A__TWO_FIFTY_SIX = 256,
        /// <summary>Unknown button.</summary>
        __A__FIVE_TWELVE = 512,
        /// <summary>Unknown button.</summary>
        __A__TEN_TWENTY_FOUR = 1024,
        /// <summary>Unknown button.</summary>
        __A__TWENTY_FOURTY_EIGHTY = 2048,
        /// <summary>Unknown button.</summary>
        __A__FOURTY_NINETY_SIX = 4096,
        /// <summary>Unknown button.</summary>
        __A__EIGHTY_ONE_NINETY_TWO = 8192,
        /// <summary>Unknown button.</summary>
        __N__ONE = 1 * 16384,
        /// <summary>Unknown button.</summary>
        __N__TWO = 2 * 16384,
        /// <summary>Unknown button.</summary>
        __N__FOUR = 4 * 16384,
        /// <summary>Unknown button.</summary>
        __N__EIGHT = 8 * 16384,
        /// <summary>Unknown button.</summary>
        __N__SIXTEEN = 16 * 16384,
        /// <summary>Unknown button.</summary>
        __N__THIRTY_TWO = 32 * 16384,
        /// <summary>Unknown button.</summary>
        __N__SIXTY_FOUR = 64 * 16384,
        /// <summary>Unknown button.</summary>
        __N__ONE_TWENTY_EIGHT = 128 * 16384,
        /// <summary>Unknown button.</summary>
        __N__TWO_FIFTY_SIX = 256 * 16384,
        /// <summary>Unknown button.</summary>
        __N__FIVE_TWELVE = 512 * 16384,
        /// <summary>Unknown button.</summary>
        __N__TEN_TWENTY_FOUR = 1024 * 16384,
        /// <summary>Unknown button.</summary>
        __N__TWENTY_FOURTY_EIGHTY = 2048 * 16384,
        /// <summary>Unknown button.</summary>
        __N__FOURTY_NINETY_SIX = 4096 * 16384,
        /// <summary>Unknown button.</summary>
        __N__EIGHTY_ONE_NINETY_TWO = 8192 * 16384,
        /// <summary>Unknown button.</summary>
        __U__ONE = 1 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__TWO = 2 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__FOUR = 4 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__EIGHT = 8 * 16384 * 16384ul,
        /// <summary>
        /// The track pad.
        /// </summary>
        TRACKPAD = 16 * 16384 * 16384ul,
        /// <summary>
        /// The trigger.
        /// </summary>
        TRIGGER = 32 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__SIXTY_FOUR = 64 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__ONE_TWENTY_EIGHT = 128 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__TWO_FIFTY_SIX = 256 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__FIVE_TWELVE = 512 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__TEN_TWENTY_FOUR = 1024 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__TWENTY_FOURTY_EIGHTY = 2048 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__FOURTY_NINETY_SIX = 4096 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __U__EIGHTY_ONE_NINETY_TWO = 8192 * 16384 * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__ONE = 1 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__TWO = 2 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__FOUR = 4 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__EIGHT = 8 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__SIXTEEN = 16 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__THIRTY_TWO = 32 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__SIXTY_FOUR = 64 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__ONE_TWENTY_EIGHT = 128 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__TWO_FIFTY_SIX = 256 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__FIVE_TWELVE = 512 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__TEN_TWENTY_FOUR = 1024 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__TWENTY_FOURTY_EIGHTY = 2048 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__FOURTY_NINETY_SIX = 4096 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __V__EIGHTY_ONE_NINETY_TWO = 8192 * 16384 * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__ONE = 1 * 16384 * 16384ul * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__TWO = 2 * 16384 * 16384ul * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__FOUR = 4 * 16384 * 16384ul * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__EIGHT = 8 * 16384 * 16384ul * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__SIXTEEN = 16 * 16384 * 16384ul * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__THIRTY_TWO = 32 * 16384 * 16384ul * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__SIXTY_FOUR = 64 * 16384 * 16384ul * 16384ul * 16384ul,
        /// <summary>Unknown button.</summary>
        __Z__ONE_TWENTY_EIGHT = 128 * 16384 * 16384ul * 16384ul * 16384ul
    }
}
