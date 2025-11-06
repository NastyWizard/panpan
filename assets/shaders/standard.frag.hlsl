struct FragInput
{
    float4 color : COLOR0;
    float2 uv    : TEXCOORD0;
};

//cbuffer GlobalUniforms : register(b0, space3)
//{
//    float time;
//    float4 color;
//}

Texture2D mainTexture : register(t0, space2);
SamplerState mainSampler : register(s0, space2);

float4 main(FragInput input) : SV_TARGET
{
    float2 uv = input.uv;
    float4 col = mainTexture.Sample(mainSampler, uv);

    if(col.a < 0.01)
        discard;
    return col;
}
