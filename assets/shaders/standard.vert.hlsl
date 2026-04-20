struct VertexInput
{
    float3 position : POSITION;
    float4 color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

struct VertexOutput
{
    float4 position : SV_POSITION;
    float4 color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

cbuffer CameraBuffer : register(b0, space1)
{
    float4x4 viewProjection;
}

cbuffer GlobalUniforms : register(b1, space1)
{
    float4x4 model;
}

VertexOutput main(VertexInput input)
{
    VertexOutput output;
    output.position = mul(viewProjection, mul(model, float4(input.position, 1.0f)));
    output.color = input.color;
    output.uv = input.uv;
    return output;
}