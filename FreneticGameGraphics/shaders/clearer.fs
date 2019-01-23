//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define P_SIZE 4

layout(size1x32, binding = 4) coherent uniform uimage2DArray ui_page;
layout(size4x32, binding = 5) coherent uniform imageBuffer uib_spage;
layout(size1x32, binding = 6) coherent uniform uimageBuffer uib_llist;
layout(size1x32, binding = 7) coherent uniform uimageBuffer uib_cspage;

layout (location = 4) uniform vec2 u_screensize = vec2(1024, 1024);

layout (location = 1) in vec2 f_scrpos;

void main()
{
	ivec2 scrpos = ivec2(f_scrpos * u_screensize);
	imageStore(ui_page, ivec3(scrpos, 0), uvec4(0U, 0U, 0U, 0U));
	imageStore(ui_page, ivec3(scrpos, 1), uvec4(0U, 0U, 0U, 0U));
	imageStore(ui_page, ivec3(scrpos, 2), uvec4(0U, 0U, 0U, 0U));
	//imageStore(uib_spage, 0, vec4(0, 0, 4.0, 1.0));
	//imageStore(uib_llist, 0, uvec4(0U, 0U, 0U, 0U));
	imageStore(uib_cspage, 0, uvec4(P_SIZE, 0U, 0U, 0U));
	discard;
}
