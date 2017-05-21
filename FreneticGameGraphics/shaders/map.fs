
#version 430 core

#define MCM_VOX 0

#if MCM_VOX
layout (binding = 0) uniform sampler2DArray s;
#else
layout (binding = 0) uniform sampler2D s;
#endif

in struct vox_out
{
#if MCM_VOX
	vec3 texcoord;
#else
	vec2 texcoord;
#endif
	vec3 tcol;
} f;

layout (location = 0) out vec4 color;

void main()
{
	color = vec4(texture(s, f.texcoord).xyz * f.tcol, 1.0);
}
