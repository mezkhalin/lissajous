#version 330

in vec4 opColor;
out vec4 outputColor;

void main()
{
    outputColor = vec4(opColor.xyz, opColor.w * opColor.w); //vec4(0.04, .5, 0.04, .4);
}