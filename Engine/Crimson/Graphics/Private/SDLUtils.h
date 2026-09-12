#pragma once

#include "Core/Logger.h"
#include "Math/Size.h"

#include <SDL3/SDL.h>

#define CGE_SDL_CHECK(value, operation) if (!value) CGE_FATAL("SDL operation \"{}\" failed: {}", operation, SDL_GetError());

namespace cge::Private::SDLUtils
{
    constexpr SDL_GPUColorTargetBlendState PremultipliedBlend = {
        .src_color_blendfactor = SDL_GPU_BLENDFACTOR_SRC_ALPHA,
        .dst_color_blendfactor = SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
        .color_blend_op = SDL_GPU_BLENDOP_ADD,
        .src_alpha_blendfactor = SDL_GPU_BLENDFACTOR_SRC_ALPHA,
        .dst_alpha_blendfactor = SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
        .alpha_blend_op = SDL_GPU_BLENDOP_ADD,
        .enable_blend = true,
        .enable_color_write_mask = false,
    };

    inline u32 CalculateMipLevels(const Sizeu& size)
    {
        return static_cast<u32>(floor(log2(std::max(size.Width, size.Height))) + 1);
    }
}