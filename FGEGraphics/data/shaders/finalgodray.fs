//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_GOOD_GRAPHICS 0
#define MCM_TOONIFY 0
#define MCM_MOTBLUR 0
#define MCM_LIGHTS 0
#define MCM_SPECIAL_FOG 0

layout (binding = 0) uniform sampler2D colortex; // Color G-Buffer Texture
layout (binding = 1) uniform sampler2D positiontex; // Positions G-Buffer Texture
layout (binding = 2) uniform sampler2D normaltex; // Normals G-Buffer Texture
layout (binding = 3) uniform sampler2D depthtex; // Depth G-Buffer Texture
layout (binding = 4) uniform sampler2D lighttex; // Lighting value from light passes
layout (binding = 5) uniform sampler2D renderhinttex; // Rendering hint data (y = blur)
layout (binding = 6) uniform sampler2D renderhint2tex; // More rendering hint data (Refract normal, or reflection value)

layout (location = 0) in vec2 f_texcoord; // The input texture coordinate (from the VS data).

// ...
layout (location = 8) uniform vec3 cameraTargetPos = vec3(0.0, 0.0, 0.0); // What position the camera is targeting in the world (ray traced).
layout (location = 9) uniform float cameraTargetDepth = 0.01; // How far away the camera target position is from the camera. (Useful for DOF effects). // TODO: ???
layout (location = 10) uniform float hdrExposure = 1.0; // The frame's HDR exposure value.
layout (location = 11) uniform float frameDelta = 0.05; // The current frame's delta. // TODO: ???
// ...
layout (location = 14) uniform vec3 cameraPos = vec3(0.0); // Camera position, relative to rendering origin.
// ...
layout (location = 16) uniform float fogDist = 1.0 / 100000.0; // The distance fog should be around.
layout (location = 17) uniform vec2 zdist = vec2(0.1, 1000.0); // The Z-Near and Z-Far value of the 3D projection.
layout (location = 18) uniform vec4 fogCol = vec4(0.0); // What color any fog to apply is. For no fog, the alpha value will be zero.
layout (location = 19) uniform float desaturationAmount = 0.0; // How much to desaturate the view by. 1.0 = fully desaturated.
layout (location = 20) uniform vec3 eye_position = vec3(0.0); // What position the eye of the 3D camera view is at in the world.
layout (location = 21) uniform vec3 desaturationColor = vec3(0.95, 0.77, 0.55); // What color to desaturate too. Default is an orange-ish color.
layout (location = 22) uniform mat4 proj_mat = mat4(1.0); // The full 3D projection matrix.
// ...
layout (location = 24) uniform float width = 1280.0; // How wide the screen is.
layout (location = 25) uniform float height = 720.0; // How tall the screen is.
layout (location = 26) uniform float time = 0.0; // A timer value, in seconds. Simply used for things that move.
layout (location = 27) uniform vec2 mot_blur = vec2(0.0); // How much motion blur to apply, and in what direction.
layout (location = 28) uniform float do_grayscale = 0.0; // Whether to gray-scale the view.

const float HDR_Mod = 5.0; // How much to multiply all lights by to ensure lighting colors are quality.
const float HDR_Div = (1.0 / HDR_Mod); // The inverse of HDR_Mod, for quick calculation.

layout (location = 0) out vec4 color; // The color to be rendered to screen.
layout (location = 1) out vec4 bloom; // The color of any bloom our pixel may produce, or zero if none.

float linearizeDepth(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * zdist.x) / (zdist.y + zdist.x - rinput * (zdist.y - zdist.x));
}

vec4 raytrace(in vec3 reflectionVector, in float startDepth) // Trace a ray across the screen, for reflection purposes.
{
	float stepSize = 0.01;
	reflectionVector = normalize(reflectionVector) * stepSize;
	vec2 sampledPosition = f_texcoord;
	float currentDepth = startDepth;
	while(sampledPosition.x <= 1.0 && sampledPosition.x >= 0.0 && sampledPosition.y <= 1.0 && sampledPosition.y >= 0.0)
	{
		sampledPosition = sampledPosition + reflectionVector.xy;
		currentDepth = currentDepth + reflectionVector.z * startDepth;
		float sampledDepth = linearizeDepth(texture(depthtex, sampledPosition).x);
		if(currentDepth > sampledDepth)
		{
			float delta = currentDepth - sampledDepth;
			if(delta < 0.03)
			{
				if (sampledDepth < startDepth)
				{
					return vec4(0.0);
				}
				else
				{
					return texture(colortex, sampledPosition);
				}
			}
		}
	}
	return vec4(0.0);
}

vec4 regularize(in vec4 input_r) // Limit the brightness of R/G/B values to 1.0 - the highest value is shrink to 1.0 and the rest scaled by the same value.
{
	 // TODO: Is this working the best it can?
	if (input_r.x <= 1.0 && input_r.y <= 1.0 && input_r.z <= 1.0) // If all values are less than or equal to 1.0, we don't need to limit brightness.
	{
		return input_r;
	}
	return vec4(input_r.xyz / max(max(input_r.x, input_r.y), input_r.z), input_r.w); // Otherwise, limit everything but the alpha.
}

vec3 desaturate(in vec3 c) // Desaturates color to be closer to the specified desaturationColor uniform.
{
	return c * (1.0 - desaturationAmount) + desaturationColor * dot(c, vec3(1.0)) * desaturationAmount; // Roughly equivalent to a mix call. (Mix doesn't work well on all cards for some reason.)
}

vec4 getColorInt(in vec2 pos, in float exposure) // Grab the color of a pixel, after lighting. Regularized.
{
#if MCM_LIGHTS
	return regularize(texture(lighttex, pos) * HDR_Div * exposure); // The light color, brought into standard range then multiplied by exposure.
#else
	return texture(colortex, pos); // The primary color of the object without lighting.
#endif
}

vec4 getColor(in vec2 pos, in float exposure, in float mblen) // Grab the color of a pixel, after lighting AND blurring.
{
	vec4 renderhint = texture(renderhinttex, pos);
	if (renderhint.y > 0.01) // If blurring is enabled.
	{
		// TODO: Better variation to the blur effect?
		float depthBasic = linearizeDepth(texture(depthtex, pos).x);
		float tried = 1.0;
		vec2 psx = vec2(0.0);
		while (tried > 0.1)
		{
			psx = normalize(pos - vec2(0.5 + cos(time + renderhint.y), 0.5 + sin(time + renderhint.y))) * 0.02 * tried;
			float depthNew = linearizeDepth(texture(depthtex, pos + psx).z);
			if (depthNew > depthBasic)
			{
				break;
			}
			tried *= 0.75;
		}
		return getColorInt(pos + psx, exposure) * 0.5 + getColorInt(pos, exposure) * 0.5;
	}
#if MCM_MOTBLUR
	vec4 bcol = vec4(0.0);
	float mblen_inv = 1.0 / mblen;
	float amt = 0.0;
	for (float f = 0.0; f <= mblen; f += 0.001)
	{
		vec4 ccol = getColorInt(pos + mot_blur * mblen_inv * f, exposure); // Just use the other function for getting the actual color data.
		float cur = ((mblen - f) * mblen_inv);
		bcol += ccol * cur;
		amt += cur;
	}
	return bcol * (1.0 / amt);
#else
	return getColorInt(pos, exposure);
#endif
}

// If TOONIFY is enabled, this section will contain all the toonify helper methods.
#if MCM_TOONIFY
#include include_toonify.inc
#endif

void main() // The central entry point of the shader. Handles everything!
{
	float mblen = dot(mot_blur, mot_blur) <= 0.0001 ? 0.0001 : length(mot_blur);
	vec4 light_color = vec4(getColor(f_texcoord, hdrExposure, mblen).xyz, 1.0); // Grab the basic color of our pixel.
	// This section applies toonify if it is enabled generally.
#if MCM_TOONIFY
	// TODO: Toonify option per pixel: block paint?
	vec3 vHSV = RGBtoHSV(light_color.x, light_color.y, light_color.z);
	vHSV.x = nearestLevel(vHSV.x, 0);
	vHSV.y = nearestLevel(vHSV.y, 1);
	vHSV.z = nearestLevel(vHSV.z, 2);
	float edg = IsEdge(f_texcoord, hdrExposure, mblen);
	vec3 vRGB = (edg >= edge_thres) ? vec3(0.0, 0.0, 0.0) : HSVtoRGB(vHSV.x, vHSV.y, vHSV.z);
	light_color = vec4(vRGB.x, vRGB.y, vRGB.z, light_color.w);
	// TODO: Maybe just return here?
#endif
	// Fancy effects are only available to quality graphics cards. Cut out quick if one's not available.
#if MCM_GOOD_GRAPHICS
	vec4 renderhintFull = texture(renderhinttex, f_texcoord);
	vec3 renderhint = renderhintFull.xyz;
	vec3 renderhint2 = texture(renderhint2tex, f_texcoord).xyz;
	vec3 pos = texture(positiontex, f_texcoord).xyz;
	float dist = linearizeDepth(texture(depthtex, f_texcoord).x); // This is useful for both fog and reflection, so grab it here.
	if ((renderhint.z < 0.99 && renderhint.z > 0.001) || fogCol.w > 1.0)
	{
		vec3 relPos = pos - cameraPos;
		float fog_distance = pow(dot(relPos, relPos) * fogDist, 0.6);
		float fogMod = min(fog_distance * exp(fogCol.w) * fogCol.w, 1.5);
		float fmz = min(fogMod, 1.0);
#if MCM_SPECIAL_FOG
		fmz *= fmz * fmz * fmz;
#endif
		light_color.xyz = light_color.xyz * (1.0 - fmz) + fogCol.xyz * fmz + vec3(fogMod - fmz);
	}
	if (dot(renderhint2, renderhint2) > 0.99) // Apply refraction if set. This is set by having a strong renderhint2 value that has a length-squared of at least 1.0!
	{
		vec3 viewDir = texture(positiontex, f_texcoord).xyz - eye_position;
		vec3 refr = refract(normalize(viewDir), normalize(renderhint2), 0.75);
		vec4 refrCol = getColor(f_texcoord + refr.xy * 0.1, hdrExposure, mblen);
		// TODO: Maybe apply a dynamic mixing value here, rather than static 0.5?
		light_color = light_color * 0.5 + refrCol * 0.5; // Color is half base value, and half refracted value.
	}
	else if (renderhint2.y > 0.01) // Apply (screen-space) reflection if set. This is set by having a renderhint2 green value greater than zero but less than one.
	{
		vec4 norm = texture(normaltex, f_texcoord);
		vec3 normal = normalize(norm.xyz);
		float currDepth = dist;
		vec3 eyePosition = normalize(eye_position - pos);
		vec4 reflectionVector = proj_mat * reflect(vec4(eyePosition, 0.0), vec4(normal, 0.0));
		vec4 SSR = raytrace(reflectionVector.xyz / reflectionVector.w, currDepth);
		if (SSR.w > 0.0)
		{
			float rhy = min(renderhint2.y, 1.0);
			light_color = light_color * (1.0 - rhy) + SSR * rhy; // If we found a reflection, apply it at the strength specified.
		}
	}
	light_color = vec4(desaturate(light_color.xyz), light_color.w); // Desaturate whatever color we've ended up with.
#endif
#if MCM_LIGHTS
	// HDR/bloom is available to all!
	vec3 basecol = texture(lighttex, f_texcoord).xyz * HDR_Div; // The base pixel color is our current pixel's color, without regularization.
	float val = max(max(basecol.x, basecol.y), basecol.z); // The brightest component of the base pixel color.
	float mod = min(max(val - hdrExposure * 4.0 + 3.0, 0.0), 1.0);
	bloom = vec4(basecol, mod * mod);
#else
	bloom = vec4(0.0);
#endif
	color = light_color; // Finally, 'return' (assign the base color value).
	if (do_grayscale > 0.5) // TODO: define rather than var?
	{
		// TODO: Add this effect to transparency shaders?
		color.xyz = vec3((light_color.x + light_color.y + light_color.z) * 0.3333);
	}
}
