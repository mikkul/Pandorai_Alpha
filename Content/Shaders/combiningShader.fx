#if OPENGL
	#define PS_SHADERMODEL ps_3_0
#else
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;
Texture2D background;
sampler2D backgroundSampler = sampler_state { Texture = <background>; };

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);
    float4 bgColor = tex2D(backgroundSampler, coords);
    float4 newColor = float4(bgColor.r, bgColor.g, bgColor.b + 0.5, 1);

    return newColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}