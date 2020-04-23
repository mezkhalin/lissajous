#version 330

layout(location=0) out vec4 outputColor;
in vec4 opColor;

void main(void)
{
	float w = 1.0 - opColor.w;
    outputColor = vec4(opColor.xyz, opColor.w); // 1.0 - (w * w * w * w)); //vec4(0.04, .5, 0.04, .4);
}