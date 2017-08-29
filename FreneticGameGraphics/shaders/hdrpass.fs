//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

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
