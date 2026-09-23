#!/usr/bin/env dotnet
#:project ../../src/Crimson.Graphics/Crimson.Graphics.csproj

using System.Numerics;
using Crimson.Graphics;
using Crimson.Math;
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

Texture texture1 = new Texture("Content/DEBUG.png");
Texture texture2 = new Texture("Content/bagel.png");
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
            case SDL.EventType.WindowResized:
                Size<uint> newSize = new Size<uint>((uint) sdlEvent.Window.Data1, (uint) sdlEvent.Window.Data2);
                Renderer.Resize(newSize);
                break;
        }
    }

    value += 0.03f;
    if (value >= float.Pi * 2)
        value -= float.Pi * 2;

    Renderer.NewFrame();
    
    for (int i = 0; i < 10; i++)
        Renderer.DrawImage(texture1, new Vector2(i * 30 + float.Sin(value + i) * 100, i * 50));

    for (int i = 0; i < 10; i++)
    {
        Renderer.DrawImage(texture2,
            new Vector2((Renderer.Size.Width - texture2.Size.Width) - i * 30 + float.Cos(value + i) * 100, i * 50),
            Color.Aqua);
    }

    Renderer.Render();
}

texture2.Dispose();
texture1.Dispose();
Renderer.Free();
SDL.DestroyWindow(window);
SDL.Quit();