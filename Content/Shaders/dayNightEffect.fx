#if OPENGL
	#define PS_SHADERMODEL ps_3_0
#else
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;
float time;
Texture2D colorMask;
sampler2D colorMaskSampler = sampler_state { Texture = <colorMask>; };

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);

    float2 maskCoords = float2(time, 0);
    float4 maskColor = tex2D(colorMaskSampler, maskCoords);

    return color * maskColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}