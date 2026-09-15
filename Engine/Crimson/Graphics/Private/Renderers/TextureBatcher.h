#pragma once

#include "../RenderContext.h"
#include "../../Texture.h"
#include "Math/Vec2.h"
#include "Math/Vec4.h"

#include <vector>

namespace cge::Private
{
    class TextureBatcher final
    {
        friend class cge::Texture;

    public:
        struct Draw
        {
            cge::Texture& Texture;
            Vec2f TopLeft;
            Vec2f TopRight;
            Vec2f BottomLeft;
            Vec2f BottomRight;
            Vec4f Tint; // todo replace with color
        };

    private:
        using Index = u32;
        struct Vertex
        {
            Vec2f Position;
            Vec2f TexCoord;
            Vec4f Tint; // todo replace with Color
        };

        struct Batch
        {
            cge::Texture& Texture;
            u32 DrawOffset;
            u32 NumDraws;
        };

        // the initial maximum number of sprites per batch, before the batch is expanded.
        static constexpr u32 InitialBatchSize = 4096;

        static constexpr u32 NumVertices = 4; // the number of vertices per sprite
        static constexpr u32 NumIndices = 6; // the number of indices per sprite

        RenderContext& _context;

        SDL_GPUBuffer* _vertexBuffer;
        SDL_GPUBuffer* _indexBuffer;
        u32 _batchSize;

        SDL_GPUGraphicsPipeline* _pipeline;
        SDL_GPUSampler* _sampler; // todo sampler per texture

        std::vector<Draw> _draws;
        std::vector<Batch> _batches;

    public:
        TextureBatcher(RenderContext& context, SDL_GPUTextureFormat outFormat);
        ~TextureBatcher();

        void Clear();
        void AddToBatch(const Draw& draw);
        bool Render(SDL_GPUCommandBuffer* cb, SDL_GPUTexture* texture, bool clear);
    };
}
