#pragma once

#include "RendererContext.h"

#include <memory>

namespace cge
{
    /**
     * Crimson's renderer, responsible for drawing to the screen.
     */
    class Renderer final
    {
        std::unique_ptr<RendererContext> _context;

    public:
        Renderer(const Renderer&) = delete;
        Renderer& operator =(const Renderer&) = delete;

        explicit Renderer(SDL_Window* window);

        void Render();
    };
}
