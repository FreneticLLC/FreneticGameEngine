#version 430 core

layout (binding = 0) uniform sampler2D color_tex;
layout (binding = 1) uniform sampler2D light_tex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec2 f_texcoord;

out vec4 color;

void main()
{
	vec4 c_color = texture(color_tex, f_texcoord) * f_color;
	c_color.w = min(c_color.w, 1.0);
	vec4 l_color = texture(light_tex, f_texcoord);
	color = vec4(sqrt(l_color.xyz), 1.0) * c_color;
}
