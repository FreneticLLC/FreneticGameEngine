#version 430 core

layout (binding = 0) uniform sampler2D tex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec2 f_texcoord;
layout (location = 2) in vec2 f_pos;

layout (location = 6) uniform vec2 light_pos = vec2(0.0);

out float color;

void main()
{
	vec4 tcolor = texture(tex, f_texcoord) * f_color;
	if (tcolor.w < 0.01)
	{
		discard;
	}
	vec2 rel_pos = f_pos - light_pos;
	color = dot(rel_pos, rel_pos);
}
