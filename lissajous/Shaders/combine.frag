#version 330

layout(location=0) out vec4 outputColor;

uniform sampler2D Source;
uniform sampler2D Combine;
in vec2 UV;

void main(void)
{
    outputColor = (texture(Source, UV) + texture(Combine, UV));
}