//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texcoord;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec4 color;

layout (location = 1) uniform mat4 projection;
layout (location = 2) uniform mat4 model_matrix;

out struct vox_out
{
	vec2 texcoord;
} f;

void main()
{
	f.texcoord = texcoord;
	gl_Position = projection * model_matrix * vec4(position, 1.0);
}
