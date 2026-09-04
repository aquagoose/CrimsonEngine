#pragma once

#include "Core/Logger.h"
#include "Math/Size.h"

#include <SDL3/SDL.h>

#define CGE_SDL_CHECK(value, operation) if (!value) CGE_FATAL("SDL operation \"{}\" failed: {}", operation, SDL_GetError());

namespace cge::Private::SDLUtils
{
    inline u32 CalculateMipLevels(const Sizeu& size)
    {
        return static_cast<u32>(floor(log2(std::max(size.Width, size.Height))) + 1);
    }
}