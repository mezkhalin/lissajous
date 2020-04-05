#version 330

#define THRESHOLD vec4(0)

layout(location=0) out vec4 outputColor;

uniform sampler2D Source;
in vec2 UV;

void main(void)
{
	vec4 op = vec4(0, 0, 0, 1);
	vec4 frag = texture(Source, UV);

	if(
		frag.r >= THRESHOLD.r &&
		frag.g >= THRESHOLD.g &&
		frag.b >= THRESHOLD.b &&
		frag.a >= THRESHOLD.a
	)
		op = frag;

    outputColor = op;
}