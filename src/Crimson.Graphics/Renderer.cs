using System.Diagnostics;
using Crimson.Graphics.Utils;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// Responsible for drawing 2D and 3D scenes.
/// </summary>
public static class Renderer
{
    /// <summary>
    /// Gets if the renderer has been initialized.
    /// </summary>
    public static bool IsInitialized { get; private set; }

    internal static RenderContext Context = null!;

    /// <summary>
    /// Gets the name of the graphics backend associated with the renderer.
    /// </summary>
    public static string BackendName => SDL.GetGPUDeviceDriver(Context.Device);

    /// <summary>
    /// Initialize the renderer.
    /// </summary>
    /// <param name="window">The SDL3 window to associate with the renderer.</param>
    public static void Init(SDL.Window window)
    {
        Debug.Assert(!IsInitialized, "The renderer has already been initialized!");
        Context = new RenderContext(window);

        IsInitialized = true;
    }

    /// <summary>
    /// Free the renderer and release resources.
    /// </summary>
    public static void Free()
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");
        SDL.WaitForGPUIdle(Context.Device).Check("Wait for idle");

        // fancy disposal stuff to go here

        Context.Dispose();
        IsInitialized = false;
    }
}