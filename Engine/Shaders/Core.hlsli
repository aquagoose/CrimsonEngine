#pragma once

#define VertexUniform(type, name, index) cbuffer type##_Buffer : register(b##index, space1) { type name; }
#define PixelUniform(type, name, index) cbuffer type##_Buffer : register(b##index, space3) { type name; }

#define VertexSampler2D(name, index) Texture2D name : register(t##index, space0);\
    SamplerState name##Sampler : register(s##index, space0);
#define PixelSampler2D(name, index) Texture2D name : register(t##index, space2);\
    SamplerState name##Sampler : register(s##index, space2);

struct Camera
{
    float4x4 Projection;
    float4x4 View;
    float3 Position;
    float _padding1;
};
