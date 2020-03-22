#version 330 core

#define MAX_DIST 0.095

in vec2 aPosition;
in vec2 aNext;
in vec3 aNormal;

out vec4 opColor;

void main(void)
{
	float fac = 1 - clamp((MAX_DIST - distance(aPosition, aNext)) / MAX_DIST, 0.1, .85);
    gl_Position = vec4(aPosition + (aNormal.xy * aNormal.z), 0.0, 1.0);
	opColor = vec4(.2, 1.0, .2, .3 * (1 - (fac * fac * fac)));
}