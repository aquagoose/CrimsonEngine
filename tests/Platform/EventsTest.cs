#!/usr/bin/env dotnet
#:project ../../src/Crimson.Platform/Crimson.Platform.csproj

using Crimson.Math;
using Crimson.Platform;
using piko.SDL3;

// force window to use X11 on linux, as wayland won't display a window if nothing has been presented to it
if (OperatingSystem.IsLinux())
    SDL.SetHint(SDL.Hint.VideoDriver, "x11");

WindowInfo windowInfo = new WindowInfo("Events Test", new Size<uint>(1280, 720), true);
Window.Init(in windowInfo);
Events.Init();

bool running = true;
Events.Quit += () => running = false;
    
while (running)
{
    Events.Poll();
    Thread.Sleep(100);
}

Events.Free();
Window.Free();