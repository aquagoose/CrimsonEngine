using System.Diagnostics;
using System.Reflection;
using Crimson.Core;
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

    public static nint Handle => _window.Handle;

    /// <summary>
    /// Initialize the window.
    /// </summary>
    /// <param name="info">The <see cref="WindowInfo"/> to use on window creation.</param>
    public static void Init(in WindowInfo info)
    {
        Debug.Assert(!IsInitialized, "The window has already been initialized!");
        
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");
        
        uint windowProps = SDL.CreateProperties();
        SDL.SetBooleanProperty(windowProps, SDL.Prop.WindowCreateHiddenBoolean, true);
        SDL.SetStringProperty(windowProps, SDL.Prop.WindowCreateTitleString, info.Title);
        SDL.SetNumberProperty(windowProps, SDL.Prop.WindowCreateWidthNumber, info.Size.Width);
        SDL.SetNumberProperty(windowProps, SDL.Prop.WindowCreateHeightNumber, info.Size.Height);
        SDL.SetBooleanProperty(windowProps, SDL.Prop.WindowCreateResizableBoolean, info.Resizable);
        
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