#include "Renderer.h"

namespace cge
{
    Renderer::Renderer(SDL_Window* window)
    {
        _context = std::make_unique<Private::RenderContext>(window);
    }
}
