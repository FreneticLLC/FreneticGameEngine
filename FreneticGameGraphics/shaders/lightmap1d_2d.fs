#version 430 core

layout (binding = 0) uniform sampler2D tex;

layout (location = 0) in struct fge_in {
	vec4 f_color;
	vec2 f_texcoord;
} fin;

out float color;

void main()
{
	vec4 tcolor = texture(tex, fin.f_texcoord) * fin.f_color;
	if (tcolor.w < 0.01)
	{
		discard;
	}
	color = gl_FragCoord.z;
}
