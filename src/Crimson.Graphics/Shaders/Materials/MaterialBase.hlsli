struct Vertex
{
    float3 Position: TEXCOORD0;
    float2 TexCoord: TEXCOORD1;
    float3 Normal:   TEXCOORD2;
    float4 Color:    TEXCOORD3;
};

struct Camera
{
    float4x4 Projection;
    float4x4 View;
    float4 Position;
};

cbuffer SceneData : register(b0)
{
    Camera gCamera;
}