#!/usr/bin/env dotnet
#:project ../../src/Crimson.Platform/Crimson.Platform.csproj

using Crimson.Math;
using Crimson.Platform;
using piko.SDL3;

// force window to use X11 on linux, as wayland won't display a window if nothing has been presented to it
if (OperatingSystem.IsLinux())
    SDL.SetHint(SDL.Hint.VideoDriver, "x11");

WindowInfo windowInfo = new WindowInfo("Window Test", new Size<uint>(1280, 720), false);
Window.Init(in windowInfo);
Thread.Sleep(5000);
Window.Free();