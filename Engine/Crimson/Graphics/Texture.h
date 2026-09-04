#pragma once

#include "Private/RenderContext.h"

namespace cge
{
    class Texture final
    {
        friend class Renderer;

        Private::RenderContext& _context;
        bool _generateMips;

        SDL_GPUTexture* TextureHandle;

        Texture(Private::RenderContext& context, SDL_GPUTexture* texture, bool generateMips);

    public:
        ~Texture();
    };
}
