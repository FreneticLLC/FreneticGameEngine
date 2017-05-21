
#version 430 core

layout (location = 0) in vec3 position;

layout (location = 1) uniform mat4 projection;
layout (location = 2) uniform mat4 model_matrix;

layout (location = 1) out vec2 f_scrpos;

void main()
{
	vec4 adj = projection * model_matrix * vec4(position, 1.0);
	f_scrpos = adj.xy / adj.w * 0.5 + vec2(0.5);
	gl_Position = adj;
}
