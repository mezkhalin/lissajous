#version 330 core

#define MAX_DIST 0.05
#define MAX_ALPHA 0.5
#define MIN_ALPHA 0.25

in vec2 aPosition;
in vec2 aNext;
in vec3 aNormal;

out vec4 opColor;

void main(void)
{
    gl_Position = vec4(aPosition + (aNormal.xy * aNormal.z), 0.0, 1.0);

	float distFac = distance(aNext, aPosition) / MAX_DIST;
	distFac = clamp(distFac, 0.0, 1.0);
	distFac = 1.0 - (distFac * distFac);

	opColor = vec4(.2, 1.0, .2, distFac * (MAX_ALPHA - MIN_ALPHA) + MIN_ALPHA);
}