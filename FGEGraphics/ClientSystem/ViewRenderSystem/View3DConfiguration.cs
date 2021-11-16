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
using FreneticUtilities.FreneticDataSyntax;
using FGECore.MathHelpers;
using FGEGraphics.LightingSystem;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>Contains basic configuration fields for <see cref="View3D"/>.</summary>
    public class View3DConfiguration
    {
        /// <summary>
        /// Whether to render in forward mode. Defaults to true.
        /// <para>If set false, will render in deferred mode.</para>
        /// </summary>
        public bool ForwardMode = true;

        /// <summary>Set this to whatever method call is needed after the solid rendering and we're switching to transparent rendering.</summary>
        public Action PostFirstRender = null;

        /// <summary>Set this to whatever method call renders all 3D decals in this view.</summary>
        public Action<View3D> DecalRender = null;

        /// <summary>Set this to whatever method call renders all 3D objects in this view.</summary>
        public Action<View3D> Render3D = null;

        /// <summary>All lights known to this view.</summary>
        public List<LightObject> Lights = new List<LightObject>();

        /// <summary>Whether shadows are allowed to be rendered.</summary>
        public bool ShadowingAllowed = true;

        /// <summary>Render target width.</summary>
        public int Width;

        /// <summary>Render target height.</summary>
        public int Height;

        /// <summary>Current fog color.</summary>
        public Location FogCol = new Location(0.7);

        /// <summary>Minimum sun light value.</summary>
        public float SunLight_Minimum = 0;

        /// <summary>Maximum sun light value.</summary>
        public float SunLight_Maximum = 1;

        /// <summary>The alpha value to clear the lighting buffer to.</summary>
        public float LightsRenderClearAlpha = 1f;

        /// <summary>The shadow blur factor for deferred mode.</summary>
        public float ShadowBlur = 0.25f;

        /// <summary>The strength factor for depth-of-field if enabled.</summary>
        public float DOF_Factor = 4;

        /// <summary>The target location, if DOF is used.</summary>
        public Location DOF_Target = Location.Zero;

        /// <summary>The location of the sun when available.</summary>
        public Location SunLocation = Location.NaN;

        /// <summary>Maximum view distance of small lights.</summary>
        public double LightMaxDistance = 200f;

        /// <summary>Whether LinkedList Transparency tricks are enabled.</summary>
        public bool LLActive = false;

        /// <summary>The shadow texture size. Defaults to 64.</summary>
        public Func<int> ShadowTexSize = () => 64;

        /// <summary>Current fog alpha.</summary>
        public float FogAlpha = 0.0f;

        /// <summary>The current camera forward vector.</summary>
        public Location ForwardVec = Location.Zero;

        /// <summary>What color to clear the viewport to.</summary>
        public float[] ClearColor = new float[] { 0.2f, 1f, 1f, 1f };

        /// <summary>Ambient light.</summary>
        public Location Ambient;

        /// <summary>Desaturation amount to apply to the view.</summary>
        public float DesaturationAmount = 0f;

        /// <summary>Desaturation color to apply to the screen.</summary>
        public Vector3 DesaturationColor = new Vector3(0.95f, 0.77f, 0.55f);

        /// <summary>The camera's current position.</summary>
        public Location CameraPos;

        /// <summary>The camera's current 'target' location.</summary>
        public Location CameraTarget;

        /// <summary>Whether to enable godrays.</summary>
        public bool GodRays = true;

        /// <summary>Gets any camera rotation effects needed. Defaults to Identity.</summary>
        public Func<FGECore.MathHelpers.Quaternion> CameraModifier = () => FGECore.MathHelpers.Quaternion.Identity;

        /// <summary>The camera's up vector. Defaults to Z-Up (0,0,1).</summary>
        public Func<Location> CameraUp = () => Location.UnitZ;

        /// <summary>The maximum distance of lights.</summary>
        public double LightsMaxDistance = 1000;

        /// <summary>Change to set whether the system redraws shadows... defaults to always true.</summary>
        public Func<bool> ShouldRedrawShadows = () => true;

        /// <summary>Get and reset an indication of major updates that needs redrawing. Defaults to always give a true and reset nothing.</summary>
        public Func<bool> GetAndResetShouldMajorUpdates = () => true;

    }
}
