#pragma once

#include "Crimson/Core/Logger.h"

#include <SDL3/SDL.h>

#define SDL_CHECK(result, operation) if (!result) CGE_FATAL("SDL operation \"{}\" failed: {}", operation, SDL_GetError());