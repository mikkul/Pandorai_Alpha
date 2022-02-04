#if OPENGL
	#define PS_SHADERMODEL ps_3_0
#else
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;
float ambientLight;
Texture2D intensityMask;
Texture2D colorMask;
sampler2D intensityMaskSampler = sampler_state { Texture = <intensityMask>; };
sampler2D colorMaskSampler = sampler_state { Texture = <colorMask>; };

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);
    float4 intensity = tex2D(intensityMaskSampler, coords);
    float4 lightingColor = tex2D(colorMaskSampler, coords);

    return color * (intensity + ambientLight) + lightingColor * intensity;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}