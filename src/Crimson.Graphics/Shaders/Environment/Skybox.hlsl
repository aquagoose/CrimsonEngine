#include "../Core3D.hlsli"

struct VSOutput
{
    float4 Position: SV_Position;
    float3 TexCoord: TEXCOORD0;
};

PixelSamplerCube(Texture, 0)

VSOutput VSMain(const in Vertex input)
{
    VSOutput output;
    
    // remove translation component so skybox always surrounds camera
    float3x3 view = (float3x3) Scene.Camera.View;
    output.Position = mul((float4x3) Scene.Camera.Projection, mul(view, input.Position)).xyww;
    output.TexCoord = input.Position;
    
    return output;
}

float4 PSMain(const in VSOutput input): SV_Target0
{
    return SampleTexture(Texture, input.TexCoord);
}