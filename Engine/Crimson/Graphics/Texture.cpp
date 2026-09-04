#include "Texture.h"

namespace cge
{
    Texture::Texture(Private::RenderContext& context, SDL_GPUTexture* texture, bool generateMips)
        : _context(context), _generateMips(generateMips), TextureHandle(texture) {}

    Texture::~Texture()
    {
        SDL_ReleaseGPUTexture(_context.Device, TextureHandle);
    }
}
