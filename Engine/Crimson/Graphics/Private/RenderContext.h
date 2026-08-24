#pragma once

#include <SDL3/SDL.h>

namespace cge::Private
{
    /**
     * The core renderer context, containing the core objects for the renderer.
     */
    class RenderContext final
    {
    public:
        SDL_Window* Window;
        SDL_GPUDevice* Device;

        explicit RenderContext(SDL_Window* window);
        ~RenderContext();
    };
}
