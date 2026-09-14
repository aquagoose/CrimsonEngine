#pragma once

#include "Private/RenderContext.h"

namespace cge
{
    class Texture final
    {
        friend class Renderer;

        Private::RenderContext& _context;
        Sizeu _size;
        bool _generateMips;

        SDL_GPUTexture* TextureHandle;

        Texture(Private::RenderContext& context, SDL_GPUTexture* texture, Sizeu size, bool generateMips);

    public:
        ~Texture();

        [[nodiscard]] Sizeu Size() const;
    };
}
