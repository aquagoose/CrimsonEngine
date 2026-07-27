using System.Numerics;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Platform;

/// <summary>
/// Contains a series of events that may be called in an application.
/// </summary>
public static class Events
{
    /// <summary>
    /// Invoked when the window is requesting to close.
    /// </summary>
    public static event OnWindowClose WindowClose = delegate { };

    public static event OnSurfaceSizeChanged SurfaceSizeChanged = delegate { };

    public static event OnKeyDown KeyDown = delegate { };

    public static event OnKeyRepeat KeyRepeat = delegate { };

    public static event OnKeyUp KeyUp = delegate { };

    public static event OnMouseButtonDown MouseButtonDown = delegate { };

    public static event OnMouseButtonUp MouseButtonUp = delegate { };

    public static event OnMouseMove MouseMove = delegate { };

    public static event OnMouseScroll MouseScroll = delegate { };

    public static event OnTextInput TextInput = delegate { };

    /// <summary>
    /// Create a new <see cref="Events"/>.
    /// </summary>
    public static void Create()
    {
        if (!SDL.Init(SDL.InitFlags.Events))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");
    }
    
    /// <summary>
    /// Dispose of this <see cref="Events"/>.
    /// </summary>
    public static void Destroy()
    {
        SDL.Quit();
    }

    /// <summary>
    /// Poll and process events.
    /// </summary>
    public static unsafe void ProcessEvents()
    {
        SDL.Event sdlEvent;
        while (SDL.PollEvent(&sdlEvent))
        {
            switch ((SDL.EventType) sdlEvent.Type)
            {
                case SDL.EventType.WindowCloseRequested:
                {
                    WindowClose();
                    break;
                }
                case SDL.EventType.WindowResized:
                {
                    SurfaceSizeChanged(Surface.Size);
                    break;
                }
                
                case SDL.EventType.KeyDown:
                {
                    if (sdlEvent.Key.Repeat)
                        break;
                    KeyDown(SdlUtils.KeycodeToKey(sdlEvent.Key.Key));
                    break;
                }
                case SDL.EventType.KeyUp:
                {
                    KeyUp(SdlUtils.KeycodeToKey(sdlEvent.Key.Key));
                    break;
                }

                case SDL.EventType.MouseButtonDown:
                {
                    MouseButtonDown(SdlUtils.ButtonIndexToButton(sdlEvent.Button.Button));
                    break;
                }
                case SDL.EventType.MouseButtonUp:
                {
                    MouseButtonUp(SdlUtils.ButtonIndexToButton(sdlEvent.Button.Button));
                    break;
                }
                case SDL.EventType.MouseMotion:
                {
                    float scale = SDL.GetWindowPixelDensity(SDL.GetWindowFromID(sdlEvent.Window.WindowID));
                    MouseMove(new Vector2T<float>(sdlEvent.Motion.X, sdlEvent.Motion.Y) * scale,
                        new Vector2T<float>(sdlEvent.Motion.Xrel, sdlEvent.Motion.Yrel));
                    break;
                }
                case SDL.EventType.MouseWheel:
                {
                    MouseScroll(new Vector2T<float>(sdlEvent.Wheel.X, sdlEvent.Wheel.Y));
                    break;
                }

                case SDL.EventType.TextInput:
                {
                    string text = new string((sbyte*) sdlEvent.Text.Text);
                    foreach (char c in text)
                        TextInput(c);
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// A delegate that is used in the <see cref="Events.WindowClose"/> event.
    /// </summary>
    public delegate void OnWindowClose();

    public delegate void OnSurfaceSizeChanged(Size<int> newSize);
    
    public delegate void OnKeyDown(Key key);

    public delegate void OnKeyRepeat(Key key);

    public delegate void OnKeyUp(Key key);

    public delegate void OnMouseButtonDown(MouseButton button);

    public delegate void OnMouseButtonUp(MouseButton button);
    
    public delegate void OnMouseMove(Vector2T<float> position, Vector2T<float> delta);

    public delegate void OnMouseScroll(Vector2T<float> scroll);

    public delegate void OnTextInput(char c);
}