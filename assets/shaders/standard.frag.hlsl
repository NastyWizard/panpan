struct FragInput
{
    float4 color : COLOR0;
};

float4 main(FragInput input) : SV_TARGET
{
    return input.color;
}
