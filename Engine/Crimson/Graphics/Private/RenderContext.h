#pragma once

#include <SDL3/SDL.h>

namespace cge::Private
{
    class RenderContext final
    {
    public:
        SDL_Window* Window;
        SDL_GPUDevice* Device;

        explicit RenderContext(SDL_Window* window);
        ~RenderContext();
    };
}
