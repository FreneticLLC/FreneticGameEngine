//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout (binding = 0) uniform sampler2D light_tex;

layout (location = 3) uniform vec2 l_scaler;
layout (location = 4) uniform vec2 l_adder;
layout (location = 5) uniform float light_width;
layout (location = 6) uniform vec4 light_color;
layout (location = 7) uniform float aspect;
layout (location = 8) uniform float sub_divider = 4.0;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

void main()
{
	vec2 lmat = (f_texcoord * vec2(2.0) + vec2(-1.0)) * l_scaler + l_adder;
	lmat.y /= aspect;
	float modif = max(0.95 - dot(lmat, lmat), 0.0);
	//color = vec4(modif, modif, modif, 1.0); return;
	//color = texture(light_tex, f_texcoord); return;
	if (modif < 0.01)
	{
		discard;
	}
	float lmat_len = length(lmat);
	vec2 move = ((-lmat) / lmat_len) * (1.0 / light_width) * sub_divider;
	int reps = int(lmat_len * light_width / sub_divider);
	float sdx = (sub_divider / 4.0);
	for (int i = 0; i < reps; i++)
	{
		lmat += move;
		vec4 col_read = texture(light_tex, lmat * vec2(0.5) + vec2(0.5));
		modif *= 1.0 - (col_read.w * 0.15 * sdx);
	}
	vec4 c = light_color * modif;
	color = vec4(c.xyz * c.xyz, c.w);
}
