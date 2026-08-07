using Crimson.Core;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.Tests.Common;

public abstract class TestBase(string name) : IDisposable
{
    private SDL.Window _window;

    protected virtual void Init() { }
    protected virtual void Loop(float dt) { }

    public unsafe void Run()
    {
        Logger.Trace("Initializing SDL.");
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        Logger.Trace("Creating window.");
        _window = SDL.CreateWindow(name, 1280, 720, SDL.WindowFlags.Resizable);
        if (_window.IsNull)
            throw new Exception($"Failed to create window: {SDL.GetError()}");

        Logger.Trace("Initializing renderer.");
        Renderer.Init(_window);

        Init();

        bool alive = true;
        while (alive)
        {
            SDL.Event sdlEvent;
            while (SDL.PollEvent(&sdlEvent))
            {
                switch ((SDL.EventType) sdlEvent.Type)
                {
                    case SDL.EventType.Quit:
                    case SDL.EventType.WindowCloseRequested:
                        alive = false;
                        break;
                    case SDL.EventType.WindowResized:
                        int w, h;
                        SDL.GetWindowSizeInPixels(_window, &w, &h);
                        Renderer.Resize(new Size<uint>((uint) w, (uint) h));
                        break;
                }
            }

            Loop(1.0f / 60.0f);
            Renderer.Render();
        }
    }

    public virtual void Dispose()
    {
        Renderer.Free();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}