#version 430 core

#define MCM_PRETTY 0
#define MCM_SHADOWS 0
#define MCM_IS_A_SHADOW 0

layout (points) in;
layout (triangle_strip, max_vertices = 6) out;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);
// ...
#if MCM_IS_A_SHADOW
layout (location = 5) uniform float should_sqrt = 0.0;
#endif
layout (location = 6) uniform float time = 0.0;
layout (location = 7) uniform vec3 wind = vec3(0.0);
layout (location = 8) uniform float render_distance_limit = 50.0 * 50.0;

in struct vox_out
{
#if MCM_PRETTY
	vec4 position;
	vec3 texcoord;
	vec4 color;
	mat3 tbn;
#else
	mat3 tbn;
	vec3 texcoord;
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
#else
	mat3 tbn;
	vec3 pos;
	vec3 texcoord;
	vec4 color;
#endif
} fi;

#include glnoise.inc

vec4 qfix(in vec4 pos, in vec3 right, in vec3 pos_norm)
{
	fi.tbn = transpose(mat3(right, cross(right, pos_norm), pos_norm)); // TODO: Neccessity of transpose()?
#if MCM_PRETTY
	fi.position = pos;
#else
	fi.pos = pos.xyz;
#endif
	return pos;
}

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

void main()
{
	vec3 pos = gl_in[0].gl_Position.xyz;
	float chance = snoise2(pos);
	chance *= chance;
	if (dot(pos.xy, pos.xy) * chance > render_distance_limit)
	{
		return;
	}
	float snz = snoise((pos + vec3(time, time, 0.0)) * 0.2);
	float timeSinceSquished = log(min(60.0, max(0.0, f[0].texcoord.z - time)) + 1.0) * (1.0 / log(60.0));
	vec3 wnd = wind * snz * (1.0 - timeSinceSquished) + vec3(timeSinceSquished, 0.0, -timeSinceSquished);
	vec3 up = vec3(timeSinceSquished, 0.0, 1.0 - timeSinceSquished);
	vec3 right = cross(up, normalize(vec3(pos.x, pos.y, 0.0))) * 0.3;
	vec3 nr = right * (1.0 / 0.3);
	vec3 pos_norm = normalize(pos.xyz + wnd);
	float scale = f[0].texcoord.x * 0.5;
	float tid = f[0].texcoord.y;
	float snoisey = snoise(pos.xyz + wnd * 2.0);
	float snoisey2 = snoise(pos.xyz - wnd * 2.0);
	vec3 this_grass = normalize(vec3(snoisey, snoisey2, -5.0));
	fi.color = vec4(f[0].color.xyz * dot(pos_norm, vec3(0.0, 0.0, -1.0)) * 0.5 + 0.5, 1.0) * f[0].color;
	// First Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos - (right) * scale, 1.0), nr, this_grass));
	fi.texcoord = vec3(0.0, 1.0, tid);
	EmitVertex();
	// Second Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos + (right) * scale, 1.0), nr, this_grass));
	fi.texcoord = vec3(1.0, 1.0, tid);
	EmitVertex();
	// Third Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos - (right - up * 2.0) * scale + wnd, 1.0), nr, this_grass));
	fi.texcoord = vec3(0.0, 0.5, tid);
	EmitVertex();
	// Forth Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos + (right + up * 2.0) * scale + wnd, 1.0), nr, this_grass));
	fi.texcoord = vec3(1.0, 0.5, tid);
	EmitVertex();
	// Fifth Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos - (right - up * 4.0) * scale + wnd * 2.0, 1.0), nr, this_grass));
	fi.texcoord = vec3(0.0, 0.0, tid);
	EmitVertex();
	// Sixth Vertex
	gl_Position = final_fix(proj_matrix * qfix(vec4(pos + (right + up * 4.0) * scale + wnd * 2.0, 1.0), nr, this_grass));
	fi.texcoord = vec3(1.0, 0.0, tid);
	EmitVertex();
	EndPrimitive();
}
