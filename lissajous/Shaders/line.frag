#version 330

layout(location=0) out vec4 outputColor;
in vec4 opColor;

void main(void)
{
    outputColor = vec4(opColor.xyz, opColor.w * opColor.w); //vec4(0.04, .5, 0.04, .4);
}