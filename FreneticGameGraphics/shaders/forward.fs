//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_GOOD_GRAPHICS 0
#define MCM_TRANSP 0
#define MCM_GEOM_ACTIVE 0
#define MCM_NO_ALPHA_CAP 0
#define MCM_BRIGHT 0
#define MCM_INVERSE_FADE 0
#define MCM_FADE_DEPTH 0
#define MCM_LIGHTS 0
#define MCM_SHADOWS 0
#define MCM_TH 0
#define MCM_SKY_FOG 0
#define MCM_ANTI_TRANSP 0
#define MCM_SIMPLE_LIGHT 0
#define MCM_SPECIAL_FOG 0

#if MCM_GEOM_ACTIVE
layout (binding = 0) uniform sampler2DArray s;
layout (binding = 1) uniform sampler2DArray normal_tex;
#if MCM_LIGHTS
layout (binding = 2) uniform sampler2DArray spec;
#endif
#else
layout (binding = 0) uniform sampler2D s;
layout (binding = 1) uniform sampler2D normal_tex;
#if MCM_LIGHTS
layout (binding = 2) uniform sampler2D spec;
#endif
#endif
layout (binding = 4) uniform sampler2D depth;
layout (binding = 5) uniform sampler2DArray shadowtex;
// ...

in struct vox_fout
{
	mat3 tbn;
	vec3 pos;
#if MCM_GEOM_ACTIVE
	vec3 texcoord;
#else
	vec2 texcoord;
#endif
	vec4 color;
#if MCM_INVERSE_FADE
	float size;
#endif
#if MCM_FADE_DEPTH
	float size;
#endif
} fi;

const int LIGHTS_MAX = 38;

// ...
layout (location = 4) uniform vec4 screen_size = vec4(1024, 1024, 0.1, 1000.0);
// ...
layout (location = 6) uniform float time;
// ...
layout (location = 10) uniform vec3 sunlightDir = vec3(0.0, 0.0, -1.0);
layout (location = 11) uniform vec3 maximum_light = vec3(0.9, 0.9, 0.9);
layout (location = 12) uniform vec4 fogCol = vec4(0.0);
layout (location = 13) uniform float fogDist = 1.0 / 100000.0;
// ...
layout (location = 15) uniform float lights_used = 0.0;
layout (location = 16) uniform float minimum_light = 0.2;
#if MCM_LIGHTS
layout (location = 20) uniform mat4 shadow_matrix_array[LIGHTS_MAX];
layout (location = 58) uniform mat4 light_data_array[LIGHTS_MAX];
#endif

layout (location = 0) out vec4 color;
layout (location = 1) out vec4 position;
layout (location = 2) out vec4 nrml;
// ...
layout (location = 4) out vec4 renderhint2;

float snoise2(in vec3 v);

vec4 unused_nonsense() // Prevent shader compiler from claiming variables are unused (Even if they /are/ unused!)
{
	return screen_size + fogCol;
}

float linearizeDepth(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * screen_size.z) / (screen_size.w + screen_size.z - rinput * (screen_size.w - screen_size.z));
}

void applyFog()
{
#if MCM_SKY_FOG
	float fmza = 1.0 - max(min((fi.pos.z - 1000.0) / 2000.0, 1.0), 0.0);
	color.xyz = min(color.xyz * (1.0 - fmza) + fogCol.xyz * fmza, vec3(1.0));
#endif
#if MCM_BRIGHT
	if (fogCol.w > 1.0)
#endif
	{
		float dist = pow(dot(fi.pos, fi.pos) * fogDist, 0.6);
		float fogMod = dist * exp(fogCol.w) * fogCol.w;
		float fmz = min(fogMod, 1.0);
#if MCM_SPECIAL_FOG
		fmz *= fmz * fmz * fmz;
#endif
		color.xyz = min(color.xyz * (1.0 - fmz) + fogCol.xyz * fmz, vec3(1.0));
	}
}

float fix_sqr(in float inTemp)
{
	return 1.0 - (inTemp * inTemp);
}

#if MCM_GEOM_ACTIVE
vec4 read_texture(in sampler2DArray samp_in, in vec3 texcrd)
{
	return texture(samp_in, texcrd);
}
#else
vec4 read_texture(in sampler2D samp_in, in vec2 texcrd)
{
	return texture(samp_in, texcrd);
}
#endif

void main()
{
	position = vec4(fi.pos, 1.0);
	vec4 col = read_texture(s, fi.texcoord);
#if MCM_LIGHTS
	float specularStrength = read_texture(spec, fi.texcoord).x;
#endif // MCM_LIGHTS
#if MCM_NO_ALPHA_CAP
	if (col.w * fi.color.w <= 0.01)
	{
		discard;
	}
#if MCM_ANTI_TRANSP
	col.w = 1.0;
#endif
#else // MCM_NO_ALPHA_CAP
#if MCM_TRANSP
	if (col.w * fi.color.w >= 0.99)
	{
		discard;
	}
#else // MCM_TRANSP
	if (col.w * fi.color.w < 0.99)
	{
		discard;
	}
#endif // else - MCM_TRANPS
#endif // ELSE - MCM_NO_ALPHA_CAP
	color = col * fi.color;
#if MCM_BRIGHT
#else // MCM_BRIGHT
	float opac_min = 0.0;
	vec3 norms = read_texture(normal_tex, fi.texcoord).xyz * 2.0 - vec3(1.0);
	vec3 tf_normal = normalize(fi.tbn * norms);
	nrml = vec4(tf_normal, 1.0);
#if MCM_LIGHTS
	vec3 res_color = vec3(0.0);
	int count = int(lights_used);
	for (int i = 0; i < count; i++)
	{
		mat4 light_data = light_data_array[i];
		mat4 shadow_matrix = shadow_matrix_array[i];
		// Light data.
		vec3 light_pos = vec3(light_data[0][0], light_data[0][1], light_data[0][2]); // The position of the light source.
		float diffuse_albedo = light_data[0][3]; // The diffuse albedo of this light (diffuse light is multiplied directly by this).
		float specular_albedo = light_data[1][0]; // The specular albedo (specular power is multiplied directly by this).
		float should_sqrt = light_data[1][1]; // 0 to not use square-root trick, 1 to use it (see implementation for details).
		vec3 light_color = vec3(light_data[1][2], light_data[1][3], light_data[2][0]); // The color of the light.
		float light_radius = light_data[2][1]; // The maximum radius of the light.
		vec3 eye_pos = vec3(light_data[2][2], light_data[2][3], light_data[3][0]); // The position of the camera eye.
		float light_type = light_data[3][1]; // What type of light this is: 0 is standard (point, sky, etc.), 1 is conical (spot light).
		int is_point = light_type >= 1.5 ? 1 : 0;
		float tex_size = light_data[3][2]; // If shadows are enabled, this is the inverse of the texture size of the shadow map.
		// float unused = light_data[3][3];
		vec4 f_spos = is_point == 1 ? vec4(0.0, 0.0, 0.0, 1.0) : shadow_matrix * vec4(fi.pos, 1.0); // Calculate the position of the light relative to the view.
		f_spos /= f_spos.w; // Standard perspective divide.
		vec3 light_path = light_pos - fi.pos; // What path a light ray has to travel down in theory to get from the source to the current pixel.
		float light_length = length(light_path); // How far the light is from this pixel.
		float d = light_length / light_radius; // How far the pixel is from the end of the light.
		float atten = clamp(1.0 - (d * d), 0.0, 1.0); // How weak the light is here, based purely on distance so far.
		if (is_point == 0 && light_type >= 0.5) // If this is a conical (spot light)...
		{
			atten *= 1.0 - (f_spos.x * f_spos.x + f_spos.y * f_spos.y); // Weaken the light based on how far towards the edge of the cone/circle it is. Bright in the center, dark in the corners.
		}
		if (atten <= 0.0) // If light is really weak...
		{
			continue; // Forget this light, move on already!
		}
		if (should_sqrt >= 0.5) // If inverse square trick is enabled (generally this will be 1.0 or 0.0)
		{
			light_path = light_pos;
			light_length = 1.0;
			f_spos.x = sign(f_spos.x) * fix_sqr(1.0 - abs(f_spos.x)); // Inverse square the relative position while preserving the sign. Shadow creation buffer also did this.
			f_spos.y = sign(f_spos.y) * fix_sqr(1.0 - abs(f_spos.y)); // This section means that coordinates near the center of the light view will have more pixels per area available than coordinates far from the center.
		}
#if MCM_SIMPLE_LIGHT
		const float depth = 1.0;
#else // MCM_SIMPLE_LIGHT
		vec3 fs = vec3(0.0);
		if (is_point == 0)
		{
			// Create a variable representing the proper screen/texture coordinate of the shadow view (ranging from 0 to 1 instead of -1 to 1).
			fs = f_spos.xyz * 0.5 + vec3(0.5, 0.5, 0.5); 
			if (fs.x < 0.0 || fs.x > 1.0
				|| fs.y < 0.0 || fs.y > 1.0
				|| fs.z < 0.0 || fs.z > 1.0) // If any coordinate is outside view range...
			{
				continue; // We can't light it! Discard straight away!
			}
		}
		// TODO: maybe HD well blurred shadows?
#if MCM_SHADOWS
		float shadowID = float(i);
		float mdX = 1.0, mdY = 1.0, rdX = 0.0, rdY = 0.0;
		if (i >= 10)
		{
			shadowID = float((i - 10) / 4);
			int ltCO = (i - 10) % 4;
			rdY = float(ltCO / 2) * 0.5;
			rdX = float(ltCO % 2) * 0.5;
			mdX = 0.5;
			mdY = 0.5;
		}
#if 1 // TODO: MCM_SHADOW_BLURRING?
		float depth = 1.0;
		if (is_point == 0)
		{
			depth = 0.0;
			// Pretty quality (soft) shadows require a quality graphics card.
#if MCM_GOOD_GRAPHICS
			const float depth_jump = 0.5; // Placeholder default value
			// This area is some calculus-ish stuff based upon NVidia sample code (naturally, it seems to run poorly on AMD cards. Good area to recode/optimize!)
			// It's used to take the shadow map coordinates, and gets a safe Z-modifier value (See below).
			vec3 duvdist_dx = dFdx(fs);
			vec3 duvdist_dy = dFdy(fs);
			vec2 dz_duv = vec2(duvdist_dy.y * duvdist_dx.z - duvdist_dx.y * duvdist_dy.z, duvdist_dx.x * duvdist_dy.z - duvdist_dy.x * duvdist_dx.z);
			float tlen = (duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x);
			dz_duv /= tlen;
			float oneoverdj = 1.0 / depth_jump;
			float jump = tex_size * depth_jump;
			float depth_count = 0;
			// Loop over an area quite near the pixel on the shadow map, but still covering multiple pixels of the shadow map.
			for (float x = -oneoverdj * 2; x < oneoverdj * 2 + 1; x++)
			{
				for (float y = -oneoverdj * 2; y < oneoverdj * 2 + 1; y++)
				{
					float offz = dot(dz_duv, vec2(x * jump, y * jump)) * 1000.0; // Use the calculus magic from before to get a safe Z-modifier.
					if (offz > -0.000001) // (Potentially removable?) It MUST be negative, and below a certain threshold. If it's not...
					{
						offz = -0.000001; // Force it to the threshold value to reduce errors.
					}
					//offz -= 0.001; // Set it a bit farther regardless to reduce bad shadows.
					float rd = texture(shadowtex, vec3((fs.x + x * jump) * mdX + rdX, (fs.y + y * jump) * mdY + rdY, shadowID)).r; // Calculate the depth of the pixel.
					depth += (rd >= (fs.z + offz) ? 1.0 : 0.0); // Get a 1 or 0 depth value for the current pixel. 0 means don't light, 1 means light.
					depth_count++; // Can probably use math to generate this number rather than constantly incrementing a counter.
				}
			}
			depth = depth / depth_count; // Average up the 0 and 1 light values to produce gray near the edges of shadows. Soft shadows, hooray!
#else // MCM_GOOD_GRAPHICS
			int loops = 0;
			for (float x = -1.0; x <= 1.0; x += 0.5)
			{
				for (float y = -1.0; y <= 1.0; y += 0.5)
				{
					loops++;
					float rd = texture(shadowtex, vec3((fs.x + x * tex_size) * mdX + rdX, (fs.y + y * tex_size) * mdY + rdY, shadowID)).r; // Calculate the depth of the pixel.
					depth += (rd >= (fs.z - 0.001) ? 1.0 : 0.0);
				}
			}
			depth /= loops;
#endif // else - MCM_GOOD_GRAPHICS
		}
#else // shadow blur (1)
		float depth = 1.0;
		if (is_point == 0)
		{
			float rd = texture(shadowtex, vec3(fs.x * mdX + rdX, fs.y * mdY + rdY, shadowID)).r; // Calculate the depth of the pixel.
			depth = (rd >= (fs.z - 0.001) ? 1.0 : 0.0); // If we have a bad graphics card, just quickly get a 0 or 1 depth value. This will be pixelated (hard) shadows!
		}
#endif // else - shadow blur (1)
		if (depth <= 0.0)
		{
			continue;
		}
#else // MCM_SHADOWS
		const float depth = 1.0;
#endif // else - MCM_SHADOWS
#endif // else - MCM_SIMPLE_LIGHT
		vec3 L = light_path / light_length; // Get the light's movement direction as a vector
		vec3 diffuse = max(dot(tf_normal, L), 0.0) * vec3(diffuse_albedo); // Find out how much diffuse light to apply
		vec3 reller = normalize(fi.pos - eye_pos);
		float spec_res = pow(max(dot(reflect(L, -tf_normal), reller), 0.0), 200.0) * specular_albedo * specularStrength;
		opac_min += spec_res;
		vec3 specular = vec3(spec_res); // Find out how much specular light to apply.
		res_color += (vec3(depth, depth, depth) * atten * (diffuse * light_color) * color.xyz) + (min(specular, 1.0) * light_color * atten * depth); // Put it all together now.
	}
	color.xyz = min(res_color * (1.0 - max(0.2, minimum_light)) + color.xyz * max(0.2, minimum_light), vec3(1.0));
#else // MCM_LIGHTS
	float dotted = dot(-tf_normal, sunlightDir);
	dotted = dotted <= 0.0 ? 0.0 : sqrt(dotted);
	color.xyz *= min(max(dotted * maximum_light, max(0.2, minimum_light)), 1.0) * 0.75;
#endif // else - MCM_LIGHTS
	applyFog();
#if MCM_TRANSP
#endif // MCM_TRANSP
#endif // else - MCM_BRIGHT
	applyFog();
#if MCM_INVERSE_FADE
	float dist = linearizeDepth(gl_FragCoord.z);
	vec2 fc_xy = gl_FragCoord.xy / screen_size.xy;
	float depthval = linearizeDepth(texture(depth, fc_xy).x);
	float mod2 = min(max(0.001 / max(depthval - dist, 0.001), 0.0), 1.0);
	if (mod2 < 0.8)
	{
		discard;
	}
#endif // MCM_INVERSE_FADE
#if MCM_FADE_DEPTH
	float dist = linearizeDepth(gl_FragCoord.z);
	vec2 fc_xy = gl_FragCoord.xy / screen_size.xy;
	float depthval = linearizeDepth(texture(depth, fc_xy).x);
	color.w *= min(max((depthval - dist) * fi.size * 0.5 * (screen_size.w - screen_size.z), 0.0), 1.0);
#endif // MCM_FADE_DEPTH
}

#include glnoise.inc
