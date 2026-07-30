using System.Diagnostics;
using Crimson.Graphics.SDLGPU;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// Crimson's renderer, responsible for UI, 2D, and 3D rendering.
/// </summary>
public static class Renderer
{
    /// <summary>
    /// The <see cref="GPUContext"/> instance, holding an SDL3 GPU device.
    /// </summary>
    internal static GPUContext Context = null!;

    /// <summary>
    /// Initialize the renderer.
    /// </summary>
    /// <param name="window">The <see cref="SDL.Window"/> to associate this renderer with.</param>
    public static void Init(SDL.Window window)
    {
        Debug.Assert(Context == null, "Renderer has already been initialized!");
        Context = new GPUContext(window);
    }

    /// <summary>
    /// De-initialize and free the renderer.
    /// </summary>
    public static void Free()
    {
        Debug.Assert(Context != null, "Renderer has not been initialized!");
        SDL.WaitForGPUIdle(Context.Device).Check("Wait for GPU idle");


        // fancy dispose logic to go later


        Context.Dispose();
    }
}