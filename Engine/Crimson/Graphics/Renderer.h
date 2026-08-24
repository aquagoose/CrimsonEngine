#pragma once

#include <memory>

#include "Private/RenderContext.h"

namespace cge
{
    class Renderer final
    {
        std::unique_ptr<Private::RenderContext> _context;

    public:
        explicit Renderer(SDL_Window* window);

        void Render();
    };
}
