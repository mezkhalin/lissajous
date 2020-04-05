#version 330

#define E 2.71828182846
#define KERNEL_SIZE 40

layout(location=0) out vec4 outputColor;

uniform sampler2D Source;
uniform bool Horizontal = true;
uniform float StdDevSqrd = 0;
uniform float PiFact = 0;
in vec2 UV;

vec2 texelSize;

void main(void)
{
	ivec2 size = textureSize(Source, 0);
	texelSize = vec2(1.0) / size;

	vec4 combined = vec4(0.0);
	vec2 offset = vec2(0.0);
	float stdDevTwo = StdDevSqrd * 2.0;
	float sum = 0;

	if(Horizontal) offset.x = texelSize.x;
	else offset.y = texelSize.y;

	for(int i = 0; i < KERNEL_SIZE; i++)
	{
		float fac = (i / (KERNEL_SIZE));
		vec2 uv = offset * i;

		float gauss = PiFact * pow(E, -((fac * fac) / stdDevTwo) );
		sum += gauss + gauss;
		combined += texture(Source, UV - uv) * gauss + texture(Source, UV + uv) * gauss;
	}

    outputColor = combined / sum;
}