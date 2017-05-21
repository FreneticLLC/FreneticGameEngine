#version 430 core

layout (location = 0) in vec4 screenrect;
layout (location = 1) in vec4 texcoord;
layout (location = 2) in vec4 color;

layout (location = 3) uniform vec3 v_color;

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec3 f_texcoord;

out struct text_out {
	vec4 color;
	vec4 texcoord;
	vec4 screenrect;
} f;

void main()
{
	f.color = vec4(color.xyz * v_color.xyz, color.w);
	f.texcoord = texcoord;
	f.screenrect = screenrect;
}
