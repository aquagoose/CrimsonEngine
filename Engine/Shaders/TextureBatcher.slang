#include "Core.slang"

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

struct Camera2D
{
    float4x4 Projection;
    float4x4 Transform;
}

VertexUniform<Camera2D> camera;
PixelTexture<Sampler2D> sprite;

[shader("vertex")]
VSOutput VSMain(const in Vertex input)
{
    VSOutput output;

    output.Position = mul(camera.Projection, mul(camera.Transform, float4(input.Position, 0.0, 1.0)));
    output.TexCoord = input.TexCoord;
    output.Tint = input.Tint;

    return output;
}

[shader("pixel")]
float4 PSMain(const in VSOutput input)
{
    return sprite.Sample(input.TexCoord) * input.Tint;
}
