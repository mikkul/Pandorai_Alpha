#if OPENGL
	#define PS_SHADERMODEL ps_3_0
#else
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;
float animationOffset;
float distortionScale;

float rand(float seed)
{
    return frac(sin(seed) * 43758.5453);
}

float3 rand3(float3 vec)
{
    vec = float3( dot(vec,float3(127.1,311.7, 74.7)),
                  dot(vec,float3(269.5,183.3,246.1)),
                  dot(vec,float3(113.5,271.9,124.6)));
    return -1.0 + 2.0*frac(sin(vec)*43758.5453123);
}

float noise(float seed)
{
	float i = floor(seed);
	float f = frac(seed);

	float u = f*f*(3.0-2.0*f);

	return lerp(rand(i), rand(i + 1.0), u);
}

float noise3D(float3 vec) 
{
    float3 i = floor(vec);
    float3 f = frac(vec);

    float3 u = f*f*(3.0-2.0*f);

    // gradients
    float3 ga = rand3( i+float3(0.0,0.0,0.0) );
    float3 gb = rand3( i+float3(1.0,0.0,0.0) );
    float3 gc = rand3( i+float3(0.0,1.0,0.0) );
    float3 gd = rand3( i+float3(1.0,1.0,0.0) );
    float3 ge = rand3( i+float3(0.0,0.0,1.0) );
	float3 gf = rand3( i+float3(1.0,0.0,1.0) );
    float3 gg = rand3( i+float3(0.0,1.0,1.0) );
    float3 gh = rand3( i+float3(1.0,1.0,1.0) );
    
    // projections
    float va = dot( ga, f-float3(0.0,0.0,0.0) );
    float vb = dot( gb, f-float3(1.0,0.0,0.0) );
    float vc = dot( gc, f-float3(0.0,1.0,0.0) );
    float vd = dot( gd, f-float3(1.0,1.0,0.0) );
    float ve = dot( ge, f-float3(0.0,0.0,1.0) );
    float vf = dot( gf, f-float3(1.0,0.0,1.0) );
    float vg = dot( gg, f-float3(0.0,1.0,1.0) );
    float vh = dot( gh, f-float3(1.0,1.0,1.0) );

    return va + u.x*(vb-va) + u.y*(vc-va) + u.z*(ve-va) + u.x*u.y*(va-vb-vc+vd) + u.y*u.z*(va-vc-ve+vg) + u.z*u.x*(va-vb-ve+vf) + (-va+vb+vc-vd+ve-vf-vg+vh)*u.x*u.y*u.z;
}

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float magnitude = noise3D(float3(coords, animationOffset)) * distortionScale;
    float2 offset = normalize(float2(noise(animationOffset), noise(4658.57 + animationOffset))) * magnitude;
    float4 color = tex2D(s0, coords + offset);

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}