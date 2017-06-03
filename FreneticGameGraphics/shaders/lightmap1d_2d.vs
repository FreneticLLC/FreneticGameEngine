#version 430 core

layout (location = 0) in vec3 position;
layout (location = 2) in vec2 texcoords;
layout (location = 4) in vec4 color;

layout (location = 1) uniform vec2 scaler = vec2(1.0);
layout (location = 2) uniform vec2 adder = vec2(0.0);
layout (location = 3) uniform vec4 v_color = vec4(1.0);
layout (location = 4) uniform vec3 rotation = vec3(0.0);
// ...
layout (location = 6) uniform vec2 light_pos = vec2(0.0);
layout (location = 7) uniform float light_size = 100.0;
layout (location = 8) uniform vec2 l_scaler = vec2(1.0);
layout (location = 9) uniform vec2 l_adder = vec2(0.0);
layout (location = 10) uniform float aspect;

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec2 f_texcoord;

void main()
{
	f_color = color * v_color;
	f_texcoord = texcoords;
	vec3 rotter = vec3(rotation.xy, 0.0) * vec3(scaler, 1.0);
	vec3 prerot_pos = (position * vec3(scaler, 1.0)) + rotter;
	float cosrot = cos(rotation.z);
	float sinrot = sin(rotation.z);
	prerot_pos = vec3(prerot_pos.x * cosrot - prerot_pos.y * sinrot, prerot_pos.y * cosrot + prerot_pos.x * sinrot, prerot_pos.z);
	vec4 resPos = vec4(prerot_pos - rotter, 1.0) + vec4(adder, 0.0, 0.0);
	vec2 tresPos = resPos.xy * l_scaler + l_adder;
	tresPos.y /= aspect;
	float heightZ = dot(tresPos, tresPos);
	gl_Position = vec4(atan(tresPos.y, tresPos.x) * 100.0 + 320.0, 0.0, heightZ * 2.0 - 1.0, 1.0);
	gl_Position.x = gl_VertexID % 2 == 0 ? 0.0 : 1200.0;
}
