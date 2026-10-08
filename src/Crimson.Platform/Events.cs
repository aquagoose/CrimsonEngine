using Crimson.Math;
using piko.SDL3;

namespace Crimson.Platform;

/// <summary>
/// Manages core engine events.
/// </summary>
public static class Events
{
    /// <summary>
    /// Invoked when the <see cref="Window"/> is closed.
    /// </summary>
    public static event OnWindowClosed WindowClosed;

    /// <summary>
    /// Invoked when the <see cref="Window"/> is resized.
    /// </summary>
    public static event OnResized Resized;

    /// <summary>
    /// Invoked when a key is pressed or a currently pressed key repeats.
    /// </summary>
    public static event OnKeyDown KeyDown;

    /// <summary>
    /// Invoked when a key is released.
    /// </summary>
    public static event OnKeyUp KeyUp;

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
                    WindowClosed();
                    break;
                case SDL.EventType.WindowResized:
                {
                    SDL.Window window = SDL.GetWindowFromID(e.Window.WindowID);
                    SDL.GetWindowSizeInPixels(window, out int w, out int h);
                    Size<uint> newSize = new Size<uint>((uint) w, (uint) h);
                    Resized(newSize);
                    break;
                }

                case SDL.EventType.KeyDown:
                {
                    Key key = SDLUtils.KeycodeToKey(e.Key.Key);
                    KeyDown(key, e.Key.Repeat);
                    break;
                }
                case SDL.EventType.KeyUp:
                {
                    Key key = SDLUtils.KeycodeToKey(e.Key.Key);
                    KeyUp(key);
                    break;
                }
            }
        }
    }

    private static void Reset()
    {
        WindowClosed = delegate { };
        Resized = delegate { };

        KeyDown = delegate { };
        KeyUp = delegate { };
    }

    public delegate void OnWindowClosed();

    public delegate void OnResized(Size<uint> newSize);

    /// <summary>
    /// Delegate used for key down events.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <param name="repeat">If <see langword="true"/> the key is already pressed, and is repeating. Generally this can be ignored.</param>
    public delegate void OnKeyDown(Key key, bool repeat);

    /// <summary>
    /// Delegate used for key up events.
    /// </summary>
    /// <param name="key">The key that was released.</param>
    public delegate void OnKeyUp(Key key);
}