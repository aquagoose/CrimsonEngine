using System.Diagnostics;
using Crimson.Core;
using Crimson.Graphics.Utils;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// Performs 2D and 3D rendering to draw objects to the display.
/// </summary>
public static class Renderer
{
    /// <summary>
    /// Gets if the <see cref="Renderer"/> has been initialized.
    /// </summary>
    public static bool IsInitialized { get; private set; }

    internal static RendererContext Context;

    /// <summary>
    /// Gets the graphics backend name for this renderer.
    /// </summary>
    public static string BackendName => SDL.GetGPUDeviceDriver(Context.Device);

    /// <summary>
    /// Initialize the <see cref="Renderer"/>.
    /// </summary>
    /// <param name="window">The SDL3 window to associate with this renderer.</param>
    public static void Init(SDL.Window window)
    {
        Debug.Assert(!IsInitialized, "The renderer has already been initialized!");

        Context = new RendererContext(window);

        IsInitialized = true;
    }

    /// <summary>
    /// Free the current <see cref="Renderer"/>.
    /// </summary>
    public static void Free()
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");
        SDL.WaitForGPUIdle(Context.Device);

        Context.Dispose();
        IsInitialized = false;
    }

    /// <summary>
    /// Render everything to the display.
    /// </summary>
    public static void Render()
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");

        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(Context.Device).Check("Acquire command buffer");

        SDL.WaitAndAcquireGPUSwapchainTexture(cb, Context.Window, out SDL.GPUTexture swapchainTexture, out _, out _)
            .Check("Acquire swapchain texture");

        // don't try to render if there's nothing to render to.
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

        SDL.GPURenderPass pass = SDL.BeginGPURenderPass(cb, [targetInfo], null).Check("Begin render pass");
        SDL.EndGPURenderPass(pass);

        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }
}