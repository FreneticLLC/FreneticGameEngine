//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for contents of the license.
// If neither of these are not available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#define FXAA_SPAN_MAX 8.0
#define FXAA_REDUCE_MUL (1.0 / 8.0)
#define FXAA_REDUCE_MIN (1.0 / 128.0)

// Broken FXAA. For use in post-processing FS files.

vec3 fxaaColor()
{
	float x_adj = 1.0 / WIDTH;
	float y_adj = 1.0 / HEIGHT;
	vec3 light_color = getColor(f_texcoord).xyz;
	vec3 light_colorxpyp = getColor(vec2(f_texcoord.x + x_adj, f_texcoord.y + y_adj)).xyz;
	vec3 light_colorxpym = getColor(vec2(f_texcoord.x + x_adj, f_texcoord.y - y_adj)).xyz;
	vec3 light_colorxmym = getColor(vec2(f_texcoord.x - x_adj, f_texcoord.y - y_adj)).xyz;
	vec3 light_colorxmyp = getColor(vec2(f_texcoord.x - x_adj, f_texcoord.y + y_adj)).xyz;
	vec3 lumaOr = vec3(0.299, 0.587, 0.114);
	float lumaxpyp = dot(light_colorxpyp, lumaOr);
	float lumaxpym = dot(light_colorxpym, lumaOr);
	float lumaxmym = dot(light_colorxmym, lumaOr);
	float lumaxmyp = dot(light_colorxmyp, lumaOr);
	float luma  = dot(light_color,  lumaOr);
	float lumaMin = min(luma, min(min(lumaxpyp, lumaxpym), min(lumaxmym, lumaxmyp)));
	float lumaMax = max(luma, max(max(lumaxpyp, lumaxpym), max(lumaxmym, lumaxmyp)));
	vec2 dir = vec2(-((lumaxpyp + lumaxpym) - (lumaxmym + lumaxmyp)), (lumaxpyp + lumaxmym) - (lumaxpym + lumaxmyp));
	float dirReduce = max((lumaxpyp + lumaxpym + lumaxmym + lumaxmyp) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
	float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
	dir = min(vec2( FXAA_SPAN_MAX,  FXAA_SPAN_MAX), max(vec2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX), dir * rcpDirMin));
	dir.x /= WIDTH;
	dir.y /= HEIGHT;
	vec3 rgbA = 0.5 * (getColor(f_texcoord + dir * (1.0 / 3.0 - 0.5)).xyz + getColor(f_texcoord + dir * (2.0 / 3.0 - 0.5)).xyz);
	vec3 rgbB = rgbA * 0.5 + 0.25 * (getColor(f_texcoord + dir * (-0.5)).xyz + getColor(f_texcoord + dir * 0.5).xyz);
	float lumaB = dot(rgbB, lumaOr);
	if ((lumaB < lumaMin) || (lumaB > lumaMax))
	{
		return rgbA;
	}
	else
	{
		return rgbB;
	}
}
