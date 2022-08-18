//#version 330 core

layout (location = 0) in vec2 position;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main()
{
    gl_Position = projectionMatrix * viewMatrix * vec4(position.x, 0.0f, position.y, 1.0f);    
}