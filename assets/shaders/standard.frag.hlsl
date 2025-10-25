struct FragInput
{
    float4 color : COLOR0;
    float2 uv    : TEXCOORD0;
};

cbuffer GlobalUniforms : register(b0, space1)
{
    float time;
    float4 color;
}

Texture2D mainTexture : register(t0);
SamplerState mainSampler : register(s0);

float4 main(FragInput input) : SV_TARGET
{
    float2 uv = input.uv;
    uv.x += sin(time + uv.y*3.0f) * 0.1f;
    if(uv.x < 0.0f || uv.y > 1.0f)
        discard;
    float4 col = mainTexture.Sample(mainSampler, uv) * color;
    if(col.a < 0.1)
        discard;
    return col;
}
