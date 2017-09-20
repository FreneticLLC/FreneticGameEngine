//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout (location = 0) in vec3 position;
layout (location = 2) in vec2 texcoords;
layout (location = 4) in vec4 color;

layout (location = 1) uniform vec3 scaler = vec3(1.0);
layout (location = 2) uniform vec2 adder = vec2(0.0);
layout (location = 3) uniform vec4 v_color = vec4(1.0);

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec2 f_texcoord;

void main()
{
    f_color = color * v_color;
	f_texcoord = texcoords;
	gl_Position = vec4(position, 1.0) * vec4(scaler.xy, 1.0, 1.0) + vec4(adder, 0.0, 0.0);
}
