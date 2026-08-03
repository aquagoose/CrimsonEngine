struct VSInput
{
    float2 Position: TEXCOORD0;
    float2 TexCoord: TEXCOORD1;
    float4 Tint:     TEXCOORD2;
};

struct VSOutput
{
    float4 Position: SV_Position;
    float2 TexCoord: TEXCOORD0;
    float4 Tint:     COLOR0;
};

struct TransformMatrices
{
    float4x4 Projection;
    float4x4 Transform;
};

ParameterBlock<TransformMatrices> Camera;
ParameterBlock<Sampler2D> Sprite;

VSOutput VSMain(const in VSInput input)
{
    VSOutput output;

    output.Position = mul(Camera.Projection, mul(Camera.Transform, float4(input.Position, 0.0, 1.0)));
    output.TexCoord = input.TexCoord;
    output.Tint = input.Tint;

    return output;
}

float4 PSMain(const in VSOutput input): SV_Target0
{
    return Sprite.Sample(input.TexCoord);
}
