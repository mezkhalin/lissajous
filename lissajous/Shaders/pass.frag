#version 330

layout(location=0) out vec4 outputColor;
uniform sampler2D Source;
in vec2 UV;

void main(void)
{
    outputColor = texture(Source, UV);
}