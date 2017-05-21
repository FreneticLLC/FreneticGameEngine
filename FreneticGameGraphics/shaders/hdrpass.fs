
#version 430 core

layout(binding = 0) uniform sampler2D lighttex;

const float SPREAD = 4.0;

layout (location = 4) uniform vec2 u_screensize = vec2(1024, 1024);

layout (location = 1) in vec2 f_scrpos;

out float color;

void main()
{
	float tcur = 0.0;
	float px = 0;
	vec2 jump = SPREAD / u_screensize;
	for (float x = 0.0; x < (1.0 / SPREAD); x += jump.x)
	{
		for (float y = 0.0; y < (1.0 / SPREAD); y += jump.y)
		{
			vec3 col = texture(lighttex, f_scrpos + vec2(x, y)).xyz;
			tcur += max(col.x, max(col.y, col.z));
			px++;
		}
	}
	color = tcur / px;
}
