#include "Texture.h"

namespace cge
{
    Texture::Texture(Private::RenderContext& context, SDL_GPUTexture* texture)
        : _context(context), TextureHandle(texture) {}

    Texture::~Texture()
    {
        SDL_ReleaseGPUTexture(_context.Device, TextureHandle);
    }
}
