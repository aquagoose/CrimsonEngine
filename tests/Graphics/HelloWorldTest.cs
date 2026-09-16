#!/usr/bin/env dotnet
#:project ../../src/Crimson.Graphics/Crimson.Graphics.csproj

using Crimson.Graphics;
using piko.SDL3;

if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
{
    Console.WriteLine($"Failed to initialize SDL: {SDL.GetError()}");
    return;
}

SDL.Window window = SDL.CreateWindow("Hello World Test", 1280, 720, SDL.WindowFlags.Resizable);
if (window.IsNull)
{
    Console.WriteLine($"Failed to create window: {SDL.GetError()}");
    return;
}

Renderer.Init(window);

bool alive = true;
while (alive)
{
    while (SDL.PollEvent(out SDL.Event sdlEvent))
    {
        switch ((SDL.EventType) sdlEvent.Type)
        {
            case SDL.EventType.Quit:
                alive = false;
                break;
        }
    }
}

Renderer.Free();
SDL.DestroyWindow(window);
SDL.Quit();