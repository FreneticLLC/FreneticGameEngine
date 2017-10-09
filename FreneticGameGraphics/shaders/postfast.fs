//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

layout (binding = 0) uniform sampler2D rht;
layout (binding = 1) uniform sampler2D colortex;
layout (binding = 2) uniform sampler2D normaltex;
layout (binding = 3) uniform sampler2D depthtex;
layout (binding = 4) uniform sampler2D postex;
layout (binding = 5) uniform sampler2D depthtex2;

in struct vox_out
{
	vec2 texcoord;
} f;

layout (location = 5) uniform vec2 zdist = vec2(0.1, 1000.0); // The Z-Near and Z-Far value of the 3D projection.
layout (location = 6) uniform mat4 proj_mat = mat4(1.0);
layout (location = 7) uniform vec2 odist = vec2(0.1, 1000.0); // The Z-Near and Z-Far value of the out-view 3D projection.

out vec4 color;

float linearizeDepth(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * zdist.x) / (zdist.y + zdist.x - rinput * (zdist.y - zdist.x));
}

float linearizeDepthOut(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * odist.x) / (odist.y + odist.x - rinput * (odist.y - odist.x));
}

vec4 raytrace(in vec3 reflectionVector, in float startDepth) // Trace a ray across the screen, for reflection purposes.
{
	float stepSize = 0.01;
	reflectionVector = normalize(reflectionVector) * stepSize;
	vec2 sampledPosition = f.texcoord;
	float currentDepth = startDepth;
	while(sampledPosition.x <= 1.0 && sampledPosition.x >= 0.0 && sampledPosition.y <= 1.0 && sampledPosition.y >= 0.0)
	{
		sampledPosition = sampledPosition + reflectionVector.xy;
		currentDepth = currentDepth + reflectionVector.z * startDepth;
		float sD = texture(depthtex, sampledPosition).x;
		float sampledDepth;
		if (sD > 0.999)
		{
			sD = texture(depthtex2, sampledPosition).x;
			sampledDepth = ((odist.x - zdist.x) / (zdist.y - zdist.x)) + linearizeDepthOut(sD) * ((zdist.y - zdist.x) / (odist.y - odist.x));
			sampledDepth = 1.0 - sampledDepth;
		}
		else
		{
			sampledDepth = linearizeDepth(sD);
		}
		if(currentDepth > sampledDepth)
		{
			float delta = currentDepth - sampledDepth;
			if(delta < 0.03)
			{
				if (sampledDepth < startDepth)
				{
					return vec4(0.0);
				}
				else
				{
					return texture(colortex, sampledPosition);
				}
			}
		}
		if (currentDepth < 0.0 || currentDepth > 2.0)
		{
			return vec4(0.0);
		}
	}
	return vec4(0.0);
}

void main()
{
	vec4 rhc = texture(rht, f.texcoord);
	if (rhc.w < 0.01 || rhc.y < 0.01)
	{
		discard;
	}
	vec3 pos = texture(postex, f.texcoord).xyz;
	vec3 norm = texture(normaltex, f.texcoord).xyz;
	vec3 normal = normalize(norm);
	float currDepth = linearizeDepth(texture(depthtex, f.texcoord).x);
	vec3 eyePosition = normalize(-pos);
	vec4 reflectionVector = proj_mat * vec4(reflect(eyePosition, normal), 0.0);
	reflectionVector.x = 0.0; // TODO: ???
	vec4 SSR = raytrace(reflectionVector.xyz / reflectionVector.w, currDepth);
	color = texture(colortex, f.texcoord);
	if (SSR.w > 0.0)
	{
		float rhy = min(rhc.y, 1.0);
		color = color * (1.0 - rhy) + SSR * rhy; // If we found a reflection, apply it at the strength specified.
	}
}
