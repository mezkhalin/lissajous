#version 330 core

uniform vec3 Color;
uniform float MaxDist;
uniform float MaxAlpha;
uniform float MinAlpha;

in vec2 aPosition;
in vec2 aNext;
in vec3 aNormal;

out vec4 opColor;

void main(void)
{
    gl_Position = vec4(aPosition + (aNormal.xy * aNormal.z), 0.0, 1.0);

	float alpha = 0.025;

	float distFac = distance(aNext, aPosition) / MaxDist / MaxAlpha * MinAlpha; // REMOVE ALPHA
	distFac = clamp(distFac, alpha, alpha);
	//distFac = 1.0 - (distFac * distFac);

	opColor = vec4(Color.rgb, distFac); // (1.0 - distFac) * (MaxAlpha - MinAlpha) + MinAlpha);
}