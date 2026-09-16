#pragma once

#include "Graphics/Private/RenderContext.h"
#include "Graphics/Common.h"
#include "Graphics/Color.h"
#include "Graphics/Texture.h"
#include "Math/Vec2.h"

#include <vector>

namespace cge::Private
{
    class TextureBatcher final
    {
    public:
        struct Draw
        {
            cge::Texture& Texture;
            Vec2f TopLeft;
            Vec2f TopRight;
            Vec2f BottomLeft;
            Vec2f BottomRight;
            Color Tint;
        };

    private:
        using Index = u32;
        struct Vertex
        {
            Vec2f Position;
            Vec2f TexCoord;
            Color Tint;
        };

        struct Batch
        {
            cge::Texture& Texture;
            u32 DrawOffset;
            u32 NumDraws;
        };

        // the initial maximum number of draws the batch supports, before expansion.
        static constexpr u32 InitialMaxDraws = 4096;

        static constexpr u32 NumVertices = 4; // the number of vertices per sprite
        static constexpr u32 NumIndices = 6; // the number of indices per sprite

        RenderContext& _context;

        SDL_GPUBuffer* _vertexBuffer;
        SDL_GPUBuffer* _indexBuffer;
        u32 _maxDraws; // the maximum draws the batcher currently supports

        SDL_GPUGraphicsPipeline* _pipeline;
        SDL_GPUSampler* _sampler; // todo sampler per texture

        std::vector<Draw> _draws;
        std::vector<Batch> _batches;

    public:
        TextureBatcher(RenderContext& context, SDL_GPUTextureFormat outFormat);
        ~TextureBatcher();

        void Clear();
        void AddToBatch(const Draw& draw);
        bool Render(SDL_GPUCommandBuffer* cb, SDL_GPUTexture* texture, bool clear, const Camera& camera);
    };
}
