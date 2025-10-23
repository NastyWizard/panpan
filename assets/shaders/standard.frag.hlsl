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
    float4 col = mainTexture.Sample(mainSampler, input.uv);
    if(col.a < 0.1)
        discard;
    //col.r = sin(time + input.uv.x);
    return col;
}
