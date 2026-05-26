//
// Vertex wind for dynamic plant leaves. Pivot in tangent.xyz, phase in texcoords.z.
//

layout (location = 22) uniform float plant_wind_strength = 0.08;
layout (location = 23) uniform float plant_wind_speed = 1.6;

vec3 apply_plant_wind(in vec3 pos, in vec3 pivot, in float phase)
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
