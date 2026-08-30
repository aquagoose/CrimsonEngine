using piko.SDL3;

namespace Crimson.Graphics.Tests.Common;

public abstract class TestBase(string name) : IDisposable
{
    private SDL.Window _window;

    public Renderer Renderer;

    protected virtual void Init() { }
    protected virtual void Loop(float dt) { }

    public void Run()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        _window = SDL.CreateWindow(name, 1280, 720, SDL.WindowFlags.Resizable);
        if (_window.IsNull)
            throw new Exception($"Failed to create window: {SDL.GetError()}");

        Renderer = new Renderer(_window);

        Init();

        bool alive = true;
        while (alive)
        {
            while (SDL.PollEvent(out SDL.Event sdlEvent))
            {
                switch ((SDL.EventType) sdlEvent.Type)
                {
                    case SDL.EventType.Quit:
                    case SDL.EventType.WindowCloseRequested:
                        alive = false;
                        break;
                }
            }

            Loop(1.0f / 60.0f);
            Renderer.Render();
        }
    }

    public virtual void Dispose()
    {
        Renderer.Dispose();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}