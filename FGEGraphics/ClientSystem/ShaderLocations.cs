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

namespace FGEGraphics.ClientSystem;

/// <summary>Represents shader target locations.</summary>
public static class ShaderLocations
{
    /// <summary>Locations shared by most shaders (3D mainly).</summary>
    public static class Common
    {
        /// <summary>The general screen projection and view together.</summary>
        public const int PROJECTION = 1;

        /// <summary>The general world offset.</summary>
        public const int WORLD = 2;

        /// <summary>Camera position.</summary>
        public const int CAMERA_POSITION = 14;

        /// <summary>The screen size.</summary>
        public const int SCREEN_SIZE = 4;
    }

    /// <summary>Locations shared by most 2D shaders.</summary>
    public static class Common2D
    {
        /// <summary>The scaler value.</summary>
        public const int SCALER = 1;

        /// <summary>The adder value.</summary>
        public const int ADDER = 2;

        /// <summary>The color multiplier to add.</summary>
        public const int COLOR = 3;

        /// <summary>The rotation effect to apply.</summary>
        public const int ROTATION = 4;
    }

    /// <summary>Locations used in Forward Rendering mode.</summary>
    public static class Forward
    {
        // TODO
    }

    /// <summary>Locations used in Deferred Rendering mode.</summary>
    public static class Deferred
    {
        /// <summary>Locations used in the shadow shader.</summary>
        public static class Shadow
        {
            /// <summary>
            /// Whether to allow transparency in the shadow.
            /// Uniform: allow_transp
            /// </summary>
            public const int ALLOW_TRANSPARENCY = 4;

            /// <summary>
            /// Whether to use a square root operation to adjust if the sub light is an orthographic light.
            /// Uniform: should_sqrt
            /// </summary>
            public const int SHOULD_SQRT = 5;
        }

        /// <summary>Locations used in the gbuffer shader (Shader name is FBO).</summary>
        public static class GBuffer
        {
            /// <summary>The screen size. Used for depth linearization.</summary>
            public const int SCREEN_SIZE = 4;

            /// <summary>Used in color_for for getting the color of a vertex based on time.</summary>
            public const int TIME = 6;

            /// <summary>The fog color including alpha.</summary>
            public const int FogColor = 18;
        }

        /// <summary>Locations used in the lightadder shader.</summary>
        public static class LightAdder
        {
            /// <summary>How much to blur the shadow (gives a soft shadow effect).</summary>
            public const int DEPTH_JUMP = 3;

            /// <summary>How much ambient light to add.</summary>
            public const int AMBIENT = 4;

            /// <summary>Z-near and Z-far for depth linearization.</summary>
            public const int Z_DISTANCE = 7;

            /// <summary>Current projection for SSAO (Screen Space Ambient Occlusion) usage.</summary>
            public const int SSAO_PROJECTION = 8;

            /// <summary>How many lights are present.</summary>
            public const int LIGHTS_USED = 9;

            /// <summary>The matrices of the light sources (essentially their point of view this is used for shadow mapping).</summary>
            public const int SHADOW_MATRIX_ARRAY = 10;
        }

        /// <summary>Locations used in the final pass shader.</summary>
        public static class FinalPass
        {
            /// <summary>What position the camera is targeting in the world (ray traced).</summary>
            public const int CAMERA_TARGET_POSITION = 8;

            /// <summary>How far away the camera target position is from the camera.</summary>
            public const int CAMERA_TARGET_DEPTH = 9;

            /// <summary>The HDR exposure value.</summary>
            public const int HDR_EXPOSURE = 10;

            /// <summary>Camera position.</summary>
            public const int CAMERA_POSITION = 14;

            /// <summary>The distance fog should be around.</summary>
            public const int FOG_DISTANCE = 16;

            /// <summary>The Z-Near and Z-Far value of the 3D projection.</summary>
            public const int Z_DISTANCE = 17;

            /// <summary>What color any fog to apply is. For no fog, the alpha value will be zero.</summary>
            public const int FOG_COLOR = 18;

            /// <summary>How much to desaturate the view by. 1.0 = fully desaturated.</summary>
            public const int DESATURATION = 19;

            /// <summary>
            /// What position the eye of the 3D camera view is at in the world.
            /// Important for reflection calculations by normalizing the fragment position subtracted by this value in order to get the view direction.
            /// </summary>
            public const int EYE_POSITION = 20;

            /// <summary>What color to desaturate too. Default is an orange-ish color.</summary>
            public const int DESATURATION_COLOR = 21;

            /// <summary>The full 3D projection matrix.</summary>
            public const int PROJECTION_MATRIX = 22;

            /// <summary>How wide the screen is.</summary>
            public const int WIDTH = 24;

            /// <summary>How tall the screen is.</summary>
            public const int HEIGHT = 25;

            /// <summary>Passes in the engine global tick time to the shader.</summary>
            public const int TIME = 26;

            /// <summary>How much motion blur to apply, and in what direction.</summary>
            public const int MOTION_BLUR = 27;

            /// <summary>Whether to gray-scale the view. 0 for off, 1 for on.</summary>
            public const int DO_GRAYSCALE = 28;
        }

        /// <summary>Locations used in the hdrpass shader.</summary>
        public static class HDRPass
        {
            /// <summary>The screen size.</summary>
            public const int SCREEN_SIZE = 4;
        }

        /// <summary>Locations used in the godray shader.</summary>
        public static class Godray
        {
            /// <summary>The amount of exposure to apply.</summary>
            public const int EXPOSURE = 6;

            /// <summary>The aspect ratio of the screen to ensure the godray effect is properly scaled.</summary>
            public const int ASPECT_RATIO = 7;

            /// <summary>The position of the light source (sun location).</summary>
            public const int SUN_LOCATION = 8;

            /// <summary>The density of the godray effect - Higher values make the godray effect more dense and focused while lower values make it more spread out.</summary>
            public const int DENSITY = 12;

            /// <summary>The minimum depth, used for depth linearization.</summary>
            public const int MIN_DEPTH = 14;

            /// <summary>The maximum depth, used for depth linearization.</summary>
            public const int MAX_DEPTH = 15;

            /// <summary>Distance that determines whether a pixel is in the sky or not.</summary>
            public const int SKY_DISTANCE = 16;
        }

        /// <summary>Locations used in the transparents-only shader.</summary>
        public static class TranspOnly
        {
            /// <summary>Amount of desaturation to apply.</summary>
            public const int DESATURATION_AMOUNT = 4;

            /// <summary>Screen size.</summary>
            public const int SCREEN_SIZE = 8;

            /// <summary>Data matrix for light data to be used in the shader.</summary>
            public const int LIGHTS_USED_HELPER = 9;

            /// <summary>The matrices of the light sources (essentially their point of view this is used for shadow mapping).</summary>
            public const int SHADOW_MATRIX_ARRAY = 20;

            /// <summary>The distance fog should be around.</summary>
            public const int FOG_DISTANCE = 13;

            /// <summary>Camera position.</summary>
            public const int CAMERA_POSITION = 14;
        }

        /// <summary>Locations used in the transparents data adder shader.</summary>
        public static class TranspAdder
        {
            /// <summary>The amount of lights used.</summary>
            public const int LIGHT_COUNT = 3;
        }
    }
}
