#include "../Core3D.hlsli"

struct VSOutput
{
    float4 Position: SV_Position;
    float2 TexCoord: TEXCOORD0;
    float4 Color:    COLOR0;
};

PixelSampler2D(Albedo, 0)

VSOutput VSMain(const in Vertex input)
{
    VSOutput output;
    
    output.Position = mul(Scene.Camera.Projection, mul(Scene.Camera.View, float4(input.Position, 1.0)));
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    
    return output;
}

float4 PSMain(const in VSOutput input): SV_Target0
{
    return SampleTexture(Albedo, input.TexCoord) * input.Color;
}