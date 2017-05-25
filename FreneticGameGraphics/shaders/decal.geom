
#version 430 core

#define MCM_PRETTY 0

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);

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
#else
	mat3 tbn;
	vec3 pos;
	vec3 texcoord;
	vec4 color;
#endif
	float size;
} fi;

vec3 qfix(vec3 pos)
{
#if MCM_PRETTY
	fi.position = vec4(pos, 1.0);
#else
	fi.pos = pos;
#endif
	return pos;
}

void main()
{
	vec3 pos = gl_in[0].gl_Position.xyz;
	 // TODO: Configurable decal render range cap! Maybe with a fade over distance?
	/*if (dot(pos, pos) > (50.0 * 50.0))
	{
		return;
	}*/
	vec3 norm = vec3(f[0].tbn[0][2], f[0].tbn[1][2], f[0].tbn[2][2]);
	fi.tbn = f[0].tbn;
	float scale = f[0].texcoord.x * 0.5;
	float tid = f[0].texcoord.y;
	fi.color = f[0].color;
	fi.size = 1.0 / scale;
	vec3 UPPER = norm.z > 0.8 ? vec3(0.0, 1.0, 0.0) : vec3(0.0, 0.0, 1.0);
	vec3 tangent = normalize(UPPER - (dot(UPPER, norm)) * norm);
	vec3 bitangent = cross(tangent, norm);
	vec3 xp = tangent * scale;
	vec3 yp = bitangent * scale;
	// First Vertex
	vec3 p = qfix(pos + (-xp - yp));
	gl_Position = proj_matrix * vec4(p, 1.0);
	fi.texcoord = vec3(0.0, 1.0, tid);
	EmitVertex();
	// Second Vertex
	p = qfix(pos + (xp - yp));
	gl_Position = proj_matrix * vec4(p, 1.0);
	fi.texcoord = vec3(1.0, 1.0, tid);
	EmitVertex();
	// Third Vertex
	p = qfix(pos + (-xp + yp));
	gl_Position = proj_matrix * vec4(p, 1.0);
	fi.texcoord = vec3(0.0, 0.0, tid);
	EmitVertex();
	// Forth Vertex
	p = qfix(pos + (xp + yp));
	gl_Position = proj_matrix * vec4(p, 1.0);
	fi.texcoord = vec3(1.0, 0.0, tid);
	EmitVertex();
	EndPrimitive();
}
