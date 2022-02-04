#if OPENGL
	#define PS_SHADERMODEL ps_3_0
#else
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);

    color.rgb = (color.r + color.g + color.b) / 3;
    color.rgb = 1 - color.rgb;
    color.rgb -= 0.6f;
    color.b += 0.10f;
    color.g += 0.10f;

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}