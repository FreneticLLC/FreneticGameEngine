//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout (binding = 0) uniform sampler1D tex;

layout (location = 3) uniform vec2 l_scaler;
layout (location = 4) uniform vec2 l_adder;
// ...
layout (location = 6) uniform vec4 light_color;
layout (location = 7) uniform float aspect;
layout (location = 8) uniform float extra_light_dist = 50.0;
// ...
layout (location = 21) uniform vec4 sky = vec4(0.0);

layout (location = 1) in vec2 f_texcoord;
layout (location = 2) in vec2 f_pos;

out vec4 color;

void main()
{
	vec2 lmat = (f_texcoord * vec2(2.0) + vec2(-1.0)) * l_scaler + l_adder;
	lmat.y /= aspect;
	float modif = 1.0;
	if (sky.w > 0.5)
	{
		float ownDist = lmat.y * lmat.y;
		float xDist = texture(tex, lmat.x + 0.5).x;
		modif *= ownDist >= xDist ? 1.0 - min((ownDist - xDist) * extra_light_dist, 1.0) : 1.0;
	}
	else
	{
		modif *= max(0.95 - dot(lmat, lmat), 0.0);
		if (modif < 0.01)
		{
			discard;
		}
		float ownDist = dot(lmat, lmat);
		float xDist = texture(tex, (atan(lmat.y, lmat.x) * (1.0 / 6.28318) * 0.5) + 0.0).x;
		modif *= ownDist >= xDist ? 1.0 - min((ownDist - xDist) * extra_light_dist, 1.0) : 1.0;
		xDist = texture(tex, (atan(lmat.y, lmat.x) * (1.0 / 6.28318)) * 0.5 + 0.5).x;
		modif *= ownDist >= xDist ? 1.0 - min((ownDist - xDist) * extra_light_dist, 1.0) : 1.0;
		xDist = texture(tex, (atan(lmat.y, lmat.x) * (1.0 / 6.28318)) * 0.5 + 1.0).x;
		modif *= ownDist >= xDist ? 1.0 - min((ownDist - xDist) * extra_light_dist, 1.0) : 1.0;
	}
	if (modif < 0.01)
	{
		discard;
	}
	vec4 c = light_color * modif;
	color = vec4(c.xyz * c.xyz, c.w);
}
