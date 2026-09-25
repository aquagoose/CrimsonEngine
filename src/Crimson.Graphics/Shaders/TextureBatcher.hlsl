#include "Core.hlsli"

struct QuadVertex
{
    float2 Position: POSITION0;
    float2 TexCoord: TEXCOORD0;
    float4 Tint:     COLOR0;
};

struct VSOutput
{
    float4 Position: SV_Position;
    float2 TexCoord: TEXCOORD0;
    float4 Tint:     COLOR0;
};

struct BatcherMatrices
{
    float4x4 Projection;
    float4x4 Transform;
};

VertexUniform(BatcherMatrices, camera, 0)
PixelSampler2D(sprite, 0)

VSOutput VSMain(const in QuadVertex input)
{
    VSOutput output;

    output.Position = mul(camera.Projection, mul(camera.Transform, float4(input.Position, 0.0, 1.0)));
    output.TexCoord = input.TexCoord;
    output.Tint = input.Tint;

    return output;
}

float4 PSMain(const in VSOutput input): SV_Target0
{
    return SampleTexture(sprite, input.TexCoord) * input.Tint;
}