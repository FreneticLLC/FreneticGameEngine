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

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{

    /// <summary>
    /// Helper for current rendering mode ID.
    /// TODO: Replace this with <see cref="RenderTargetType"/>.
    /// </summary>
    public enum FBOID : byte
    {
        /// <summary>
        /// No relevant mode.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Main mode (FBO).
        /// </summary>
        MAIN = 1,
        /// <summary>
        /// Main mode (Extras: decals).
        /// </summary>
        MAIN_EXTRAS = 2,
        /// <summary>
        /// Transparency, no lights.
        /// </summary>
        TRANSP_UNLIT = 3,
        /// <summary>
        /// Shadows.
        /// </summary>
        SHADOWS = 4,
        /// <summary>
        /// Static shadows.
        /// </summary>
        STATIC_SHADOWS = 5,
        /// <summary>
        /// Dynamic shadows.
        /// </summary>
        DYNAMIC_SHADOWS = 6,
        /// <summary>
        /// Transparency (lights).
        /// </summary>
        TRANSP_LIT = 7,
        /// <summary>
        /// Transparency (lights and shadows).
        /// </summary>
        TRANSP_SHADOWS = 8,
        /// <summary>
        /// Transparency (LL).
        /// </summary>
        TRANSP_LL = 12,
        /// <summary>
        /// Transparency (lights and LL).
        /// </summary>
        TRANSP_LIT_LL = 13,
        /// <summary>
        /// Transparency (lights and shadows and LL).
        /// </summary>
        TRANSP_SHADOWS_LL = 14,
        /// <summary>
        /// Refraction helper.
        /// </summary>
        REFRACT = 21,
        /// <summary>
        /// Forward extras (decals).
        /// </summary>
        FORWARD_EXTRAS = 97,
        /// <summary>
        /// Forward transparency.
        /// </summary>
        FORWARD_TRANSP = 98,
        /// <summary>
        /// Forward opaque.
        /// </summary>
        FORWARD_SOLID = 99,
    }

    /// <summary>
    /// Helpers for <see cref="FBOID"/>.
    /// </summary>
    public static class FBOIDExtensions
    {
        /// <summary>
        /// Checks if the ID is the 'main + transparent' modes.
        /// </summary>
        /// <param name="id">The ID.</param>
        public static bool IsMainTransp(this FBOID id)
        {
            return id == FBOID.TRANSP_LIT || id == FBOID.TRANSP_LIT_LL || id == FBOID.TRANSP_LL || id == FBOID.TRANSP_SHADOWS || id == FBOID.TRANSP_SHADOWS_LL || id == FBOID.TRANSP_UNLIT;
        }

        /// <summary>
        /// Checks if the ID is the 'main + opaque' modes.
        /// </summary>
        public static bool IsMainSolid(this FBOID id)
        {
            return id == FBOID.FORWARD_SOLID || id == FBOID.MAIN;
        }

        /// <summary>
        /// Checks if the ID is the 'solid (opaque)' modes.
        /// </summary>
        public static bool IsSolid(this FBOID id)
        {
            return id == FBOID.SHADOWS || id == FBOID.STATIC_SHADOWS || id == FBOID.DYNAMIC_SHADOWS || id == FBOID.FORWARD_SOLID || id == FBOID.REFRACT || id == FBOID.MAIN;
        }

        /// <summary>
        /// Checks if the ID is the 'forward' modes.
        /// </summary>
        public static bool IsForward(this FBOID id)
        {
            return id == FBOID.FORWARD_SOLID || id == FBOID.FORWARD_TRANSP || id == FBOID.FORWARD_EXTRAS;
        }
    }
}
