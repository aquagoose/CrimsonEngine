#include "Core.hlsli"

struct Vertex
{
    float2 Position:  TEXCOORD0;
    float2 TexCoord:  TEXCOORD1;
    float4 Tint:      TEXCOORD2;
};

struct VSOutput
{
    float4 Position: SV_Position;
    float2 TexCoord: TEXCOORD0;
    float4 Tint:     COLOR0;
};

VertexUniform(Camera, camera, 0);
PixelSampler2D(sprite, 0);

VSOutput VSMain(const in Vertex input)
{
    VSOutput output;

    output.Position = mul(camera.Projection, mul(camera.View, float4(input.Position, 0.0, 1.0)));
    output.TexCoord = input.TexCoord;
    output.Tint = input.Tint;

    return output;
}

float4 PSMain(const in VSOutput input) : SV_Target0
{
    return sprite.Sample(spriteSampler, input.TexCoord) * input.Tint;
}
