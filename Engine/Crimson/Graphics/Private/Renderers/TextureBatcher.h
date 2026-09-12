#pragma once

#include "../RenderContext.h"
#include "Math/Vec2.h"
#include "Math/Vec4.h"

#include <vector>

namespace cge::Private
{
    class TextureBatcher final
    {
        struct Vertex;
        using Index = u32;

        static constexpr u32 InitialMaxSprites = 4096;

        static constexpr u32 NumVertices = 4;
        static constexpr u32 NumIndices = 6;

        RenderContext& _context;

        std::vector<Vertex> _vertices;
        std::vector<Index> _indices;

        SDL_GPUBuffer* _vertexBuffer;
        SDL_GPUBuffer* _indexBuffer;

        SDL_GPUGraphicsPipeline* _pipeline;

    public:
        TextureBatcher(RenderContext& context, SDL_GPUTextureFormat outFormat);
        ~TextureBatcher();

    private:
        struct Vertex
        {
            Vec2f Position;
            Vec2f TexCoord;
            Vec4f Tint; // todo replace with Color
        };
    };
}
