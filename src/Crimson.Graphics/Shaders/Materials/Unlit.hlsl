#include "MaterialBase.hlsli"

struct VSOutput
{
    float4 Position: SV_Position;
    float2 TexCoord: TEXCOORD0;
    float4 Color:    COLOR0;
};

VSOutput VSMain(const in Vertex vertex)
{
    VSOutput output;

    output.Position = mul(gCamera.Projection, mul(gCamera.View, float4(vertex.Position, 1.0)));
    output.TexCoord = vertex.TexCoord;
    output.Color = vertex.Color;

    return output;
}

float4 PSMain(const in VSOutput input): SV_Target0
{
    
}