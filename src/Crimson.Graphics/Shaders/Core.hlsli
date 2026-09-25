#pragma once

#define __UNIFORM(type, name, binding, set) cbuffer type##Buffer : register(b##binding, space##set) { type name; }
#define __TEXTURE(type, name, binding, set) Texture##type name : register(t##binding, space##set);\
    SamplerState name##Sampler : register(s##binding, space##set);

#define VertexUniform(type, name, binding) __UNIFORM(type, name, binding, 1)
#define PixelUniform(type, name, binding) __UNIFORM(type, name, binding, 3)

#define VertexSampler2D(name, binding) __TEXTURE(2D, name, binding, 0)
#define PixelSampler2D(name, binding) __TEXTURE(2D, name, binding, 2)

#define SampleTexture(texture, texCoord) texture.Sample(texture##Sampler, texCoord)