#version 430 core

#define MCM_GEOM_ACTIVE 0
#define MCM_NO_BONES 0
#define MCM_GEOM_THREED_TEXTURE 0

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoords;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec4 color;
#if MCM_GEOM_ACTIVE
#else
layout (location = 5) in vec4 Weights;
layout (location = 6) in vec4 BoneID;
layout (location = 7) in vec4 Weights2;
layout (location = 8) in vec4 BoneID2;
#endif

const int MAX_BONES = 200;

#if MCM_GEOM_ACTIVE
#else
layout (location = 1) uniform mat4 projection = mat4(1.0);
#endif
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);
// ...
#if MCM_GEOM_ACTIVE
#else
layout (location = 5) uniform float should_sqrt = 0.0;
// ...
layout (location = 100) uniform mat4 simplebone_matrix = mat4(1.0);
layout (location = 101) uniform mat4 boneTrans[MAX_BONES];
#endif

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

#define f fi

float fix_sqr(in float inTemp)
{
	return 1.0 - (inTemp * inTemp);
}
#endif


void main()
{
#if MCM_GEOM_ACTIVE
	f.position = model_matrix * vec4(position, 1.0);
#else
#if MCM_NO_BONES
	const float rem = 1.0;
#else
	float rem = 1.0 - (Weights[0] + Weights[1] + Weights[2] + Weights[3] + Weights2[0] + Weights2[1] + Weights2[2] + Weights2[3]);
#endif
	vec4 pos1;
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
	}
	else
	{
		pos1 = vec4(position, 1.0);
	}
	pos1 *= simplebone_matrix;
	f.position = projection * model_matrix * vec4(pos1.xyz, 1.0);
	if (should_sqrt >= 0.5)
	{
		f.position /= f.position.w;
		f.position.x = sign(f.position.x) * fix_sqr(1.0 - abs(f.position.x));
		f.position.y = sign(f.position.y) * fix_sqr(1.0 - abs(f.position.y));
	}
#endif
#if MCM_GEOM_THREED_TEXTURE
	f.texcoord = texcoords;
#else
	f.texcoord = texcoords.xy;
#endif
	f.color = color;
	gl_Position = f.position;
}
