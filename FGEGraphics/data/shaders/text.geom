//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);

in struct text_out
{
	vec4 color;
	vec4 texcoord;
	vec4 screenrect;
} f[1];

out struct text_fout
{
	vec4 color;
	vec2 texcoord;
} fi;

void main()
{
	fi.color = f[0].color;
	// First Vertex
	gl_Position = proj_matrix * vec4(f[0].screenrect.z, f[0].screenrect.w, 0.0, 1.0);
	fi.texcoord = vec2(f[0].texcoord.z, f[0].texcoord.w);
	EmitVertex();
	// Second Vertex
	gl_Position = proj_matrix * vec4(f[0].screenrect.x, f[0].screenrect.w, 0.0, 1.0);
	fi.texcoord = vec2(f[0].texcoord.x, f[0].texcoord.w);
	EmitVertex();
	// Third Vertex
	gl_Position = proj_matrix * vec4(f[0].screenrect.z, f[0].screenrect.y, 0.0, 1.0);
	fi.texcoord = vec2(f[0].texcoord.z, f[0].texcoord.y);
	EmitVertex();
	// Fourth Vertex
	gl_Position = proj_matrix * vec4(f[0].screenrect.x, f[0].screenrect.y, 0.0, 1.0);
	fi.texcoord = vec2(f[0].texcoord.x, f[0].texcoord.y);
	EmitVertex();
	EndPrimitive();
}
