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
using FreneticGameGraphics.GraphicsHelpers;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Holds the default shader references for a GameEngine3D.
    /// </summary>
    public class GE3DShaders
    {
        /// <summary>
        /// Loads all shaders from the backing engine given.
        /// </summary>
        /// <param name="Shaders">The given backing shader engine.</param>
        /// <param name="AllowLL">Whether to allow and load the LL lighting helper.</param>
        /// <param name="forNorm">Whether to enable forward-mode normal effects.</param>
        /// <param name="forLight">Whether to enable forward-mode lighting.</param>
        /// <param name="forShad">Whether to enable forward-mode shadow effects.</param>
        public void LoadAll(ShaderEngine Shaders, bool AllowLL, bool forNorm, bool forLight, bool forShad)
        {
            string def = Shaders.MCM_GOOD_GRAPHICS ? "#MCM_GOOD_GRAPHICS" : "#";
            Deferred.s_shadow = Shaders.GetShader("shadow" + def);
            Deferred.s_shadow_nobones = Shaders.GetShader("shadow" + def + ",MCM_NO_BONES");
            Deferred.s_fbo = Shaders.GetShader("fbo" + def);
            Deferred.s_fbot = Shaders.GetShader("fbo" + def + ",MCM_TRANSP_ALLOWED");
            Deferred.s_fbo_refract = Shaders.GetShader("fbo" + def + ",MCM_REFRACT");
            Deferred.s_shadowadder = Shaders.GetShader("lightadder" + def + ",MCM_SHADOWS");
            Deferred.s_lightadder = Shaders.GetShader("lightadder" + def);
            Deferred.s_shadowadder_ssao = Shaders.GetShader("lightadder" + def + ",MCM_SHADOWS,MCM_SSAO");
            Deferred.s_lightadder_ssao = Shaders.GetShader("lightadder" + def + ",MCM_SSAO");
            Deferred.s_transponly = Shaders.GetShader("transponly" + def);
            Deferred.s_transponlylit = Shaders.GetShader("transponly" + def + ",MCM_LIT");
            Deferred.s_transponlylitsh = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS");
            Deferred.s_godray = Shaders.GetShader("godray" + def);
            Deferred.s_transpadder = Shaders.GetShader("transpadder" + def);
            Deferred.s_finalgodray = Shaders.GetShader("finalgodray" + def);
            Deferred.s_finalgodray_toonify = Shaders.GetShader("finalgodray" + def + ",MCM_TOONIFY");
            Deferred.s_finalgodray_lights = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS");
            Deferred.s_finalgodray_lights_toonify = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS,MCM_TOONIFY");
            Deferred.s_finalgodray_lights_motblur = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS,MCM_MOTBLUR");
            string forw_extra = (forNorm ? ",MCM_NORMALS" : "")
                + (forLight ? ",MCM_LIGHTS" : "")
                + (forShad ? ",MCM_SHADOWS" : "");
            Forward.BasicSolid = Shaders.GetShader("forward" + def + forw_extra);
            Forward.BasicSolid_NoBones = Shaders.GetShader("forward" + def + ",MCM_NO_BONES" + forw_extra);
            Forward.BasicTransparent = Shaders.GetShader("forward" + def + ",MCM_TRANSP" + forw_extra);
            Forward.BasicTransparent_NoBones = Shaders.GetShader("forward" + def + ",MCM_TRANSP,MCM_NO_BONES" + forw_extra);
            if (AllowLL)
            {
                Deferred.s_transponly_ll = Shaders.GetShader("transponly" + def + ",MCM_LL");
                Deferred.s_transponlylit_ll = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_LL");
                Deferred.s_transponlylitsh_ll = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS,MCM_LL");
                Deferred.s_ll_clearer = Shaders.GetShader("clearer" + def);
                Deferred.s_ll_fpass = Shaders.GetShader("fpass" + def);
            }
            Deferred.s_hdrpass = Shaders.GetShader("hdrpass" + def);
            Forward.PostProcess = Shaders.GetShader("postfast" + def);
            Forward.Grass = Shaders.GetShader("forward" + def + ",MCM_GEOM_ACTIVE,MCM_GEOM_THREED_TEXTURE" + forw_extra + "?grass");
            Deferred.s_fbo_grass = Shaders.GetShader("fbo" + def + ",MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_GEOM_THREED_TEXTURE?grass");
            Deferred.s_shadow_grass = Shaders.GetShader("shadow" + def + ",MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_SHADOWS,MCM_IS_A_SHADOW,MCM_GEOM_THREED_TEXTURE?grass");
            Deferred.s_shadow_parts = Shaders.GetShader("shadow" + def + ",MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_SHADOWS,MCM_NO_ALPHA_CAP,MCM_FADE_DEPTH,MCM_IS_A_SHADOW?particles");
            Forward.Particles = Shaders.GetShader("forward" + def + ",MCM_GEOM_ACTIVE,MCM_TRANSP,MCM_BRIGHT,MCM_NO_ALPHA_CAP,MCM_FADE_DEPTH" + forw_extra + "?particles");
            Deferred.s_fbodecal = Shaders.GetShader("fbo" + def + ",MCM_INVERSE_FADE,MCM_NO_ALPHA_CAP,MCM_GEOM_ACTIVE,MCM_PRETTY?decal");
            Forward.Decals = Shaders.GetShader("forward" + def + ",MCM_INVERSE_FADE,MCM_NO_ALPHA_CAP,MCM_GEOM_ACTIVE" + forw_extra + "?decal");
            Forward.AllTransparencies_NoFog = Shaders.GetShader("forward" + def + ",MCM_NO_ALPHA_CAP,MCM_BRIGHT,MCM_NO_BONES" + forw_extra);
            Forward.AllTransparencies_Objects = Shaders.GetShader("forward" + def + ",MCM_NO_BONES" + forw_extra);
            Forward.AllTransparencies_Sky = Shaders.GetShader("forward" + def + ",MCM_NO_ALPHA_CAP,MCM_BRIGHT,MCM_NO_BONES,MCM_SKY_FOG" + forw_extra);
            Deferred.s_transponly_particles = Shaders.GetShader("transponly" + def + ",MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
            Deferred.s_transponlylit_particles = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
            Deferred.s_transponlylitsh_particles = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
            if (AllowLL)
            {
                Deferred.s_transponly_ll_particles = Shaders.GetShader("transponly" + def + ",MCM_LL,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
                Deferred.s_transponlylit_ll_particles = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_LL,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
                Deferred.s_transponlylitsh_ll_particles = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS,MCM_LL,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
            }
        }

        /// <summary>
        /// Shader objects for forward rendering.
        /// </summary>
        public struct ForwardShaders
        {
            /// <summary>
            /// The shader used for forward ('fast') rendering of data.
            /// </summary>
            public Shader BasicSolid;

            /// <summary>
            /// The shader used for forward ('fast') rendering of data, with no bones.
            /// </summary>
            public Shader BasicSolid_NoBones;

            /// <summary>
            /// The shader used for forward ('fast') rendering of transparent data.
            /// </summary>
            public Shader BasicTransparent;

            /// <summary>
            /// The shader for forward ('fast') post-processing calculations.
            /// </summary>
            public Shader PostProcess;

            /// <summary>
            /// The shader used for forward ('fast') rendering of transparent data, with no bones.
            /// </summary>
            public Shader BasicTransparent_NoBones;

            /// <summary>
            /// The shader used for grass-sprites in forward rendering mode.
            /// </summary>
            public Shader Grass;

            /// <summary>
            /// The shader used for particles in forward rendering mode.
            /// </summary>
            public Shader Particles;

            /// <summary>
            /// The shader used for decal rendering in forward mode.
            /// </summary>
            public Shader Decals;

            /// <summary>
            /// The shader used for all-transparency rendering in forward mode (specifically not the skybox itself).
            /// </summary>
            public Shader AllTransparencies_Objects;

            /// <summary>
            /// The shader used for all-transparency rendering in forward mode (specifically those objects that shouldn't have fog).
            /// </summary>
            public Shader AllTransparencies_NoFog;

            /// <summary>
            /// The shader used for all-transparency rendering in forward mode (primarily the skybox).
            /// </summary>
            public Shader AllTransparencies_Sky;
        }

        /// <summary>
        /// Shader objects for forward rendering.
        /// </summary>
        public ForwardShaders Forward;

        /// <summary>
        /// Shader objects for deferred rendering.
        /// </summary>
        public struct DeferredShaders
        {
            /// <summary>
            /// The Shadow Pass shader.
            /// </summary>
            public Shader s_shadow;

            /// <summary>
            /// The Shadow Pass shader, with bones off.
            /// </summary>
            public Shader s_shadow_nobones;

            /// <summary>
            /// The Shadow Pass shader, for grass.
            /// </summary>
            public Shader s_shadow_grass;

            /// <summary>
            /// The Shadow Pass shader, for particles.
            /// </summary>
            public Shader s_shadow_parts;

            /// <summary>
            /// The final write + godrays shader.
            /// </summary>
            public Shader s_finalgodray;

            /// <summary>
            /// The final write + godrays shader, with lights on.
            /// </summary>
            public Shader s_finalgodray_lights;

            /// <summary>
            /// The final write + godrays shader, with toonify on.
            /// </summary>
            public Shader s_finalgodray_toonify;

            /// <summary>
            /// The final write + godrays shader, with lights and toonify on.
            /// </summary>
            public Shader s_finalgodray_lights_toonify;

            /// <summary>
            /// The final write + godrays shader, with lights and motion blur on.
            /// </summary>
            public Shader s_finalgodray_lights_motblur;

            /// <summary>
            /// The G-Buffer FBO shader.
            /// </summary>
            public Shader s_fbo;

            /// <summary>
            /// The G-Buffer FBO shader, for alltransparents (Skybox mainly).
            /// </summary>
            public Shader s_fbot;

            /// <summary>
            /// The shader used for grass-sprites in deferred rendering mode.
            /// </summary>
            public Shader s_fbo_grass;

            /// <summary>
            /// The G-Buffer FBO shader, for the refraction pass.
            /// </summary>
            public Shader s_fbo_refract;

            /// <summary>
            /// The shader that adds shadowed lights to a scene.
            /// </summary>
            public Shader s_shadowadder;

            /// <summary>
            /// The shader that adds lights to a scene.
            /// </summary>
            public Shader s_lightadder;

            /// <summary>
            /// The shader that adds shadowed lights to a scene, with SSAO.
            /// </summary>
            public Shader s_shadowadder_ssao;

            /// <summary>
            /// The shader that adds lights to a scene, with SSAO.
            /// </summary>
            public Shader s_lightadder_ssao;

            /// <summary>
            /// The shader used only for transparent data.
            /// </summary>
            public Shader s_transponly;

            /// <summary>
            /// The shader used only for transparent data with lighting.
            /// </summary>
            public Shader s_transponlylit;

            /// <summary>
            /// The shader used only for transparent data with shadowed lighting.
            /// </summary>
            public Shader s_transponlylitsh;

            /// <summary>
            /// The shader used for calculating godrays.
            /// </summary>
            public Shader s_godray;

            /// <summary>
            /// The shader used as the final step of adding transparent data to the scene.
            /// TODO: Optimize this away.
            /// </summary>
            public Shader s_transpadder;

            /// <summary>
            /// The shader used for transparent data (LinkedList Transparency version).
            /// </summary>
            public Shader s_transponly_ll;

            /// <summary>
            /// The shader used for lit transparent data (LinkedList Transparency version).
            /// </summary>
            public Shader s_transponlylit_ll;

            /// <summary>
            /// The shader used for shadowed lit transparent data (LinkedList Transparency version).
            /// </summary>
            public Shader s_transponlylitsh_ll;

            /// <summary>
            /// The shader used to clear LL data.
            /// </summary>
            public Shader s_ll_clearer;

            /// <summary>
            /// The shader used to finally apply LL data.
            /// </summary>
            public Shader s_ll_fpass;

            /// <summary>
            /// The shader used to assist in HDR calculation acceleration.
            /// </summary>
            public Shader s_hdrpass;

            /// <summary>
            /// The shader used only for transparent particles.
            /// </summary>
            public Shader s_transponly_particles;

            /// <summary>
            /// The shader used only for transparent particles with lighting.
            /// </summary>
            public Shader s_transponlylit_particles;

            /// <summary>
            /// The shader used only for transparent particles with shadowed lighting.
            /// </summary>
            public Shader s_transponlylitsh_particles;

            /// <summary>
            /// The shader used for transparent particles (LinkedList Transparency version).
            /// </summary>
            public Shader s_transponly_ll_particles;

            /// <summary>
            /// The shader used for lit transparent particles (LinkedList Transparency version).
            /// </summary>
            public Shader s_transponlylit_ll_particles;

            /// <summary>
            /// The shader used for shadowed lit transparent particles (LinkedList Transparency version).
            /// </summary>
            public Shader s_transponlylitsh_ll_particles;

            /// <summary>
            /// The shader used for decal rendering in deferred rendering mode.
            /// </summary>
            public Shader s_fbodecal;
        }

        /// <summary>
        /// Shader objects for deferred rendering.
        /// </summary>
        public DeferredShaders Deferred;
    }
}
