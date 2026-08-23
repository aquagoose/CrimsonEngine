#pragma once

#include <Core/Logger.h>
#include <SDL3/SDL.h>

#define CGE_SDL_CHECK(value, operation) if (!value) CGE_FATAL("SDL operation \"{}\" failed: {}", operation, SDL_GetError());

namespace cge::Private::SDLUtils
{

}