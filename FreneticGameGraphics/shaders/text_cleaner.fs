//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout (binding = 0) uniform sampler2D tex;

in struct text_fout
{
	vec4 color;
	vec2 texcoord;
} fi;

out vec4 color;

void main()
{
	vec4 tcolor = texture(tex, fi.texcoord);
	color = vec4(fi.color.xyz * tcolor.xyz, ((tcolor.x + tcolor.y + tcolor.z) / 3) * fi.color.w);
}
