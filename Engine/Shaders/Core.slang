#pragma once

#define VertexBlock layout (set = 1) ParameterBlock
#define PixelBlock layout (set = 3) ParameterBlock

#define VertexSampler2D layout (set = 0) Sampler2D
#define PixelSampler2D layout (set = 2) Sampler2D

struct Camera
{
    float4x4 Projection;
    float4x4 View;
    float3 Position;
    float _padding1;
}
