#include "Texture.h"

namespace cge
{
    Texture::Texture(Private::RenderContext& context, SDL_GPUTexture* texture, Sizeu size, bool generateMips)
        : _context(context), _size(size), _generateMips(generateMips), TextureHandle(texture) {}

    Texture::~Texture()
    {
        SDL_ReleaseGPUTexture(_context.Device, TextureHandle);
    }

    Sizeu Texture::Size() const
    {
        return _size;
    }
}
