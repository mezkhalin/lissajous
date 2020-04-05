#version 330 core

layout(location=0) in vec2 Position;
out vec2 UV;

void main(void)
{
	gl_Position = vec4(Position, 0.0, 1.0);
	UV = (Position.xy + vec2(1.0)) * 0.5;
}