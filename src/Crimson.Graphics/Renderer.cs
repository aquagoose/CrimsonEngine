using System.Diagnostics;
using System.Numerics;
using Crimson.Core;
using Crimson.Graphics.Rendering;
using Crimson.Graphics.Rendering.Structs;
using Crimson.Graphics.Utils;
using Crimson.Math;
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

    private static Size<uint> _renderSize;
    
    private static TextureBatcher _uiBatcher = null!;
    
    internal static RenderContext Context = null!;

    /// <summary>
    /// Get or set the background color which is used when a skybox is not present.
    /// </summary>
    public static Color BackgroundColor;

    /// <summary>
    /// Gets the render size in pixels.
    /// </summary>
    public static Size<uint> Size => _renderSize;
    
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
        BackgroundColor = Color.Black;
        
        Context = new RenderContext(window);

        SDL.GetWindowSizeInPixels(window, out int w, out int h);
        _renderSize = new Size<uint>((uint) w, (uint) h);

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
    /// Draw a 2D image.
    /// </summary>
    /// <param name="texture">The <see cref="Texture"/> to use as the image.</param>
    /// <param name="position">The position, in pixels, to draw at.</param>
    /// <param name="tint">The color the image should be tinted with, if any.</param>
    /// <remarks>The origin point is the top left of the texture.</remarks>
    public static void DrawImage(Texture texture, Vector2 position, Color? tint = null)
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");
        
        Vector2 topLeft = position;
        Vector2 topRight = position + new Vector2(texture.Size.Width, 0);
        Vector2 bottomLeft = position + new Vector2(0, texture.Size.Height);
        Vector2 bottomRight = position + new Vector2(texture.Size.Width, texture.Size.Height);

        TextureBatcher.Draw draw =
            new TextureBatcher.Draw(texture, topLeft, topRight, bottomLeft, bottomRight, tint ?? Color.White);
        
        _uiBatcher.AddToBatch(in draw);
    }

    /// <summary>
    /// Signal that a new frame has begun, and reset various states ready for new draw commands.
    /// </summary>
    public static void NewFrame()
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");
        _uiBatcher.Clear();
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

        ClearInfo clearInfo = new ClearInfo(BackgroundColor, false);

        Camera uiCamera = new()
        {
            Projection = Matrix4x4.CreateOrthographicOffCenter(0, _renderSize.Width, _renderSize.Height, 0, -1, 1),
            View = Matrix4x4.Identity
        };
        _uiBatcher.Render(cb, swapchainTexture, in uiCamera, ref clearInfo);

        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }

    /// <summary>
    /// Resize the <see cref="Renderer"/>.
    /// </summary>
    /// <param name="newSize">The new size, in pixels.</param>
    public static void Resize(in Size<uint> newSize)
    {
        Debug.Assert(IsInitialized, "The renderer has not been initialized!");
        _renderSize = newSize;
    }
}