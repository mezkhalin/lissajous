#version 330

layout(location=0) out vec4 outputColor;

uniform sampler2D Source;
uniform float Glow;
in vec2 UV;

void main(void)
{
	vec4 op = vec4(0, 0, 0, 1);
	vec4 frag = texture(Source, UV);
	
	if(
		frag.r >= Glow ||
		frag.g >= Glow ||
		frag.b >= Glow
	) op = frag;

    outputColor = op;
}