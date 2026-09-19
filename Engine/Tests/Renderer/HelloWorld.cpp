#include <Core/Logger.h>
#include <Graphics/Renderer.h>

using namespace cge;

int main(int argc, char* argv[])
{
    if (!SDL_Init(SDL_INIT_VIDEO))
        CGE_FATAL("Failed to initialize SDL: {}", SDL_GetError());

    SDL_Window* window = SDL_CreateWindow("Hello World Test", 1280, 720, SDL_WINDOW_RESIZABLE | SDL_WINDOW_HIGH_PIXEL_DENSITY);
    if (!window)
        CGE_FATAL("Failed to create window: {}", SDL_GetError());

    auto renderer = new Renderer(window);
    float value = 0;
    auto texture = renderer->CreateTexture("Content/DEBUG.png");
    auto texture2 = renderer->CreateTexture("Content/Bagel.png");

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
                case SDL_EVENT_WINDOW_RESIZED:
                    renderer->Resize({ static_cast<u32>(event.window.data1), static_cast<u32>(event.window.data2) });
                    break;
            }
        }

        value += 1.0f / 60.0f;
        if (value >= 2 * M_PI)
            value -= 2 * M_PI;

        renderer->NewFrame();

        for (int i = 0; i < 10; i++)
        {
            float v = std::sin(value + i) * 100;
            renderer->DrawImage(*texture, Vec2f(v, static_cast<float>(i * 50)));
        }

        for (int i = 0; i < 10; i++)
        {
            float v = std::cos(value + i) * 100;
            renderer->DrawImage(*texture2, Vec2f(600 - v, i * 50), Color::Aquamarine());
        }

        renderer->Render();
    }

    texture2.reset();
    texture.reset();
    delete renderer;
    SDL_DestroyWindow(window);
    SDL_Quit();

    return 0;
}
