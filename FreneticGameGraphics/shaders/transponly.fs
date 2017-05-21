
#version 430 core
// transponly.fs

#define MCM_GOOD_GRAPHICS 0
#define MCM_LIT 0
#define MCM_SHADOWS 0
#define MCM_LL 0
#define MCM_ANY 0
#define MCM_GEOM_ACTIVE 0
#define MCM_FADE_DEPTH 0

#define AB_SIZE 16
#define P_SIZE 4

// TODO: more dynamically defined?
#define ab_shared_pool_size (8 * 1024 * 1024)

#if MCM_GEOM_ACTIVE
layout (binding = 0) uniform sampler2DArray tex;
#if MCM_FADE_DEPTH
layout (binding = 1) uniform sampler2D depth_tex;
#endif
#else
layout (binding = 0) uniform sampler2D tex;
layout (binding = 1) uniform sampler2D normal_tex;
#endif
// TODO: Spec, refl!?
layout (binding = 3) uniform sampler2DArray shadowtex;
#if MCM_LL
layout(size1x32, binding = 4) coherent uniform uimage2DArray ui_page;
layout(size4x32, binding = 5) coherent uniform imageBuffer uib_spage;
layout(size1x32, binding = 6) coherent uniform uimageBuffer uib_llist;
layout(size1x32, binding = 7) coherent uniform uimageBuffer uib_cspage;
#endif

const int LIGHTS_MAX = 20; // How many lights we can ever have.

layout (location = 4) uniform float desaturationAmount = 1.0;
// ...
layout (location = 8) uniform vec2 u_screensize = vec2(1024, 1024);
layout (location = 9) uniform mat4 lights_used_helper;
// ...
layout (location = 16) uniform float minimum_light;
// ...
layout (location = 20) uniform mat4 shadow_matrix_array[LIGHTS_MAX];
layout (location = 40) uniform mat4 light_details_array[LIGHTS_MAX];
layout (location = 60) uniform mat4 light_details2_array[LIGHTS_MAX];

#if MCM_GEOM_ACTIVE
in struct vox_fout
#else
in struct vox_out
#endif
{
	vec4 position;
#if MCM_GEOM_ACTIVE
	vec3 texcoord;
#else
	vec2 texcoord;
#endif
	vec4 color;
	mat3 tbn;
	vec2 scrpos;
	float z;
#if MCM_FADE_DEPTH
	float size;
#endif
#if MCM_GEOM_ACTIVE
} fi;

#define f fi

#else
} f;
#endif

#if MCM_LL
#else
out vec4 fcolor;
#endif

vec3 desaturate(vec3 c)
{
	return mix(c, vec3(0.95, 0.77, 0.55) * dot(c, vec3(1.0)), desaturationAmount);
}

float linearizeDepth(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * lights_used_helper[0][1]) / (lights_used_helper[0][2] + lights_used_helper[0][1] - rinput * (lights_used_helper[0][2] - lights_used_helper[0][1]));
}

void main()
{
#if MCM_LL
	vec4 fcolor;
#endif
	vec4 tcolor = texture(tex, f.texcoord);
#if MCM_ANY
#else
	if (tcolor.w * f.color.w >= 0.99)
	{
		discard;
	}
#endif
	if (tcolor.w * f.color.w < 0.01)
	{
		discard;
	}
	vec4 color = tcolor * f.color;
	fcolor = color;
#if MCM_LIT
	fcolor = vec4(0.0);
#if MCM_GEOM_ACTIVE
	vec3 norms = vec3(0.0, 0.0, 1.0);
#else
	vec3 norms = texture(normal_tex, f.texcoord).xyz * 2.0 - 1.0;
#endif
	int count = int(lights_used_helper[0][0]);
	for (int i = 0; i < count; i++)
	{
	mat4 light_details = light_details_array[i];
	mat4 light_details2 = light_details2_array[i];
	mat4 shadow_matrix = shadow_matrix_array[i];
	// Loop body
	float light_radius = light_details[0][0];
	vec3 diffuse_albedo = vec3(light_details[0][1], light_details[0][2], light_details[0][3]);
	vec3 specular_albedo = vec3(light_details[1][1], light_details[1][2], light_details[1][3]);
	float light_type = light_details[1][3];
	float should_sqrt = light_details[2][0];
	float tex_size = light_details[2][1];
	float depth_jump = light_details[2][2];
	float lightc = light_details[2][3];
	if (minimum_light > 0.99)
	{
		fcolor += vec4(color.xyz / lightc, color.w);
		continue;
	}
	vec4 bambient = (vec4(light_details[3][0], light_details[3][1], light_details[3][2], 1.0)
		+ vec4(minimum_light, minimum_light, minimum_light, 0.0)) / lightc;
	vec3 eye_pos = vec3(light_details2[0][0], light_details2[0][1], light_details2[0][2]);
	vec3 light_pos = vec3(light_details2[1][0], light_details2[1][1], light_details2[1][2]);
	float exposure = light_details2[2][0];
	vec3 light_color = vec3(light_details2[0][3], light_details2[2][1], light_details2[2][2]);
	vec4 x_spos = shadow_matrix * vec4(f.position.xyz, 1.0);
	vec3 N = -normalize(f.tbn * norms);
	vec3 light_path = light_pos - f.position.xyz;
	float light_length = length(light_path);
	float d = light_length / light_radius;
	float atten = clamp(1.0 - (d * d), 0.0, 1.0);
	if (light_type == 1.0)
	{
		vec4 fst = x_spos / x_spos.w;
		atten *= 1 - (fst.x * fst.x + fst.y * fst.y);
		if (atten < 0)
		{
			atten = 0;
		}
	}
#if MCM_SHADOWS
	if (should_sqrt >= 1.0)
	{
		x_spos.x = sign(x_spos.x) * sqrt(abs(x_spos.x));
		x_spos.y = sign(x_spos.y) * sqrt(abs(x_spos.y));
	}
	vec4 fs = x_spos / x_spos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	fs.w = 1.0;
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0)
	{
		//fcolor += vec4(0.0, 0.0, 0.0, color.w);
		continue;
	}
#if MCM_GOOD_GRAPHICS
	vec2 dz_duv;
	vec3 duvdist_dx = dFdx(fs.xyz);
	vec3 duvdist_dy = dFdy(fs.xyz);
	dz_duv.x = duvdist_dy.y * duvdist_dx.z - duvdist_dx.y * duvdist_dy.z;
	dz_duv.y = duvdist_dx.x * duvdist_dy.z - duvdist_dy.x * duvdist_dx.z;
	float tlen = (duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x);
	dz_duv /= tlen;
	float oneoverdj = 1.0 / depth_jump;
	float jump = tex_size * depth_jump;
	float depth = 0.0;
	float depth_count = 0.0;
	// TODO: Make this more efficient
	for (float x = -oneoverdj * 2; x < oneoverdj * 2 + 1; x++)
	{
		for (float y = -oneoverdj * 2; y < oneoverdj * 2 + 1; y++)
		{
			float offz = dot(dz_duv, vec2(x * jump, y * jump)) * 1000.0;
			if (offz > -0.000001)
			{
				offz = -0.000001;
			}
			offz -= 0.001;
			float rd = texture(shadowtex, vec3(fs.x + x * jump, -(fs.y + y * jump), float(i))).r;
			depth += (rd >= (fs.z + offz) ? 1.0 : 0.0);
			depth_count++;
		}
	}
	depth = depth / depth_count;
#else // good graphics
	float rd = texture(shadowtex, vec3(fs.x, fs.y, float(i))).r;
	float depth = (rd >= (fs.z - 0.001) ? 1.0 : 0.0);
#endif // else-good graphics
	vec3 L = light_path / light_length;
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec3 specular = vec3(pow(max(dot(reflect(L, N), normalize(f.position.xyz - eye_pos)), 0.0), /* renderhint.y * 1000.0 */ 128.0) * specular_albedo * /* renderhint.x */ 0.0);
	fcolor += vec4((bambient * color + (vec4(depth, depth, depth, 1.0) * atten * (diffuse * vec4(light_color, 1.0)) * color) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten * depth)).xyz, color.w);
#else // shadows
	vec4 fs = x_spos / x_spos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	fs.w = 1.0;
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0)
	{
		fcolor += vec4(0.0, 0.0, 0.0, color.w);
		continue;
	}
	vec3 L = light_path / light_length;
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec3 specular = vec3(pow(max(dot(reflect(L, N), normalize(f.position.xyz - eye_pos)), 0.0), /* renderhint.y * 1000.0 */ 128.0) * specular_albedo * /* renderhint.x */ 0.0);
	fcolor += vec4((bambient * color + (vec4(1.0) * atten * (diffuse * vec4(light_color, 1.0)) * color) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten)).xyz, color.w);
#endif // else-shadows
	}
#endif // lit
	float dist = linearizeDepth(gl_FragCoord.z);
#if MCM_GOOD_GRAPHICS
	fcolor.xyz = desaturate(fcolor.xyz);
#endif
	vec4 fogCol = lights_used_helper[3];
	float fogMod = dist * exp(fogCol.w) * fogCol.w;
	float fmz = min(fogMod, 1.0);
	// TODO: apply fog to things that aren't clouds only!
	//fcolor.xyz = fcolor.xyz * (1.0 - fmz) + fogCol.xyz * fmz + vec3(fogMod - fmz);
	fcolor = vec4(fcolor.xyz, tcolor.w * f.color.w);
#if MCM_FADE_DEPTH
	vec2 fc_xy = gl_FragCoord.xy / vec2(lights_used_helper[0][3], lights_used_helper[1][0]);
	float depthval = linearizeDepth(texture(depth_tex, fc_xy).x);
	fcolor.w *= min(max((depthval - dist) * fi.size * 0.5 * (lights_used_helper[0][2] - lights_used_helper[0][1]), 0.0), 1.0);
#endif
#if MCM_LL
	uint page = 0;
	uint frag = 0;
	uint frag_mod = 0;
	ivec2 scrpos = ivec2(f.scrpos * u_screensize);
	int i = 0;
	while (imageAtomicExchange(ui_page, ivec3(scrpos, 2), 1U) != 0U && i < 100) // TODO: 100 -> uniform var?!
	{
		memoryBarrier();
		i++;
	}
	/*if (i == 1000)
	{
		return;
	}*/
	page = imageLoad(ui_page, ivec3(scrpos, 0)).x;
	frag = imageLoad(ui_page, ivec3(scrpos, 1)).x;
	frag_mod = frag % P_SIZE;
	if (frag_mod == 0)
	{
		uint npage = imageAtomicAdd(uib_cspage, 0, P_SIZE);
		if (npage < ab_shared_pool_size)
		{
			imageStore(uib_llist, int(npage / P_SIZE), uvec4(page, 0U, 0U, 0U));
			imageStore(ui_page, ivec3(scrpos, 0), uvec4(npage, 0U, 0U, 0U));
			page = npage;
		}
		else
		{
			page = 0;
		}
	}
	if (page > 0)
	{
		imageStore(ui_page, ivec3(scrpos, 1), uvec4(frag + 1, 0U, 0U, 0U));
	}
	frag = frag_mod;
	memoryBarrier();
	imageAtomicExchange(ui_page, ivec3(scrpos, 2), 0U);
	vec4 abv = fcolor;
	abv.z = float(int(fcolor.z * 255) & 255 | int(fcolor.w * 255 * 255) & (255 * 255));
	abv.w = f.z;
	imageStore(uib_spage, int(page + frag), abv);
#endif
}
