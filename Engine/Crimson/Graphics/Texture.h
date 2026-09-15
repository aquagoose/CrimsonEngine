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

        SDL_GPUTexture* _handle;

        Texture(Private::RenderContext& context, SDL_GPUTexture* texture, Sizeu size, bool generateMips);

    public:
        ~Texture();

        [[nodiscard]] void* Handle() const { return _handle; }
        [[nodiscard]] Sizeu Size() const { return _size; }
    };
}
