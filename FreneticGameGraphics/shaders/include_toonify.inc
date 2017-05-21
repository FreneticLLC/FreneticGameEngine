//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for contents of the license.
// If neither of these are not available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

const float edge_thres = 0.2;
const float edge_thres2 = 5.0;

// TOONIFY: a post process effect for FS files.

#define HueLevCount 7
#define SatLevCount 11
#define ValLevCount 4
const float[HueLevCount] HueLevels = float[] (0.0, 60.0, 120.0, 180.0, 240.0, 300.0, 360.0);
const float[SatLevCount] SatLevels = float[] (0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0);
const float[ValLevCount] ValLevels = float[] (0.0, 0.33, 0.66, 1.0);

vec3 RGBtoHSV(in float r, in float g, in float b)
{
	float minv = min(min(r, g), b);
	float maxv = max(max(r, g), b);
	vec3 res;
	res.z = maxv;
	float delta = maxv - minv;
	if( maxv != 0.0 )
	{
		res.y = delta / maxv;
	}
	else
	{
		res.y = 0.0;
		res.x = -1.0;
		return res;
	}
	if(r == maxv)
	{
		res.x = (g - b) / delta;
	}
	else if(g == maxv)
	{
		res.x = 2.0 + (b - r) / delta;
	}
	else
	{
		res.x = 4.0 + (r - g) / delta;
	}
	res.x = res.x * 60.0;
	if(res.x < 0.0)
	{
      res.x = res.x + 360.0;
	}
	return res;
}

vec3 HSVtoRGB(in float h, in float s, in float v)
{
	if(s == 0.0)
	{
		return vec3(v, v, v);
	}
	float ht = h / 60.0;
	float i = floor(ht);
	float f = ht - i;
	float p = v * (1.0 - s);
	float q = v * (1.0 - s * f);
	float t = v * ( 1.0 - s * (1.0 - f));
	switch(int(i))
	{
		case 0:
			return vec3(v, t, p);
		case 1:
			return vec3(q, v, p);
		case 2:
			return vec3(p, v, t);
		case 3:
			return vec3(p, q, v);
		case 4:
			return vec3(t, p, v);
		default:
			return vec3(v, p, q);
	}
}

float nearestLevel(in float col, in int mode)
{
	int levCount;
	if (mode == 0)
	{
		levCount = HueLevCount;
	}
	else if (mode == 1)
	{
		levCount = SatLevCount;
	}
	else
	{
		levCount = ValLevCount;
	}
	for (int i = 0; i < levCount - 1; i++)
	{
		if (mode == 0)
		{
			if (col >= HueLevels[i] && col <= HueLevels[i + 1])
			{
				return HueLevels[i + 1];
			}
		}
		else if (mode == 1)
		{
			if (col >= SatLevels[i] && col <= SatLevels[i + 1])
			{
				return SatLevels[i + 1];
			}
		}
		else
		{
			if (col >= ValLevels[i] && col <= ValLevels[i + 1])
			{
				return ValLevels[i + 1];
			}
		}
	}
	return 0;
}

float avg_intensity(in vec4 pix) 
{
	return (pix.r + pix.g + pix.b) / 3.0;
}

float IsEdge(in vec2 coords, in float exposure, in float mblen)
{
	float dxtex = 1.0 / width;
	float dytex = 1.0 / height;
	float pix[9];
	int k = -1;
	float delta;
	for (int x = -1; x < 2; x++)
	{
		for(int y = -1; y < 2; y++)
		{
			k++;
			pix[k] = avg_intensity(getColor(coords + vec2(float(x) * dxtex, float(y) * dytex), exposure, mblen)); // TODO: Can we simplify this getColor?
		}
	}
	delta = (abs(pix[1] - pix[7])+ abs(pix[5] - pix[3]) + abs(pix[0] - pix[8])+ abs(pix[2] - pix[6])) / 4.0;
  return clamp(edge_thres2 * delta, 0.0, 1.0);
}
