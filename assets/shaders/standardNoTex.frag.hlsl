struct FragInput
{
    float4 color : COLOR0;
    float2 uv    : TEXCOORD0;
};

cbuffer GlobalUniforms : register(b0, space3)
{
    float4 color;
}

float4 main(FragInput input) : SV_TARGET
{
    return color;
}
