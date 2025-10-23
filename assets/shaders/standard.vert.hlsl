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

//cbuffer GlobalUniforms : register(b0, space1)
//{
//    float4x4 viewProjection;
//}

VertexOutput main(VertexInput input)
{
    VertexOutput output;
    output.position = float4(input.position, 1.0f); //mul(float4(input.position, 1.0f), viewProjection);
    output.color = float4(1.0f,1.0f,1.0f,1.0f);//input.color;
    output.uv = input.uv;
    return output;
}