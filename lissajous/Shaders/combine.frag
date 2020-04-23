#version 330

layout(location=0) out vec4 outputColor;

uniform sampler2D Source;
uniform sampler2D Combine;
in vec2 UV;

void main(void)
{
	const float gamma = 0.9;

    vec3 frag = texture(Source, UV).rgb * 0.5 + texture(Combine, UV).rgb * 0.5;

	// reinhard mapping
	frag = frag / (frag + vec3(0.75));
	// gamma correction
	//frag = pow(frag, vec3(1.0 / gamma));

	outputColor = vec4(frag, 1.0);
}