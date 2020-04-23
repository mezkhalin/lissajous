#version 330

const float[11] Kernel = float[]( 0.158522, 0.146491, 0.115601, 0.077901, 0.044828, 0.022027, 0.009243, 0.003311, 0.001013, 0.000265, 0.000059 );

layout(location=0) out vec4 outputColor;

uniform sampler2D Source;
uniform bool Horizontal = true;
uniform vec2 texelSize;
in vec2 UV;

void main(void)
{
	vec4 combined = texture(Source, UV) * Kernel[0];
	vec2 offset = vec2(0.0);

	if(Horizontal) offset.x = texelSize.x;
	else offset.y = texelSize.y;

	for(int i = 1; i < 11; i++)
	{
		combined += texture(Source, UV + (offset * i)) * Kernel[i];
		combined += texture(Source, UV - (offset * i)) * Kernel[i];
	}

	// 0.261182	0.210838	0.110892	0.037984	0.008468	0.001227

	/*
	combined += texture(Source, UV - offset * 5) * 0.001227;
	combined += texture(Source, UV - offset * 4) * 0.008468;
	combined += texture(Source, UV - offset * 3) * 0.037984;
	combined += texture(Source, UV - offset * 2) * 0.110892;
	combined += texture(Source, UV - offset) * 0.210838;

	combined += texture(Source, UV) * 0.261182;
	
	combined += texture(Source, UV + offset * 5) * 0.001227;
	combined += texture(Source, UV + offset * 4) * 0.008468;
	combined += texture(Source, UV + offset * 3) * 0.037984;
	combined += texture(Source, UV + offset * 2) * 0.110892;
	combined += texture(Source, UV + offset) * 0.210838;
	*/

	outputColor = vec4(combined.rgb, 1.0);
}