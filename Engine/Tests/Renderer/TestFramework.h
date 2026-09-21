#pragma once

#include <SDL3/SDL.h>
#include <Core/Logger.h>
#include <Graphics/Renderer.h>

#include <string>
#include <utility>

using namespace cge;

class TestBase
{
    std::string _name;

protected:
    SDL_Window* Window{};
    Renderer* Renderer{};

public:
    explicit TestBase(std::string name) : _name(std::move(name)) {}
    virtual ~TestBase()
    {
        delete Renderer;
        if (Window)
            SDL_DestroyWindow(Window);

        SDL_Quit();
    }

    virtual void Init() {}
    virtual void Loop(f32 dt) {}

    void Run()
    {
        if (!SDL_Init(SDL_INIT_VIDEO))
            CGE_FATAL("Failed to initialize SDL: {}", SDL_GetError());

        Window = SDL_CreateWindow(_name.c_str(), 1280, 720, SDL_WINDOW_RESIZABLE);
        if (!Window)
            CGE_FATAL("Failed to create window: {}", SDL_GetError());

        Renderer = new cge::Renderer(Window);
        Init();

        bool alive = true;
        while (alive)
        {
            SDL_Event event;
            while (SDL_PollEvent(&event))
            {
                switch (event.type)
                {
                    case SDL_EVENT_QUIT:
                        alive = false;
                        break;
                    case SDL_EVENT_WINDOW_RESIZED:
                        Renderer->Resize({ static_cast<u32>(event.window.data1), static_cast<u32>(event.window.data2) });
                        break;
                }
            }

            Renderer->NewFrame();
            // todo actual delta time
            Loop(1.0f / 60.0f);
            Renderer->Render();
        }
    }
};
