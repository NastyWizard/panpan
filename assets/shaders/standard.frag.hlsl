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

float4 main(FragInput input) : SV_TARGET
{
    float4 col = color;
    col.r = sin(time + input.uv.x);
    return col;
}
