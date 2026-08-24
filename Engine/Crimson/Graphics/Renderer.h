#pragma once

#include <memory>

#include "Private/RenderContext.h"

namespace cge
{
    /**
     * Crimson's renderer, responsible for drawing to the window.
     */
    class Renderer final
    {
        std::unique_ptr<Private::RenderContext> _context;

    public:
        /**
         * Create a renderer for the given window.
         * @param window The SDL3 window to associate the renderer with.
         */
        explicit Renderer(SDL_Window* window);

        /**
         * Process and render all geometry to the window.
         */
        void Render();
    };
}
