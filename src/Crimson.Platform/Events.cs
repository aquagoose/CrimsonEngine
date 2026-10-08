using System.Numerics;
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

    /// <summary>
    /// Invoked when the mouse is moved.
    /// </summary>
    public static event OnMouseMove MouseMove;

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

                case SDL.EventType.MouseMotion:
                {
                    Vector2 position = new Vector2(e.Motion.X, e.Motion.Y);
                    Vector2 delta = new Vector2(e.Motion.Xrel, e.Motion.Yrel);
                    MouseMove(position, delta);
                    break;
                }
            }
        }
    }

    private static void Reset()
    {
        // todo should these be in Window?
        WindowClosed = delegate { };
        Resized = delegate { };

        KeyDown = delegate { };
        KeyUp = delegate { };

        MouseMove = delegate { };
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

    /// <summary>
    /// Delegate used for mouse move events.
    /// </summary>
    /// <param name="position">The absolute position of the mouse on-screen, in pixel coordinates.</param>
    /// <param name="delta">The change in the mouse position since the last frame, in pixel coordinates.</param>
    public delegate void OnMouseMove(Vector2 position, Vector2 delta);
}