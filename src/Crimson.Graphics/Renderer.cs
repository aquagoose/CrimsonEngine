using System.Diagnostics;
using Crimson.Core;
using Crimson.Graphics.Rendering;
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

    private static TextureBatcher _uiBatcher = null!;
    
    internal static RenderContext Context = null!;

    // todo: Renderer.BackgroundColor

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

        SDL.GPUTextureFormat format = SDL.GetGPUSwapchainTextureFormat(Context.Device, Context.Window);
        _uiBatcher = new TextureBatcher(Context, format);
        
        IsInitialized = true;
    }

    /// <summary>
    /// Free the renderer and release resources.
    /// </summary>
    public static void Free()
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");
        SDL.WaitForGPUIdle(Context.Device).Check("Wait for idle");
        _uiBatcher.Dispose();
        Context.Dispose();
        IsInitialized = false;
    }

    /// <summary>
    /// Render the scene to the window.
    /// </summary>
    public static void Render()
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");

        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(Context.Device).Check("Acquire command buffer");

        SDL.WaitAndAcquireGPUSwapchainTexture(cb, Context.Window, out SDL.GPUTexture swapchainTexture, out _, out _)
            .Check("Acquire swapchain texture");

        // don't bother rendering if there is nothing to render to
        if (swapchainTexture.IsNull)
        {
            SDL.CancelGPUCommandBuffer(cb);
            return;
        }

        foreach (SDL.GPUTexture texture in Context.MipmapQueue)
        {
            Logger.Trace($"Generating mipmaps for texture {texture.Handle}.");
            SDL.GenerateMipmapsForGPUTexture(cb, texture);
        }
        Context.MipmapQueue.Clear();

        SDL.GPUColorTargetInfo colorTarget = new()
        {
            Texture = swapchainTexture,
            ClearColor = new SDL.FColor(1.0f, 0.5f, 0.25f, 1.0f),
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass pass = SDL.BeginGPURenderPass(cb, [colorTarget], null).Check("Begin render pass");
        SDL.EndGPURenderPass(pass);

        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }
}