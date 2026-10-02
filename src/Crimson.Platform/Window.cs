using System.Diagnostics;
using System.Reflection;
using Crimson.Core;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Platform;

/// <summary>
/// The window that the engine is rendering to.
/// </summary>
/// <remarks>On some platforms, this may not be a floating window, and some properties will not apply.</remarks>
public static class Window
{
    /// <summary>
    /// Gets if the <see cref="Window"/> has been initialized.
    /// </summary>
    public static bool IsInitialized => !_window.IsNull;
    
    private static SDL.Window _window;

    /// <summary>
    /// Gets the SDL3 window handle.
    /// </summary>
    public static nint Handle => _window.Handle;

    /// <summary>
    /// Get or set the window size in window coordinates. This may not be representative of actual the window size.
    /// Use <see cref="SizeInPixels"/> to get the actual window size.
    /// </summary>
    public static Size<uint> Size
    {
        get
        {
            SDL.GetWindowSize(_window, out int w, out int h);
            return new Size<uint>((uint) w, (uint) h);
        }
        set => SDL.SetWindowSize(_window, (int) value.Width, (int) value.Height);
    }

    /// <summary>
    /// Get the window size in pixels.
    /// </summary>
    public static Size<uint> SizeInPixels
    {
        get
        {
            SDL.GetWindowSizeInPixels(_window, out int w, out int h);
            return new Size<uint>((uint) w, (uint) h);
        }
    }

    /// <summary>
    /// Get or set the window title.
    /// </summary>
    public static string Title
    {
        get => SDL.GetWindowTitle(_window);
        set => SDL.SetWindowTitle(_window, value);
    }

    /// <summary>
    /// Get or set if the window can be resized by the user.
    /// </summary>
    public static bool Resizable
    {
        get => (SDL.GetWindowFlags(_window) & SDL.WindowFlags.Resizable) != 0;
        set => SDL.SetWindowResizable(_window, value);
    }

    /// <summary>
    /// Initialize the window.
    /// </summary>
    /// <param name="info">The <see cref="WindowInfo"/> to use on window creation.</param>
    public static void Init(in WindowInfo info)
    {
        Debug.Assert(!IsInitialized, "The window has already been initialized!");
        
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        // a size of zero tells the engine to auto-decide what size to use.
        // in debug mode we usually want it as a window, so a 1280x720 window will do.
        // in release, we usually want fullscreen.
        Size<uint> size = info.Size;
        bool fullscreen = info.Fullscreen;
        if (size == Size<uint>.Zero)
        {
#if DEBUG
            size = new Size<uint>(1280, 720);
#else
            fullscreen = true;
#endif
        }
        
        uint windowProps = SDL.CreateProperties();
        SDL.SetBooleanProperty(windowProps, SDL.Prop.WindowCreateHiddenBoolean, true);
        SDL.SetStringProperty(windowProps, SDL.Prop.WindowCreateTitleString, info.Title);
        SDL.SetNumberProperty(windowProps, SDL.Prop.WindowCreateWidthNumber, size.Width);
        SDL.SetNumberProperty(windowProps, SDL.Prop.WindowCreateHeightNumber, size.Height);
        SDL.SetBooleanProperty(windowProps, SDL.Prop.WindowCreateResizableBoolean, info.Resizable);
        SDL.SetBooleanProperty(windowProps, SDL.Prop.WindowCreateFullscreenBoolean, fullscreen);
        SDL.SetBooleanProperty(windowProps, SDL.Prop.WindowCreateHighPixelDensityBoolean, true);
        
        Logger.Trace("Creating window.");
        _window = SDL.CreateWindowWithProperties(windowProps);
        if (_window.IsNull)
            throw new Exception($"Failed to create window: {SDL.GetError()}");
        SDL.DestroyProperties(windowProps);

        byte[] icon;
        if (OperatingSystem.IsMacOS())
            icon = Resource.Load("Crimson.Core.Assets.CrimsonIcon-MacOS.png", Assembly.GetAssembly(typeof(Logger)));
        else
            icon = Resource.Load("Crimson.Core.Assets.CrimsonIcon.png", Assembly.GetAssembly(typeof(Logger)));
        SDL.Surface surface;
        unsafe
        {
            fixed (byte* pIcon = icon)
            {
                SDL.IOStream io = SDL.IOFromMem((nint) pIcon, (nuint) icon.Length);
                surface = SDL.LoadPNGIO(io, true);
            }
        }

        if (!SDL.SetWindowIcon(_window, surface))
            throw new Exception($"Failed to set window icon: {SDL.GetError()}");

        if (!SDL.ShowWindow(_window))
            throw new Exception($"Failed to show window: {SDL.GetError()}");
    }

    /// <summary>
    /// Free the window.
    /// </summary>
    public static void Free()
    {
        Debug.Assert(IsInitialized, "The window has not been initialized!");
        
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}