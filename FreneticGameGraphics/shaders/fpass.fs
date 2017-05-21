
#version 430 core

#define AB_SIZE 16
#define P_SIZE 4

vec4 flist[AB_SIZE];

layout(size1x32, binding = 4) coherent uniform uimage2DArray ui_page;
layout(size4x32, binding = 5) coherent uniform imageBuffer uib_spage;
layout(size1x32, binding = 6) coherent uniform uimageBuffer uib_llist;
layout(size1x32, binding = 7) coherent uniform uimageBuffer uib_cspage;

layout (location = 4) uniform vec2 u_screensize = vec2(1024, 1024);

layout (location = 1) in vec2 f_scrpos;

out vec4 color;

void main()
{
	ivec2 scrpos = ivec2(f_scrpos * u_screensize);
	int page = int(imageLoad(ui_page, ivec3(scrpos, 0)).x);
	if (page <= 0)
	{
		discard;
	}
	int numFrags = int(imageLoad(ui_page, ivec3(scrpos, 1)).x);
	if (numFrags <= 0)
	{
		discard;
	}
	int ip = 0;
	int fi = 0;
	while (page != 0 && ip < 20)
	{
		int ne;
		if (ip == 0)
		{
			ne = numFrags % P_SIZE;
			if (ne == 0)
			{
				ne = P_SIZE;
			}
		}
		else
		{
			ne = P_SIZE;
		}
		for (int i = 0; i < ne; i++)
		{
			if (fi < AB_SIZE)
			{
				flist[fi] = imageLoad(uib_spage, int(page + i));
			}
			fi++;
		}
		page = int(imageLoad(uib_llist, int(page / P_SIZE)).x);
		ip++;
	}
	numFrags = min(numFrags, AB_SIZE);
	for (int i = (numFrags - 2); i >= 0; i--)
	{
		for (int j = 0; j <= i; j++)
		{
			if (flist[j].w > flist[j + 1].w)
			{
				vec4 temp = flist[j + 1];
				flist[j + 1] = flist[j];
				flist[j] = temp;
			}
		}
	}
	vec4 tcol = vec4(0.0);
	//float thickness = flist[0].w * 0.5;
	for (int i = 0; i < numFrags; i++)
	{
		vec4 frag = flist[i];
		vec4 c;
		c.xyz = frag.xyz;
		int temp = int(frag.z);
		c.z = float(temp & 255) / 255.0;
		c.w = float((temp / 255) & 255) / 255.0;
		// Uncomment below to replace original alpha with thickness-estimation funtimes
		/*
		const float sig = 30.0;
		c.w = 0.5;
		if (i % 2 == numFrags % 2)
		{
			thickness = (flist[i + 1].w - frag.w) * 0.5;
		}
		c.w = 1.0 - pow(1.0 - c.w, thickness * sig);
		*/
		//c.xyz = c.xyz * c.w;
		tcol += c * (1.0 - tcol.w);
	}
	color = tcol;
}
