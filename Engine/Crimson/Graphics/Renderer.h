#pragma once

#include "Private/RenderContext.h"
#include "Texture.h"
#include "Bitmap.h"
#include "Math/Size.h"

#include <memory>

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
        ~Renderer();

        std::unique_ptr<Texture> CreateTexture(void* data, const Sizeu& size, PixelFormat format, bool generateMips = true) const;
        std::unique_ptr<Texture> CreateTexture(const Bitmap& bitmap, bool generateMips = true);
        std::unique_ptr<Texture> CreateTexture(const std::string& path, bool generateMips = true);

        /**
         * Process and render all geometry to the window.
         */
        void Render();
    };
}
