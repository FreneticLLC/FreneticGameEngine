//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

vec3 apply_plant_wind(in vec3 pos, in vec3 pivot, in float phase) // Vertex wind for dynamic plant leaves. Pivot in tangent.xyz, phase in texcoords.z.
{
	float ts = time * plant_wind_speed;
	float swayX = plant_wind_strength * sin(ts + phase);
	float swayZ = plant_wind_strength * sin(ts * 0.7 + phase + 1.0);
	float cosX = cos(swayX);
	float sinX = sin(swayX);
	float cosZ = cos(swayZ);
	float sinZ = sin(swayZ);
	mat3 rot = mat3(
		cosZ, sinX * sinZ, cosX * sinZ,
		0.0, cosX, -sinX,
		-sinZ, sinX * cosZ, cosX * cosZ);
	return pivot + rot * (pos - pivot);
}
