#version 430 core

#define MCM_PRETTY 0
#define MCM_FADE_DEPTH 0
#define MCM_SHADOWS 0
#define MCM_IS_A_SHADOW 0

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);
// ...
#if MCM_IS_A_SHADOW
layout (location = 5) uniform float should_sqrt = 0.0;
// ...
layout (location = 7) uniform vec3 camPos = vec3(0.0);
#endif
layout (location = 10) uniform vec3 sunlightDir = vec3(0.0, 0.0, -1.0);

in struct vox_out
{
#if MCM_PRETTY
	vec4 position;
	vec2 texcoord;
	vec4 color;
	mat3 tbn;
#else
	mat3 tbn;
	vec2 texcoord;
	vec4 color;
#endif
} f[1];

out struct vox_fout
{
#if MCM_PRETTY
	vec4 position;
	vec3 texcoord;
	vec4 color;
	mat3 tbn;
#if MCM_IS_A_SHADOW
#else
	vec2 scrpos;
	float z;
#endif
#else
	mat3 tbn;
	vec3 pos;
	vec3 texcoord;
	vec4 color;
#endif
#if MCM_FADE_DEPTH
	float size;
#endif
} fi;

float fix_sqr(in float inTemp)
{
	return 1.0 - (inTemp * inTemp);
}

vec4 final_fix(in vec4 pos)
{
#if MCM_IS_A_SHADOW
	if (should_sqrt >= 0.5)
	{
		pos /= pos.w;
		pos.x = sign(pos.x) * fix_sqr(1.0 - abs(pos.x));
		pos.y = sign(pos.y) * fix_sqr(1.0 - abs(pos.y));
	}
#endif
	return pos;
}

vec4 qfix(in vec4 pos)
{
#if MCM_PRETTY
	fi.position = pos;
	vec4 npos = proj_matrix * pos;
#if MCM_IS_A_SHADOW
#else
	fi.scrpos = npos.xy / npos.w * 0.5 + vec2(0.5);
	fi.z = npos.z;
#endif
#else
	fi.pos = pos.xyz;
#endif
	return pos;
}

void main()
{
	vec3 pos = gl_in[0].gl_Position.xyz;
#if MCM_IS_A_SHADOW
	vec3 pos_norm = normalize(pos.xyz - camPos);
#else
	vec3 pos_norm = normalize(pos.xyz);
#endif
	vec3 UPPER = pos_norm.z > 0.8 ? vec3(0.0, 1.0, 0.0) : vec3(0.0, 0.0, 1.0);
	vec3 up = normalize(UPPER - (dot(UPPER, pos_norm)) * pos_norm);
	float scale = f[0].texcoord.x * 0.5;
	float tid = f[0].texcoord.y;
	vec3 right = cross(up, pos_norm);
	fi.color = f[0].color;
	fi.tbn = transpose(mat3(vec3(0.0), vec3(0.0), sunlightDir));
#if MCM_FADE_DEPTH
	fi.size = 1.0 / scale;
#endif
	// TODO: Drop angle based logic. There's almost certainly a more stable and better optimized method to do this.
	float angle = (scale * 5.0) * (float(int(tid) % 2) * 2.0) - 1.0;
	float c = cos(angle);
	float s = sin(angle);
	float C = 1.0 - c;
	mat4 rot_mat = mat4(
		pos_norm.x * pos_norm.x * C + c, pos_norm.x * pos_norm.y * C - pos_norm.z * s, pos_norm.x * pos_norm.z * C + pos_norm.y * s, 0.0,
		pos_norm.y * pos_norm.x * C + pos_norm.z * s, pos_norm.y * pos_norm.y * C + c, pos_norm.y * pos_norm.z * C - pos_norm.x * s, 0.0,
		pos_norm.z * pos_norm.x * C - pos_norm.y * s, pos_norm.z * pos_norm.y * C + pos_norm.x * s, pos_norm.z * pos_norm.z * C + c, 0.0,
		0.0, 0.0, 0.0, 1.0);
	vec3 right_n = (rot_mat * vec4(right, 1.0)).xyz;
	vec3 up_n = (rot_mat * vec4(up, 1.0)).xyz;
	// First Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos - (right_n + up_n) * scale, 1.0)));
	fi.texcoord = vec3(0.0, 1.0, tid);
	EmitVertex();
	// Second Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos + (right_n - up_n) * scale, 1.0)));
	fi.texcoord = vec3(1.0, 1.0, tid);
	EmitVertex();
	// Third Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos - (right_n - up_n) * scale, 1.0)));
	fi.texcoord = vec3(0.0, 0.0, tid);
	EmitVertex();
	// Forth Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos + (right_n + up_n) * scale, 1.0)));
	fi.texcoord = vec3(1.0, 0.0, tid);
	EmitVertex();
	EndPrimitive();
}
