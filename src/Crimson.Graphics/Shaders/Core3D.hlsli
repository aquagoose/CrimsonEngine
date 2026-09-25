#include "Core.hlsli"

struct Vertex
{
    float3 Position: POSITION0;
    float2 TexCoord: TEXCOORD0;
    float3 Normal:   NORMAL0;
    float4 Color:    COLOR0;
};

struct Camera
{
    float4x4 Projection;
    float4x4 View;
    float3 Position;
    float __padding;
};

struct SceneInfo
{
    Camera Camera;
};

VertexUniform(SceneInfo, Scene, 0)