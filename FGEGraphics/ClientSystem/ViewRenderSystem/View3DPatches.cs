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
using OpenTK.Mathematics;
using FGEGraphics.LightingSystem;

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>
    /// Path methods to be called by <see cref="View3DForwardRenderer"/>.
    /// </summary>
    public class View3DPatchesForward
    {
        /// <summary>
        /// Executable view patch: pre-solid-render shader setup.
        /// </summary>
        public Action<float[], float[], float, Vector3, int> PreSolidPatch;

        /// <summary>
        /// Executable view patch: solid VR shader setup.
        /// </summary>
        public Action VRSolidPatch;

        /// <summary>
        /// Executable view patch: pre-transparent-render shader setup.
        /// </summary>
        public Action<float, float[], float[], int> PreTransparentPatch;

        /// <summary>
        /// Executable view patch: transparent VR shader setup.
        /// </summary>
        public Action VRTransparentPatch;

        /// <summary>
        /// Executable view patch: end of Forward rendering.
        /// </summary>
        public Action EndPatch;
    }

    /// <summary>
    /// Path methods to be called by <see cref="View3DDeferredRenderer"/>.
    /// </summary>
    public class View3DPatchesDeferred
    {

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action PreShadowsPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<LightObject, Light> ShadowLightPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action PreFBOPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action VRFBOPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action RefractionPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action VRRefractionPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<float> PreTransparentPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action VRTransparentPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<Matrix4, float[], float[]> TransparentLightPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<Matrix4> LLPatch;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action TransparentRenderPatch;
    }
}
