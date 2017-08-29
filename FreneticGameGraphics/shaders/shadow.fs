//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_GEOM_ACTIVE 0
#define MCM_FADE_DEPTH 0
#define MCM_SHADOWS 0

#if MCM_GEOM_ACTIVE
layout (binding = 0) uniform sampler2DArray s;
#else
layout (binding = 0) uniform sampler2D s;
#endif

layout (location = 4) uniform float allow_transp = 0.0;

in struct vox_fout
{
	vec4 position;
#if MCM_GEOM_ACTIVE
	vec3 texcoord;
#else
	vec2 texcoord;
#endif
	vec4 color;
	mat3 tbn;
#if MCM_FADE_DEPTH
#if MCM_SHADOWS
	float size;
#endif
#endif
} fi;

layout (location = 0) out float color;

void main()
{
	vec4 col = texture(s, fi.texcoord) * fi.color;
	if (col.w < 0.9 && ((col.w < 0.05) || (allow_transp <= 0.5)))
	{
		discard;
	}
	color = gl_FragCoord.z;
}
