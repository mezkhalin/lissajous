#version 330 core

#define MAX_DIST 0.022
#define MAX_ALPHA 0.06
#define MIN_ALPHA 0.006

uniform vec3 Color;

in vec2 aPosition;
in vec2 aNext;
in vec3 aNormal;

out vec4 opColor;

void main(void)
{
    gl_Position = vec4(aPosition + (aNormal.xy * aNormal.z), 0.0, 1.0);

	float distFac = distance(aNext, aPosition) / MAX_DIST;
	distFac = clamp(distFac, 0.0, 1.0);
	//distFac = 1.0 - (distFac * distFac);

	opColor = vec4(Color.r, Color.g, Color.b, (1.0 - distFac) * (MAX_ALPHA - MIN_ALPHA) + MIN_ALPHA);
}