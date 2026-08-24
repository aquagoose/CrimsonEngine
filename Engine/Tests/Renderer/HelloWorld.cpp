#include <Core/Logger.h>
#include <Graphics/Renderer.h>

int main(int argc, char* argv[])
{
    if (!SDL_Init(SDL_INIT_VIDEO))
        CGE_FATAL("Failed to initialize SDL: {}", SDL_GetError());

    SDL_Window* window = SDL_CreateWindow("Hello World Test", 1280, 720, SDL_WINDOW_RESIZABLE);
    if (!window)
        CGE_FATAL("Failed to create window: {}", SDL_GetError());

    auto renderer = new cge::Renderer(window);

    bool alive = true;
    while (alive)
    {
        SDL_Event event;
        while (SDL_PollEvent(&event))
        {
            switch (event.type)
            {
                case SDL_EVENT_QUIT:
                case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                    alive = false;
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
