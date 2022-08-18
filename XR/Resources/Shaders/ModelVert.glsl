//#version 450 core

layout (location = 0) in vec3 Position;
layout (location = 1) in vec3 Normal;
layout (location = 2) in vec2 TexCoord;
layout (location = 3) in vec3 Tangent;
layout (location = 4) in vec3 Bitangent;
layout (location = 5) in vec4 BoneID;
layout (location = 6) in vec4 Weight;

#define NR_POINT_LIGHTS 4
//#define MAX_BONE 150
#define MAX_WEIGHTS 4

struct PointLight
{
	vec3 position;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
	float constant;
	float linear;
	float quadratic;
};

out vec3 _Normal;
out vec3 _FragPos;
out vec2 _TexCoord;
out vec3 _TangentFragPos;
out vec3 _TangentViewPos;
out vec3 _TangentLightPos[NR_POINT_LIGHTS];

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

uniform vec3 viewPos;
uniform PointLight pointLights[NR_POINT_LIGHTS];

uniform int hasAnimations;
uniform int maxBoneCount;
//uniform mat4 boneTransform[MAX_BONE];

void main()
{
	vec4 worldPosition = vec4(0.0);

	if(hasAnimations == 1) 
	{
		mat4 boneTransformation = mat4(0.0f);
		vec4 normalizedWeight = normalize(Weight);    
		for(int i =0; i<MAX_WEIGHTS;i++)
			boneTransformation += boneTransform[uint(BoneID[i])] * normalizedWeight[i];	            

		worldPosition = modelMatrix * boneTransformation * vec4(Position, 1.0);
	}
	else
	{
		worldPosition = modelMatrix * vec4(Position, 1.0);
	}

	gl_Position = projectionMatrix * viewMatrix * worldPosition;
    _FragPos =  worldPosition.xyz;
	_Normal = Normal * mat3(transpose(inverse(modelMatrix)));
    _TexCoord = TexCoord;
    
	vec3 T = normalize(vec3(modelMatrix * vec4(Tangent, 0.0)));
	vec3 B = normalize(vec3(modelMatrix * vec4(Bitangent, 0.0)));
	vec3 N = normalize(vec3(modelMatrix * vec4(Normal, 0.0)));	
	mat3 TBN = mat3(T, B, N);	
		
	_TangentFragPos = _FragPos * TBN;
	_TangentViewPos = viewPos * TBN;
	for(int i = 0; i < NR_POINT_LIGHTS; i++)
		_TangentLightPos[i] = pointLights[i].position * TBN;	

}
