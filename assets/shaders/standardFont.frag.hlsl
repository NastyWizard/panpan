struct FragInput
{
    float4 color : COLOR0;
    float2 uv    : TEXCOORD0;
};

Texture2D mainTexture : register(t0, space2);
SamplerState mainSampler : register(s0, space2);

float4 main(FragInput input) : SV_TARGET
{
    float2 uv = input.uv;
    float4 col = mainTexture.Sample(mainSampler, uv);

    float alpha = col.a;
    col = float4(1.0, 1.0, 1.0, alpha);
    return col;
}
