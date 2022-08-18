//#version 330 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;

out vec2 _texCoord;

uniform mat4 modelMatrix;

void main(void)
{
	gl_Position = modelMatrix * position;
	_texCoord = texCoord;
}
