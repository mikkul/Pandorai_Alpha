#if OPENGL
	#define PS_SHADERMODEL ps_3_0
#else
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;
float ambientLight;
float timeOfDay;
Texture2D intensityMask;
Texture2D colorMask;
Texture2D dayNightColorMask;
sampler2D intensityMaskSampler = sampler_state { Texture = <intensityMask>; };
sampler2D colorMaskSampler = sampler_state { Texture = <colorMask>; };
sampler2D dayNightColorMaskSampler = sampler_state { Texture = <dayNightColorMask>; };

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);

    float4 intensity = tex2D(intensityMaskSampler, coords);
    float4 lightingColor = tex2D(colorMaskSampler, coords);

    float grey = dot(intensity * lightingColor, float3(0.333, 0.333, 0.333));

    float2 dayNightMaskCoords = float2(timeOfDay, 0);
    float4 dayNightMaskColor = tex2D(dayNightColorMaskSampler, dayNightMaskCoords);
    float4 dayNightResultColor = color * dayNightMaskColor;

    float4 resultColor = lerp(color, dayNightResultColor * normalize(lightingColor * intensity + 0.05) * 3.0, grey);

    return resultColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}