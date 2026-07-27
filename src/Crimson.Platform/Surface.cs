using System.Diagnostics;
using Crimson.Core;
using Crimson.Graphics;
using Crimson.Math;
using piko.SDL3;
using StbImageSharp;

namespace Crimson.Platform;

/// <summary>
/// The primary application surface that is being rendered to.
/// </summary>
public static class Surface
{
    private static string _title;
    private static string _details;
    
    internal static SDL.Window Window;

    /// <summary>
    /// The surface's size, in pixels.
    /// </summary>
    /// <remarks><see cref="set_Size"/> may not be available on all platforms.</remarks>
    public static unsafe Size<int> Size
    {
        get
        {
            int w, h;
            SDL.GetWindowSizeInPixels(Window, &w, &h);
            return new Size<int>(w, h);
        }
        set
        {
            SDL.SetWindowSize(Window, value.Width, value.Height);
            SDL.SetWindowPosition(Window, (int) SDL.WindowposCentered, (int) SDL.WindowposCentered);
        }
    }

    public static string Title
    {
        get => _title;
        set
        {
            _title = value;
            SDL.SetWindowTitle(Window, _title + _details);
        }
    }

    public static string Details
    {
        get => _details;
        set
        {
            _details = value;
            SDL.SetWindowTitle(Window, _title + _details);
        }
    }

    public static bool Fullscreen
    {
        get => (SDL.GetWindowFlags(Window) & SDL.WindowFlags.Fullscreen) != 0;
        set => SDL.SetWindowFullscreen(Window, (byte) (value ? 1 : 0));
    }

    /// <summary>
    /// Gets/sets if the cursor is visible. If false, the cursor will be invisible and locked to the surface.
    /// </summary>
    /// <remarks>Only works on platforms that support mouse input.</remarks>
    public static bool CursorVisible
    {
        get => SDL.GetWindowRelativeMouseMode(Window);
        set => SDL.SetWindowRelativeMouseMode(Window, (byte) (!value ? 1 : 0));
    }
    
    /// <summary>
    /// Allow/disallow text input. This may cause on-screen keyboards to appear, etc.
    /// </summary>
    public static bool AllowTextInput
    {
        get => SDL.TextInputActive(Window);
        set
        {
            if (value)
                SDL.StartTextInput(Window);
            else
                SDL.StopTextInput(Window);
        }
    }

    public static unsafe VideoMode? VideoMode
    {
        get
        {
            SDL.DisplayMode* mode = SDL.GetWindowFullscreenMode(Window);
            if (mode != null)
            {
                return new VideoMode(new Size<int>(mode->W, mode->H), mode->RefreshRate);
            }

            return null;
        }
    }

    public static unsafe VideoMode[] AvailableVideoModes
    {
        get
        {
            int count;
            SDL.DisplayMode** modes = SDL.GetFullscreenDisplayModes(SDL.GetDisplayForWindow(Window), &count);
            VideoMode[] vidModes = new VideoMode[count];

            for (int i = 0; i < count; i++)
            {
                SDL.DisplayMode* mode = modes[i];
                vidModes[i] = new VideoMode(new Size<int>(mode->W, mode->H), mode->RefreshRate);
            }
            
            return vidModes;
        }
    }
    
    /// <summary>
    /// The underlying handle(s) to the surface, typically provided by the window manager.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown if the current platform does not support the surface.</exception>
    public static SurfaceInfo Info
    {
        get
        {
            /*SurfaceInfo info;
            SDL_PropertiesID props = SDL_GetWindowProperties(_window);
            
            if (OperatingSystem.IsWindows())
            {
                nint hinstance = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WIN32_INSTANCE_POINTER, 0);
                nint hwnd = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WIN32_HWND_POINTER, 0);

                //info = SurfaceInfo.Windows(hinstance, hwnd);
                info = new SurfaceInfo(hwnd);
            }
            else if (OperatingSystem.IsLinux())
            {
                string driver = SDL_GetCurrentVideoDriver()!;

                if (driver == "wayland")
                {
                    nint display = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER, 0);
                    nint surface = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER, 0);

                    info = SurfaceInfo.Wayland(display, surface);
                }
                else if (driver == "x11")
                {
                    nint display = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_X11_DISPLAY_POINTER, 0);
                    nint window = (nint) SDL_GetNumberProperty(props, SDL_PROP_WINDOW_X11_WINDOW_NUMBER, 0);

                    info = SurfaceInfo.Xlib(display, window);
                }
                else
                    throw new PlatformNotSupportedException();
            }
            else
                throw new PlatformNotSupportedException();

            return info;*/

            return new SurfaceInfo(Window, Size);
        }
    }

    public static float DisplayScale => SDL.GetWindowDisplayScale(Window);
    
    /// <summary>
    /// Create the <see cref="Surface"/> with the given options.
    /// </summary>
    /// <param name="options">The <see cref="WindowOptions"/> to use when creating this surface.</param>
    /// <exception cref="Exception">Thrown if the surface failed to create.</exception>
    public static void Create(in WindowOptions options)
    {
        _title = options.Title;
        
        Logger.Trace("Initializing SDL.");
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        SDL.WindowFlags flags = SDL.WindowFlags.InputFocus | SDL.WindowFlags.MouseFocus |
                                SDL.WindowFlags.HighPixelDensity;

        if (options.Resizable)
            flags |= SDL.WindowFlags.Resizable;

        if (options.FullScreen || EnvVar.IsTrue(EnvVar.Fullscreen))
            flags |= SDL.WindowFlags.Fullscreen;
        
        Logger.Trace("Creating window.");
        Window = SDL.CreateWindow(_title, options.Size.Width, options.Size.Height, flags);

        if (Window.IsNull)
            throw new Exception($"Failed to create window: {SDL.GetError()}");

        if (File.Exists("Icon.png"))
        {
            ImageResult result =
                ImageResult.FromMemory(File.ReadAllBytes("Icon.png"), ColorComponents.RedGreenBlueAlpha);

            unsafe
            {
                SDL.Surface* surface;

                fixed (byte* pData = result.Data)
                {
                    surface = SDL.CreateSurfaceFrom(result.Width, result.Height, SDL.PixelFormat.Abgr8888, pData,
                        result.Width * 4);
                }

                SDL.SetWindowIcon(Window, surface);
            }
        }

        /*if (EnvVar.TryGetInt(EnvVar.SurfaceDisplay, out int displayId))
        {
            uint[]? displays = SDL.GetDisplays(out _);
            Debug.Assert(displays != null);

            if (displayId >= 0 && displayId < displays.Length)
            {
                int centered = (int) SDL.WindowPosCenteredDisplay((int) displays[displayId]);
                SDL.SetWindowPosition(Window, centered, centered);
            }
        }*/
    }

    /// <summary>
    /// Destroy the surface.
    /// </summary>
    public static void Destroy()
    {
        SDL.DestroyWindow(Window);
    }
}