#include "Crimson/Graphics/Renderer.h"

#include "../../include/Crimson/Graphics/Private/RenderContext.h"

namespace cge
{
    Renderer::Renderer(SDL_Window* window)
    {
        _context = std::make_unique<pvt::RenderContext>(window);
    }
}
