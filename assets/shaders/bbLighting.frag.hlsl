struct FragInput
{
    float4 color : COLOR0;
    float2 uv    : TEXCOORD0;
};

cbuffer GlobalUniforms : register(b0, space3)
{
    bool debugView;
}

Texture2D mainTexture : register(t0, space2);
SamplerState mainSampler : register(s0, space2);

Texture2D lightTexture : register(t1, space2);
SamplerState lightSampler : register(s1, space2);

Texture2D palletTexture : register(t2, space2);
SamplerState palletSampler : register(s2, space2);

Texture2D targetPalletTexture : register(t3, space2);
SamplerState targetPalletSampler : register(s3, space2);

float4 swapCol(float4 colIn)
{
    [unroll]
    for(int i = 0; i < 16; i++)
    {
        float2 puv = float2(i / 16.0, 0.0);
        float4 source = palletTexture.Sample(palletSampler, puv);
        float4 dest = targetPalletTexture.Sample(targetPalletSampler, puv);
        
        // Compare colors with epsilon for floating point precision
        if(abs(source.r - colIn.r) < 0.01 &&
           abs(source.g - colIn.g) < 0.01 &&
           abs(source.b - colIn.b) < 0.01 &&
           abs(source.a - colIn.a) < 0.01)
        {
            return dest;
        }
    }
    return colIn;
}


float4 demoteColor(float4 colIn, int n)
{
    [unroll]
    for(int i = 0; i < 16; i++)
    {
        float2 puv = float2(i / 16.0, 0.0);
        float4 source = palletTexture.Sample(palletSampler, puv);
        
        int di = i-n;
        if(i >= 8)
            di = max(8.0, di);
        float2 dpuv = float2(max(0.0, (di) / 16.0), 0.0);
        float4 dest = palletTexture.Sample(palletSampler, dpuv);
        
        // Compare colors with epsilon for floating point precision
        if(abs(source.r - colIn.r) < 0.01 &&
           abs(source.g - colIn.g) < 0.01 &&
           abs(source.b - colIn.b) < 0.01 &&
           abs(source.a - colIn.a) < 0.01)
        {
            return dest;
        }
    }
    return colIn;
}

float4 main(FragInput input) : SV_TARGET
{
    float2 uv = input.uv;
    float4 col = mainTexture.Sample(mainSampler, uv);
    float4 lights = lightTexture.Sample(lightSampler, uv);

    float tier = 4 - floor(lights.a * 4);
    col = demoteColor(col,tier);

    if(lights.a < 0.1)
    {
        col = demoteColor(col,3);
    }

    col = swapCol(col);

    // debug
    if(debugView)
    {
        float l = 1.0 - (tier/4);
        col = float4(l,l,l,1.0);
    }
    
    if(col.a < 0.01)
        discard;
    return col;
}
