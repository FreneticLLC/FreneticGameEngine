//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_SKYBOX 0
#define MCM_REFRACT 0
#define MCM_GEOM_ACTIVE 0
#define MCM_INVERSE_FADE 0
#define MCM_NO_ALPHA_CAP 0
#define MCM_NO_LIGHT_ANGLE 0
#define MCM_FADE_DEPTH 0

#if MCM_GEOM_ACTIVE
layout (binding = 0) uniform sampler2DArray s;
layout (binding = 1) uniform sampler2DArray normal_tex;
layout (binding = 2) uniform sampler2DArray spec;
layout (binding = 3) uniform sampler2DArray refl;
#if MCM_INVERSE_FADE
layout (binding = 4) uniform sampler2D depth;
#endif // MCM_INVERSE_FADE
#else // MCM_GEOM_ACTIVE
layout (binding = 0) uniform sampler2D s;
layout (binding = 1) uniform sampler2D normal_tex;
layout (binding = 2) uniform sampler2D spec;
layout (binding = 3) uniform sampler2D refl;
#endif // else - MCM_GEOM_ACTIVE

// ...
layout (location = 4) uniform vec4 screen_size = vec4(1024, 1024, 0.1, 1000.0);
// ...
layout (location = 9) uniform float refract_eta = 0.0;
// ...
layout (location = 16) uniform float minimum_light = 0.0;
#if MCM_SKYBOX
layout (location = 17) uniform float write_hints = 1.0;
layout (location = 18) uniform vec4 fogCol = vec4(0.0);
#endif // MCM_SKYBOX

in struct vox_fout
{
	vec4 position;
#if MCM_GEOM_ACTIVE
	vec3 texcoord;
#else // MCM_GEOM_ACTIVE
	vec2 texcoord;
#endif // else - MCM_GEOM_ACTIVE
	vec4 color;
	mat3 tbn;
#if MCM_INVERSE_FADE
	float size;
#endif // MCM_INVERSE_FADE
#if MCM_FADE_DEPTH
	vec2 scrpos;
	float z;
	float size;
#endif // MCM_FADE_DEPTH
} fi;

layout (location = 0) out vec4 color;
layout (location = 1) out vec4 position;
layout (location = 2) out vec4 normal;
layout (location = 3) out vec4 renderhint;
layout (location = 4) out vec4 renderhint2;

float linearizeDepth(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * screen_size.z) / (screen_size.w + screen_size.z - rinput * (screen_size.w - screen_size.z));
}

void main()
{
	vec4 col = texture(s, fi.texcoord);
#if MCM_SKYBOX
	float posZ = normalize(fi.position.xyz).z;
	float angleNormalizedZ = sqrt(posZ) * 3500.0;
	float fmza = 1.0 - max(min((angleNormalizedZ - 1000.0) / 2000.0, 1.0), 0.0);
	col.xyz = min(col.xyz * (1.0 - fmza) + fogCol.xyz * fmza, vec3(1.0));
#endif // MCM_SKYBOX
#if MCM_REFRACT
	if (refract_eta > 0.01)
	{
		vec3 tnorms = fi.tbn * (texture(normal_tex, fi.texcoord).xyz * 2.0 - vec3(1.0));
		color = vec4(0.0);
		position = vec4(0.0);
		normal = vec4(0.0);
		renderhint = vec4(0.0);
		renderhint2 = vec4(tnorms, 1.0);
		return;
	}
	else
	{
		discard;
	}
#endif // MCM_REFRACT
#if MCM_NO_ALPHA_CAP
#else // MCM_NO_ALPHA_CAP
#if MCM_SKYBOX
	if (col.w * fi.color.w < 0.01)
	{
		discard;
	}
#else // MCM_SKYBOX
	if (col.w * fi.color.w < 0.99)
	{
		discard;
	}
#endif // else - MCM_SKYBOX
#endif // else - MCM_NO_ALPHA_CAP
	float specular_strength = texture(spec, fi.texcoord).r;
	float reflection_amt = texture(refl, fi.texcoord).r;
	vec3 norms = texture(normal_tex, fi.texcoord).xyz * 2.0 - vec3(1.0);
	color = col * fi.color;
	position = vec4(fi.position.xyz, 1.0);
#if MCM_NO_LIGHT_ANGLE
	normal = vec4(0.0, 0.0, 0.0, 1.0);
#else // MCM_NO_LIGHT_ANGLE
	normal = vec4(normalize(fi.tbn * norms), 1.0);
#endif // else - MCM_NO_LIGHT_ANGLE
#if MCM_SKYBOX
	if (write_hints > 0.5)
	{
		renderhint = vec4(specular_strength, 0.0 /* TODO: Blur */, minimum_light, 1.0);
		renderhint2 = vec4(0.0, reflection_amt, 0.0, 1.0);
	}
	else
	{
		renderhint = vec4(0.0);
		renderhint2 = vec4(0.0);
	}
#else // MCM_SKYBOX
	renderhint = vec4(specular_strength, 0.0 /* TODO: Blur */, minimum_light, 1.0);
	renderhint2 = vec4(0.0, reflection_amt, 0.0, 1.0);
#endif // else - MCM_SKYBOX
#if MCM_INVERSE_FADE
	float dist = linearizeDepth(gl_FragCoord.z);
	vec2 fc_xy = gl_FragCoord.xy / screen_size.xy;
	float depthval = linearizeDepth(texture(depth, fc_xy).x);
	float mod = min(max(0.001 / max(depthval - dist, 0.001), 0.0), 1.0);
	if (mod < 0.8)
	{
		discard;
	}
#endif // MCM_INVERSE_FADE
#if MCM_FADE_DEPTH
	position = vec4(0.0);
	normal = vec4(0.0);
	renderhint = vec4(0.0);
	renderhint2 = vec4(0.0);
#endif // MCM_FADE_DEPTH
}
