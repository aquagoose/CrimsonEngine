#include <Core/Logger.h>
#include <Graphics/RendererContext.h>
#include <SDL3/SDL.h>

int main(int argc, char* argv[])
{
    if (!SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS))
        CGE_FATAL("Failed to initialize SDL: {}", SDL_GetError());

    SDL_Window* window = SDL_CreateWindow("Create Context Test", 1280, 720, SDL_WINDOW_VULKAN);
    if (!window)
        CGE_FATAL("Failed to create window: {}", SDL_GetError());

    auto context = new cge::RendererContext(window);

    delete context;
    SDL_DestroyWindow(window);
    SDL_Quit();
    return 0;
}
