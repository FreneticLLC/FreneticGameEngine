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
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.Shaders;

namespace FGEGraphics.ClientSystem;

/// <summary>Holds the default shader references for a GameEngine3D.</summary>
public class GE3DShaders
{
    /// <summary>Loads all shaders from the backing engine given.</summary>
    /// <param name="Shaders">The given backing shader engine.</param>
    /// <param name="AllowLL">Whether to allow and load the LL lighting helper.</param>
    /// <param name="forNorm">Whether to enable forward-mode normal effects.</param>
    /// <param name="forLight">Whether to enable forward-mode lighting.</param>
    /// <param name="forShad">Whether to enable forward-mode shadow effects.</param>
    public void LoadAll(ShaderEngine Shaders, bool AllowLL, bool forNorm, bool forLight, bool forShad)
    {
        string def = Shaders.MCM_GOOD_GRAPHICS ? "#MCM_GOOD_GRAPHICS" : "#";
        Deferred.ShadowPass_Basic = Shaders.GetShader("shadow" + def);
        Deferred.ShadowPass_NoBones = Shaders.GetShader("shadow" + def + ",MCM_NO_BONES");
        Deferred.GBufferSolid = Shaders.GetShader("fbo" + def);
        Deferred.GBuffer_SkyBox = Shaders.GetShader("fbo" + def + ",MCM_SKYBOX");
        Deferred.GBuffer_Refraction = Shaders.GetShader("fbo" + def + ",MCM_REFRACT");
        Deferred.ShadowAdderPass = Shaders.GetShader("lightadder" + def + ",MCM_SHADOWS");
        Deferred.LightAdderPass = Shaders.GetShader("lightadder" + def);
        Deferred.ShadowAdderPass_SSAO = Shaders.GetShader("lightadder" + def + ",MCM_SHADOWS,MCM_SSAO");
        Deferred.LightAdderPass_SSAO = Shaders.GetShader("lightadder" + def + ",MCM_SSAO");
        Deferred.Transparents = Shaders.GetShader("transponly" + def);
        Deferred.Transparents_Lights = Shaders.GetShader("transponly" + def + ",MCM_LIT");
        Deferred.Transparents_Lights_Shadows = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS");
        Deferred.Godrays = Shaders.GetShader("godray" + def);
        Deferred.TransparentAdderPass = Shaders.GetShader("transpadder" + def);
        Deferred.FinalPass_Basic = Shaders.GetShader("finalgodray" + def);
        Deferred.FinalPass_Toonify = Shaders.GetShader("finalgodray" + def + ",MCM_TOONIFY");
        Deferred.FinalPass_Lights = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS");
        Deferred.FinalPass_Lights_Toonify = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS,MCM_TOONIFY");
        Deferred.FinalPass_Lights_MotionBlur = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS,MCM_MOTBLUR");
        string forw_extra = (forNorm ? ",MCM_NORMALS" : "")
            + (forLight ? ",MCM_LIGHTS" : "")
            + (forShad ? ",MCM_SHADOWS" : "");
        Forward.BasicSolid = Shaders.GetShader("forward" + def + forw_extra);
        Forward.BasicSolid_NoBones = Shaders.GetShader("forward" + def + ",MCM_NO_BONES" + forw_extra);
        Forward.BasicTransparent = Shaders.GetShader("forward" + def + ",MCM_TRANSP" + forw_extra);
        Forward.BasicTransparent_NoBones = Shaders.GetShader("forward" + def + ",MCM_TRANSP,MCM_NO_BONES" + forw_extra);
        if (AllowLL)
        {
            Deferred.Transparents_LL = Shaders.GetShader("transponly" + def + ",MCM_LL");
            Deferred.Transparents_Lights_LL = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_LL");
            Deferred.Transparents_Lights_Shadows_LL = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS,MCM_LL");
            Deferred.LLClearerPass = Shaders.GetShader("clearer" + def);
            Deferred.LLFinalPass = Shaders.GetShader("fpass" + def);
        }
        Deferred.HDRPass = Shaders.GetShader("hdrpass" + def);
        Forward.PostProcess = Shaders.GetShader("postfast" + def);
        Deferred.ShadowPass_Particles = Shaders.GetShader("shadow" + def + ",MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_SHADOWS,MCM_NO_ALPHA_CAP,MCM_FADE_DEPTH,MCM_IS_A_SHADOW?particles");
        Forward.Particles = Shaders.GetShader("forward" + def + ",MCM_GEOM_ACTIVE,MCM_TRANSP,MCM_BRIGHT,MCM_NO_ALPHA_CAP,MCM_FADE_DEPTH" + forw_extra + "?particles");
        Forward.ParticlesNoFade = Shaders.GetShader("forward" + def + ",MCM_GEOM_ACTIVE,MCM_TRANSP,MCM_BRIGHT,MCM_NO_ALPHA_CAP" + forw_extra + "?particles");
        Deferred.GBuffer_Decals = Shaders.GetShader("fbo" + def + ",MCM_INVERSE_FADE,MCM_NO_ALPHA_CAP,MCM_GEOM_ACTIVE,MCM_PRETTY?decal");
        Forward.Decals = Shaders.GetShader("forward" + def + ",MCM_INVERSE_FADE,MCM_NO_ALPHA_CAP,MCM_GEOM_ACTIVE" + forw_extra + "?decal");
        Forward.AllTransparencies_NoFog = Shaders.GetShader("forward" + def + ",MCM_NO_ALPHA_CAP,MCM_BRIGHT,MCM_NO_BONES" + forw_extra);
        Forward.AllTransparencies_Objects = Shaders.GetShader("forward" + def + ",MCM_NO_BONES" + forw_extra);
        Forward.AllTransparencies_Sky = Shaders.GetShader("forward" + def + ",MCM_NO_ALPHA_CAP,MCM_BRIGHT,MCM_NO_BONES,MCM_SKY_FOG" + forw_extra);
        Deferred.Solid_Particles = Shaders.GetShader("fbo" + def + ",MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH,MCM_NO_ALPHA_CAP?particles");
        Deferred.Transparents_Particles = Shaders.GetShader("transponly" + def + ",MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
        Deferred.Transparents_Particles_Lights = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
        Deferred.Transparents_Particles_Lights_Shadows = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
        if (AllowLL)
        {
            Deferred.Transparents_Particles_LL = Shaders.GetShader("transponly" + def + ",MCM_LL,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
            Deferred.Transparents_Particles_Lights_LL = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_LL,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
            Deferred.Transparents_Particles_Lights_Shadows_LL = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS,MCM_LL,MCM_ANY,MCM_GEOM_ACTIVE,MCM_PRETTY,MCM_FADE_DEPTH?particles");
        }
    }

    /// <summary>Shader objects for forward rendering.</summary>
    public struct ForwardShaders
    {
        /// <summary>The shader used for forward ('fast') rendering of data.</summary>
        public Shader BasicSolid;

        /// <summary>The shader used for forward ('fast') rendering of data, with no bones.</summary>
        public Shader BasicSolid_NoBones;

        /// <summary>The shader used for forward ('fast') rendering of transparent data.</summary>
        public Shader BasicTransparent;

        /// <summary>The shader for forward ('fast') post-processing calculations.</summary>
        public Shader PostProcess;

        /// <summary>The shader used for forward ('fast') rendering of transparent data, with no bones.</summary>
        public Shader BasicTransparent_NoBones;

        /// <summary>The shader used for particles in forward rendering mode.</summary>
        public Shader Particles;

        /// <summary>The shader used for particles in forward rendering mode, without 'depth fade' logic.</summary>
        public Shader ParticlesNoFade;

        /// <summary>The shader used for decal rendering in forward mode.</summary>
        public Shader Decals;

        /// <summary>The shader used for all-transparency rendering in forward mode (specifically not the skybox itself).</summary>
        public Shader AllTransparencies_Objects;

        /// <summary>The shader used for all-transparency rendering in forward mode (specifically those objects that shouldn't have fog).</summary>
        public Shader AllTransparencies_NoFog;

        /// <summary>The shader used for all-transparency rendering in forward mode (primarily the skybox).</summary>
        public Shader AllTransparencies_Sky;
    }

    /// <summary>Shader objects for forward rendering.</summary>
    public ForwardShaders Forward;

    /// <summary>Shader objects for deferred rendering.</summary>
    public struct DeferredShaders
    {
        /// <summary>The Shadow Pass shader.</summary>
        public Shader ShadowPass_Basic;

        /// <summary>The Shadow Pass shader, with bones off.</summary>
        public Shader ShadowPass_NoBones;

        /// <summary>The Shadow Pass shader, for particles.</summary>
        public Shader ShadowPass_Particles;

        /// <summary>The final write + godrays shader.</summary>
        public Shader FinalPass_Basic;

        /// <summary>The final write + godrays shader, with lights on.</summary>
        public Shader FinalPass_Lights;

        /// <summary>The final write + godrays shader, with toonify on.</summary>
        public Shader FinalPass_Toonify;

        /// <summary>The final write + godrays shader, with lights and toonify on.</summary>
        public Shader FinalPass_Lights_Toonify;

        /// <summary>The final write + godrays shader, with lights and motion blur on.</summary>
        public Shader FinalPass_Lights_MotionBlur;

        /// <summary>The G-Buffer FBO shader.</summary>
        public Shader GBufferSolid;

        /// <summary>The G-Buffer FBO shader, for alltransparents (Skybox mainly).</summary>
        public Shader GBuffer_SkyBox;

        /// <summary>The G-Buffer FBO shader, for the refraction pass.</summary>
        public Shader GBuffer_Refraction;

        /// <summary>The shader that adds shadowed lights to a scene.</summary>
        public Shader ShadowAdderPass;

        /// <summary>The shader that adds lights to a scene.</summary>
        public Shader LightAdderPass;

        /// <summary>The shader that adds shadowed lights to a scene, with SSAO.</summary>
        public Shader ShadowAdderPass_SSAO;

        /// <summary>The shader that adds lights to a scene, with SSAO.</summary>
        public Shader LightAdderPass_SSAO;

        /// <summary>The shader used only for transparent data.</summary>
        public Shader Transparents;

        /// <summary>The shader used only for transparent data with lighting.</summary>
        public Shader Transparents_Lights;

        /// <summary>The shader used only for transparent data with shadowed lighting.</summary>
        public Shader Transparents_Lights_Shadows;

        /// <summary>The shader used for calculating godrays.</summary>
        public Shader Godrays;

        /// <summary>
        /// The shader used as the final step of adding transparent data to the scene.
        /// TODO: Optimize this away.
        /// </summary>
        public Shader TransparentAdderPass;

        /// <summary>The shader used for transparent data (LinkedList Transparency version).</summary>
        public Shader Transparents_LL;

        /// <summary>The shader used for lit transparent data (LinkedList Transparency version).</summary>
        public Shader Transparents_Lights_LL;

        /// <summary>The shader used for shadowed lit transparent data (LinkedList Transparency version).</summary>
        public Shader Transparents_Lights_Shadows_LL;

        /// <summary>The shader used to clear LL data.</summary>
        public Shader LLClearerPass;

        /// <summary>The shader used to finally apply LL data.</summary>
        public Shader LLFinalPass;

        /// <summary>The shader used to assist in HDR calculation acceleration.</summary>
        public Shader HDRPass;

        /// <summary>The shader used only for g-buffer particles.</summary>
        public Shader Solid_Particles;

        /// <summary>The shader used only for transparent particles.</summary>
        public Shader Transparents_Particles;

        /// <summary>The shader used only for transparent particles with lighting.</summary>
        public Shader Transparents_Particles_Lights;

        /// <summary>The shader used only for transparent particles with shadowed lighting.</summary>
        public Shader Transparents_Particles_Lights_Shadows;

        /// <summary>The shader used for transparent particles (LinkedList Transparency version).</summary>
        public Shader Transparents_Particles_LL;

        /// <summary>The shader used for lit transparent particles (LinkedList Transparency version).</summary>
        public Shader Transparents_Particles_Lights_LL;

        /// <summary>The shader used for shadowed lit transparent particles (LinkedList Transparency version).</summary>
        public Shader Transparents_Particles_Lights_Shadows_LL;

        /// <summary>The shader used for decal rendering in deferred rendering mode.</summary>
        public Shader GBuffer_Decals;
    }

    /// <summary>Shader objects for deferred rendering.</summary>
    public DeferredShaders Deferred;
}
