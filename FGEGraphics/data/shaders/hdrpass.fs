//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout(binding = 0) uniform sampler2D lighttex;

const int SPREAD = 32; // TODO: Uniform?

layout (location = 4) uniform vec2 u_screensize = vec2(1024, 1024);

out float color;

void main()
{
	float tcur = 0.0;
	ivec2 screen_size = ivec2(u_screensize);
	ivec2 tile = ivec2(gl_FragCoord.xy);
	ivec2 start = tile * screen_size / SPREAD;
	ivec2 end = (tile + ivec2(1)) * screen_size / SPREAD;
	for (int y = start.y; y < end.y; y++)
	{
		for (int x = start.x; x < end.x; x++)
		{
			vec3 col = texelFetch(lighttex, ivec2(x, y), 0).xyz;
			tcur += col.x + col.y; // Note: intentionally exclude red channel
		}
	}
	color = tcur * (float(SPREAD * SPREAD) / (2.0 * u_screensize.x * u_screensize.y));
}
