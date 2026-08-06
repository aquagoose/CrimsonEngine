using System.Diagnostics;
using Crimson.Core;
using Crimson.Graphics.Rendering;
using Crimson.Graphics.SDLGPU;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// Crimson's renderer, responsible for UI, 2D, and 3D rendering.
/// </summary>
public static class Renderer
{
    /// <summary>
    /// Gets if this <see cref="Renderer"/> is initialized.
    /// </summary>
    public static bool IsInitialized => Context != null;

    private static SDL.Window _window;

    /// <summary>
    /// The texture batcher for UI elements.
    /// </summary>
    private static TexturedQuadBatcher _uiBatcher = null!;

    /// <summary>
    /// The <see cref="GPUContext"/> instance, holding an SDL3 GPU device.
    /// </summary>
    internal static GPUContext Context = null!;

    // todo: HashSet<SDL.Texture>: piko doesn't have equality members for handle types, but it should
    internal static HashSet<Texture> MipmapQueue = null!;

    /// <summary>
    /// Initialize the renderer.
    /// </summary>
    /// <param name="window">The <see cref="SDL.Window"/> to associate this renderer with.</param>
    public static void Init(SDL.Window window)
    {
        Debug.Assert(!IsInitialized, "Renderer has already been initialized!");
        _window = window;
        Context = new GPUContext(window);
        MipmapQueue = [];

        SDL.GPUTextureFormat mainTargetFormat = SDL.GetGPUSwapchainTextureFormat(Context.Device, _window);

        Logger.Trace("Creating UI batcher.");
        _uiBatcher = new TexturedQuadBatcher(Context, mainTargetFormat);
    }

    /// <summary>
    /// De-initialize and free the renderer.
    /// </summary>
    public static void Free()
    {
        Debug.Assert(IsInitialized, "Renderer has not been initialized!");
        SDL.WaitForGPUIdle(Context.Device).Check("Wait for GPU idle");

        _uiBatcher.Dispose();

        Context.Dispose();
    }

    /// <summary>
    /// Render everything to the window.
    /// </summary>
    public static unsafe void Render()
    {
        Debug.Assert(IsInitialized, "Renderer has not been initialized!");

        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(Context.Device).Check("Acquire command buffer");

        foreach (Texture texture in MipmapQueue)
        {
            Logger.Trace($"Generating mipmaps for texture {texture.Handle.Handle}.");
            SDL.GenerateMipmapsForGPUTexture(cb, texture.Handle);
        }

        MipmapQueue.Clear();

        SDL.GPUTexture swapchainTexture;
        SDL.WaitAndAcquireGPUSwapchainTexture(cb, _window, &swapchainTexture, null, null)
            .Check("Acquire swapchain texture");

        // don't try to render if there is nothing to render to!
        // https://wiki.libsdl.org/SDL3/SDL_WaitAndAcquireGPUSwapchainTexture#remarks
        if (swapchainTexture.IsNull)
        {
            SDL.CancelGPUCommandBuffer(cb).Check("Cancel command buffer");
            return;
        }

        SDL.GPUColorTargetInfo targetInfo = new()
        {
            Texture = swapchainTexture,
            ClearColor = new SDL.FColor(1.0f, 0.5f, 0.25f, 1.0f),
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass pass = SDL.BeginGPURenderPass(cb, &targetInfo, 1, null).Check("Begin render pass");
        SDL.EndGPURenderPass(pass);

        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }
}