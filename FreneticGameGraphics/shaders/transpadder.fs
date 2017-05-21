
#version 430 core

layout (binding = 0) uniform sampler2D transptex;

layout (location = 3) uniform float lcount = 1;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

vec4 regularize(in vec4 input_r) // TODO: Is this working the best it can?
{
	if (input_r.x <= 1.0 && input_r.y <= 1.0 && input_r.z <= 1.0)
	{
		return input_r;
	}
	return vec4(input_r.xyz / max(max(input_r.x, input_r.y), input_r.z), input_r.w > 1.0 ? 1.0 : input_r.w);
}

void main()
{
	vec4 tc = texture(transptex, f_texcoord);
	color = regularize(vec4(tc.x, tc.y, tc.z, tc.w));
}
