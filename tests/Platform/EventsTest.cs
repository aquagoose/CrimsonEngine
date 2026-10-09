#!/usr/bin/env dotnet
#:project ../../src/Crimson.Platform/Crimson.Platform.csproj

using Crimson.Math;
using Crimson.Platform;
using piko.SDL3;

// force window to use X11 on linux, as wayland won't display a window if nothing has been presented to it
if (OperatingSystem.IsLinux())
    SDL.SetHint(SDL.Hint.VideoDriver, "x11");

WindowInfo windowInfo = new WindowInfo("Events Test", new Size<uint>(1280, 720), true, false);
Window.Init(in windowInfo);
Events.Init();

bool running = true;
Events.WindowClosed += () => running = false;
Events.Resized += size => Console.WriteLine($"Resized: {size}");
Events.KeyDown += (key, repeat) => Console.WriteLine($"Key down: {key}, Repeat: {repeat}");
Events.KeyUp += key => Console.WriteLine($"Key up: {key}");
Events.MouseMove += (position, delta) => Console.WriteLine($"Mouse pos: {position}, delta: {delta}");
    
while (running)
{
    Events.Poll();
    Thread.Sleep(100);
}

Events.Free();
Window.Free();