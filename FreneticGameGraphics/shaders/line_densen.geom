#version 430 core

#define MCM_PRETTY 0

layout (lines) in;
layout (line_strip, max_vertices = 16) out;

layout (location = 10) uniform float aspect;

layout (location = 0) in struct fge_out {
	vec4 f_color;
	vec2 f_texcoord;
} fout[2];

layout (location = 0) out struct fge_in {
	vec4 f_color;
	vec2 f_texcoord;
} fin;

void main()
{
	vec4 colA = fout[0].f_color;
	vec4 colAdd = (fout[1].f_color - colA) * (1.0 / 15.0);
	vec2 tcA = fout[0].f_texcoord;
	vec2 tcAdd = (fout[1].f_texcoord - tcA) * (1.0 / 15.0);
	vec2 posA = gl_in[0].gl_Position.xy;
	vec2 posAdd = (gl_in[1].gl_Position.xy - posA) * (1.0 / 15.0);
	float pA = 0.0;
	vec2 midPos = posA + posAdd * 0.5;
	float absmidA = abs(atan(midPos.y, midPos.x) * (1.0 / 6.28318));
	for (int i = 0; i < 16; i++)
	{
		fin.f_color = colA + (colAdd * float(i));
		fin.f_texcoord = tcA + (tcAdd * float(i));
		vec2 tresPos = posA + (posAdd * float(i));
		float cA = atan(tresPos.y, tresPos.x) * (1.0 / 6.28318);
		float pxN = (cA + 0.5) * 2.0 - 1.0;
		if (i != 0 && absmidA > 0.25 && sign(cA) != sign(pA))
		{
			cA = -cA;
			pxN += sign(pA) + sign(cA);
		}
		gl_Position = vec4(pxN * 0.5, 0.0, dot(tresPos, tresPos) * 2.0 - 1.0, 1.0);
		pA = cA;
		EmitVertex();
	}
	EndPrimitive();
}
