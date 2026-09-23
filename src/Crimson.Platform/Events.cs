using piko.SDL3;

namespace Crimson.Platform;

public static class Events
{
    public static event OnQuit Quit;

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
            }
        }
    }

    private static void Reset()
    {
        Quit = delegate { };
    }

    public delegate void OnQuit();
}