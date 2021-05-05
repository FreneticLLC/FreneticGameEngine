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
    /// Contextual data about what type of target is currently being rendered to, for <see cref="View3D"/>.
    /// TODO: Replace <see cref="FBOID"/> with this.
    /// </summary>
    public class RenderTargetType
    {
        /// <summary>
        /// If this is true, the target is for a deferred shading mode.
        /// If false, the target is for a forward ("fast") shading mode.
        /// <para>Generally, deferred is higher quality, while forward is higher performance.</para>
        /// </summary>
        public bool IsDeferred;

        /// <summary>
        /// If this is true, the target is rendering the standard 3D world the way it generally is.
        /// If false, the target is something unique/special, such as a shadow map.
        /// </summary>
        public bool IsPrimary;

        /// <summary>
        /// If this is true, the target includes transparent pixels.
        /// If false, any transparent pixels will be discarded.
        /// </summary>
        public bool AllowsTransparent;

        /// <summary>
        /// If this is true, the target includes opaque pixels.
        /// If false, any opaque pixels will be discarded.
        /// </summary>
        public bool AllowsOpaque;

        /// <summary>
        /// If this is true, the target is building a shadow map for a dynamic light.
        /// If false, some other target type is in use, such as a primary render.
        /// </summary>
        public bool IsShadowMap;

        /// <summary>
        /// If this is true, and see <see cref="IsShadowMap"/> is true, the target is specifically static shadow maps.
        /// If this is false, but <see cref="IsShadowMap"/> is still true, the target is dynamic shadow maps.
        /// </summary>
        public bool IsStaticShadowMap;

        /// <summary>
        /// if this is true, the target is building refraction data.
        /// </summary>
        public bool IsRefractionPass;

        /// <summary>
        /// If this is true, the target is building decal data.
        /// </summary>
        public bool IsDecalsPass;

        /// <summary>
        /// If this is true, lighting is explicitly included in the current render target.
        /// If false, lighting is either excluded or simply not relevant.
        /// </summary>
        public bool IsLit;

        /// <summary>
        /// If this is true, and <see cref="IsLit"/> is true, the target is processing lighting in a way that includes application of shadow maps.
        /// </summary>
        public bool HasShadows;
        
        /// <summary>
        /// If this is true, extra detail linked-list transparency logic is applied.
        /// </summary>
        public bool UsesLLTransparency;
    }
}
