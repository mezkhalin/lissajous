#version 330

layout(location=0) out vec4 outputColor;

uniform sampler2D Source;
uniform bool Horizontal = true;
in vec2 UV;

void main(void)
{
	ivec2 size = textureSize(Source, 0);
	vec2 texelSize = vec2(1.0) / size;

	vec4 combined = vec4(0.0);
	vec2 offset = vec2(0.0);

	if(Horizontal) offset.x = texelSize.x;
	else offset.y = texelSize.y;

	// 0.261182	0.210838	0.110892	0.037984	0.008468	0.001227

	combined += texture(Source, UV - offset * 6) * 0.001227;
	combined += texture(Source, UV - offset * 5) * 0.008468;
	combined += texture(Source, UV - offset * 4) * 0.037984;
	combined += texture(Source, UV - offset * 3) * 0.110892;
	combined += texture(Source, UV - offset * 2) * 0.210838;

	combined += texture(Source, UV) * 0.261182;
	
	combined += texture(Source, UV + offset * 6) * 0.001227;
	combined += texture(Source, UV + offset * 5) * 0.008468;
	combined += texture(Source, UV + offset * 4) * 0.037984;
	combined += texture(Source, UV + offset * 3) * 0.110892;
	combined += texture(Source, UV + offset * 2) * 0.210838;

	outputColor = combined;
}