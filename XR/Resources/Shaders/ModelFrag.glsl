//#version 450 core

#define NR_POINT_LIGHTS 4

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

const vec2 lightBias = vec2(0.8f, 0.6f);

out vec4 FragColor;

in vec3 _Normal;                    
in vec3 _FragPos;                  
in vec2 _TexCoord;
in vec3 _TangentFragPos;
in vec3 _TangentViewPos;
in vec3 _TangentLightPos[NR_POINT_LIGHTS];

uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform vec3 viewPos;         
uniform vec3 cameraPos;

uniform int hasTextureDiffuse;
uniform sampler2D textureDiffuse;
uniform vec3 colorDiffuse;   

uniform int hasTextureSpecular;
uniform sampler2D textureSpecular;
uniform vec3 colorSpecular;   

uniform int hasTextureNormal;
uniform sampler2D textureNormal;

vec3 colorDiff;
vec3 colorSpec;
vec3 colorNorm;

vec3 CalcPointLight( int i)
{
// prepare
    vec3 norm = normalize(_Normal);
    vec3 lightDir = normalize(pointLights[i].position - _FragPos);
    vec3 viewDir = normalize(viewPos - _FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);

 //ambient
    vec3 ambient = pointLights[i].ambient * colorDiff;

//diffuse   
    float diff = max(dot(-lightDir, norm), 0.0) * lightBias.x + lightBias.y;
    vec3 diffuse =  pointLights[i].diffuse * diff * colorDiff;

// specular
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = pointLights[i].specular * spec * colorSpec;

// attenuation
    float distance = length(pointLights[i].position - _FragPos);
    float attenuation = 1.0 / (pointLights[i].constant + pointLights[i].linear * distance + pointLights[i].quadratic * (distance * distance));    

// combine results
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    vec3 result = ambient + diffuse + specular;
    return result;
}

vec3 CalcPointLightNormal(int i)
{
 //ambient
    vec3 ambient = pointLights[i].ambient * colorDiff;

//diffuse   
    vec3 lightDir = normalize(_TangentLightPos[i] - _TangentFragPos);
    float diff = max(dot(colorNorm, lightDir), 0.0); // * lightBias.x + lightBias.y;
    vec3 diffuse =  pointLights[i].diffuse * diff * colorDiff;

// specular
	vec3 viewDir = normalize(_TangentViewPos - _TangentFragPos);
    vec3 reflectDir = reflect(-lightDir, colorNorm);
    vec3 halfwayDir = normalize(lightDir + viewDir);      
    float spec = pow(max(dot(halfwayDir, colorNorm), 0.0), 32);
    vec3 specular = pointLights[i].specular * spec * colorSpec;

// attenuation
    float distance = length(_TangentLightPos[i] - _TangentFragPos);
    float attenuation = 1.0 / (pointLights[i].constant + pointLights[i].linear * distance + pointLights[i].quadratic * (distance * distance));    

// combine results
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    vec3 result = ambient + diffuse + specular;
    return result;
}

void main()
{ 
    if(hasTextureDiffuse == 1)
    {            
        if(texture(textureDiffuse, _TexCoord).a < 0.1) discard; // Opacity	    
        colorDiff = vec3(texture(textureDiffuse, _TexCoord));
    }
    else 
    {
	    colorDiff = colorDiffuse;
    }
    
    if(hasTextureSpecular == 1)
    {
        colorSpec = vec3(texture(textureSpecular, _TexCoord));
    }
    else
    {
        colorSpec = colorSpecular;
    }

    colorNorm = vec3(0.5f, 0.5f, 1f);
    if(hasTextureNormal == 1)
    {
        colorNorm = vec3(texture(textureNormal, _TexCoord));
    }
    colorNorm = normalize(colorNorm * 2.0 -1.0);
    
    vec3 finalResult = vec3(0);	
    if(hasTextureNormal == 1)
    {
        for(int i = 0; i < NR_POINT_LIGHTS; i++)
	    {    
            finalResult += CalcPointLightNormal(i);
        }
    }
    else
    {
        for(int i = 0; i < NR_POINT_LIGHTS; i++)
	    {    
            finalResult = CalcPointLight(i);
        }
    }
    
    FragColor = vec4(finalResult, 1.0);

}

