//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_GEOM_ACTIVE 0
#define MCM_INVERSE_FADE 0
#define MCM_GEOM_THREED_TEXTURE 0

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoords;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec4 color;
layout (location = 5) in vec4 Weights;
layout (location = 6) in vec4 BoneID;
layout (location = 7) in vec4 Weights2;
layout (location = 8) in vec4 BoneID2;

#if MCM_GEOM_ACTIVE
out struct vox_out
#else
out struct vox_fout
#endif
{
	vec4 position;
#if MCM_GEOM_THREED_TEXTURE
	vec3 texcoord;
#else
	vec2 texcoord;
#endif
	vec4 color;
	mat3 tbn;
#if MCM_GEOM_ACTIVE
} f;
#else
} fi;
#endif

#if MCM_GEOM_ACTIVE
#else
const int MAX_BONES = 200;
#endif

vec4 color_for(in vec4 pos, in vec4 colt);

#include glnoise.inc

#if MCM_GEOM_ACTIVE
#else
layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);
#endif
layout (location = 2) uniform mat4 mv_matrix = mat4(1.0);
layout (location = 3) uniform vec4 v_color = vec4(1.0);
// ...
layout (location = 6) uniform float time;
// ...
#if MCM_GEOM_ACTIVE
#else
layout (location = 100) uniform mat4 simplebone_matrix = mat4(1.0);
layout (location = 101) uniform mat4 boneTrans[MAX_BONES];
#endif

void main()
{
	vec4 pos1;
	vec4 norm1;
#if MCM_GEOM_ACTIVE
	pos1 = vec4(position, 1.0);
	norm1 = vec4(normal, 1.0);
	f.tbn = (mat3(vec3(0.0), vec3(0.0), normal)); // TODO: Improve for decals?!
#if MCM_GEOM_THREED_TEXTURE
	f.texcoord = texcoords;
#else
	f.texcoord = texcoords.xy;
#endif
    f.color = color * v_color;
	gl_Position = mv_matrix * vec4(pos1.xyz, 1.0);
#else
	float rem = 1.0 - (Weights[0] + Weights[1] + Weights[2] + Weights[3] + Weights2[0] + Weights2[1] + Weights2[2] + Weights2[3]);
	mat4 BT = mat4(1.0);
	if (rem < 0.99)
	{
		BT = boneTrans[int(BoneID[0])] * Weights[0];
		BT += boneTrans[int(BoneID[1])] * Weights[1];
		BT += boneTrans[int(BoneID[2])] * Weights[2];
		BT += boneTrans[int(BoneID[3])] * Weights[3];
		BT += boneTrans[int(BoneID2[0])] * Weights2[0];
		BT += boneTrans[int(BoneID2[1])] * Weights2[1];
		BT += boneTrans[int(BoneID2[2])] * Weights2[2];
		BT += boneTrans[int(BoneID2[3])] * Weights2[3];
		BT += mat4(1.0) * rem;
		pos1 = vec4(position, 1.0) * BT;
		norm1 = vec4(normal, 1.0) * BT;
	}
	else
	{
		pos1 = vec4(position, 1.0);
		norm1 = vec4(normal, 1.0);
	}
	pos1 *= simplebone_matrix;
	norm1 *= simplebone_matrix;
	fi.texcoord = texcoords.xy;
	fi.position = mv_matrix * vec4(pos1.xyz, 1.0);
	fi.position /= fi.position.w;
    fi.color = color_for(fi.position, color * v_color);
	gl_Position = proj_matrix * mv_matrix * vec4(pos1.xyz, 1.0);
	mat4 mv_mat_simple = mv_matrix;
	mv_mat_simple[3][0] = 0.0;
	mv_mat_simple[3][1] = 0.0;
	mv_mat_simple[3][2] = 0.0;
	vec3 tf_normal = (mv_mat_simple * vec4(norm1.xyz, 0.0)).xyz; // TODO: Should BT be here?
	vec3 tf_tangent = (mv_mat_simple * vec4(tangent, 0.0)).xyz; // TODO: Should BT be here?
	vec3 tf_bitangent = (mv_mat_simple * vec4(cross(tangent, norm1.xyz), 0.0)).xyz; // TODO: Should BT be here?
	fi.tbn = (mat3(tf_tangent, tf_bitangent, tf_normal)); // TODO: Neccessity of transpose()?
#endif
}

const float min_cstrobe = 3.0 / 255.0;

const float min_transp = 1.0 / 255.0;

vec4 color_for(in vec4 pos, in vec4 colt)
{
	if (colt.w <= min_transp)
	{
		if (colt.x == 0.0 && colt.y == 0.0 && colt.z == 0.0)
		{
			float r = snoise2(vec3((pos.x + time) / 10.0, (pos.y + time) / 10.0, (pos.z + time) / 10.0));
			float g = snoise2(vec3((pos.x + 50.0 + time * 2) / 10.0, (pos.y + 127.0 + time * 1.7) / 10.0, (pos.z + 10.0 + time * 2.3) / 10.0));
			float b = snoise2(vec3((pos.x - 50.0 - time) / 10.0, (pos.y - 65.0 - time * 1.56) / 10.0, (pos.z + 73.0 - time * 1.3) / 10.0));
			return vec4(r, g, b, 1.0);
		}
		else
		{
			float adjust = abs(mod(time * 0.2, 2.0));
			if (adjust > 1.0)
			{
				adjust = 2.0 - adjust;
			}
			return vec4(colt.x * adjust, colt.y * adjust, colt.z * adjust, 1.0);
		}
	}
	else if (colt.w <= min_cstrobe)
	{
			float adjust = abs(mod(time * 0.2, 2.0));
			if (adjust > 1.0)
			{
				adjust = 2.0 - adjust;
			}
			return vec4(1.0 - colt.x * adjust, 1.0 - colt.y * adjust, 1.0 - colt.z * adjust, 1.0);
	}
	return colt;
}
