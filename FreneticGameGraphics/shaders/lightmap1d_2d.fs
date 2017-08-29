//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

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
