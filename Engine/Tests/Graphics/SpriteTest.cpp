#include <Core/Logger.h>
#include <Graphics/Renderer.h>
#include <SDL3/SDL.h>

using namespace cge;

int main(int argc, char* argv[])
{
    if (!SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS))
        CGE_FATAL("Failed to initialize SDL: {}", SDL_GetError());

    SDL_Window* window = SDL_CreateWindow("Sprite Test", 1280, 720, SDL_WINDOW_VULKAN);
    if (!window)
        CGE_FATAL("Failed to create window: {}", SDL_GetError());

    Renderer* renderer = new Renderer(window);

    bool running = true;
    while (running)
    {
        SDL_Event event;
        while (SDL_PollEvent(&event))
        {
            switch (event.type)
            {
                case SDL_EVENT_QUIT:
                    running = false;
                    break;
            }
        }

        renderer->Render();
    }

    delete renderer;
    SDL_DestroyWindow(window);
    SDL_Quit();

    return 0;
}
