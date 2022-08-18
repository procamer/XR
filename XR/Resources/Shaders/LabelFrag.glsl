//#version 330 core

out vec4 FragColor;

in vec2 _texCoord;

uniform sampler2D objectTexture;

void main(void)
{
	FragColor = texture(objectTexture, _texCoord);
}
