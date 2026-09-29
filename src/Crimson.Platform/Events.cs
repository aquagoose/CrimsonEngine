using Crimson.Math;
using piko.SDL3;

namespace Crimson.Platform;

public static class Events
{
    public static event OnQuit Quit;

    public static event OnResized Resized;

    public static void Init()
    {
        if (!SDL.Init(SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        Reset();
    }

    public static void Free()
    {
        Reset();
        SDL.QuitSubSystem(SDL.InitFlags.Events);
    }

    public static void Poll()
    {
        while (SDL.PollEvent(out SDL.Event e))
        {
            switch ((SDL.EventType) e.Type)
            {
                case SDL.EventType.Quit:
                    Quit();
                    break;
                case SDL.EventType.WindowResized:
                {
                    Size<uint> newSize = new Size<uint>((uint) e.Window.Data1, (uint) e.Window.Data2);
                    Resized(newSize);
                    break;
                }
            }
        }
    }

    private static void Reset()
    {
        Quit = delegate { };
        Resized = delegate { };
    }

    public delegate void OnQuit();

    public delegate void OnResized(Size<uint> newSize);
}