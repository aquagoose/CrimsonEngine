#pragma once

#include "Private/RenderContext.h"

#include <SDL3/SDL.h>

#include <memory>

namespace cge
{
    class Renderer final
    {
        std::unique_ptr<pvt::RenderContext> _context;

    public:
        explicit Renderer(SDL_Window* window);
    };
}