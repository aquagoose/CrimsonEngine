#!/usr/bin/env dotnet
#:project ../../src/Crimson.Graphics/Crimson.Graphics.csproj

using System.Numerics;
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
Console.WriteLine(Renderer.BackendName);

Texture texture = new Texture("Content/DEBUG.png");
float value = 0;

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

    value += 0.03f;
    if (value >= float.Pi * 2)
        value -= float.Pi * 2;

    Renderer.NewFrame();
    
    for (int i = 0; i < 10; i++)
        Renderer.DrawImage(texture, new Vector2(i * 50 + float.Sin(value + i) * 100, i * 50));
    
    Renderer.Render();
}

texture.Dispose();
Renderer.Free();
SDL.DestroyWindow(window);
SDL.Quit();