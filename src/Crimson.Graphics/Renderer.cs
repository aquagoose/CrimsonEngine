using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Crimson.Core;
using Crimson.Graphics.Rendering;
using Crimson.Graphics.SDLGPU;
using Crimson.Math;
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
    private static SpriteBatcher _uiBatcher = null!;

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

        Logger.Trace("Creating built-in textures.");
        Texture.White = new Texture([255, 255, 255, 255], new Size<uint>(1), PixelFormat.RGBA8, false);
        Texture.Black = new Texture([0, 0, 0, 255], new Size<uint>(1), PixelFormat.RGBA8, false);
        Texture.Debug = new Texture(new Bitmap(Resource.Load("Crimson.Graphics.DEBUG.png", Assembly.GetExecutingAssembly())));

        Logger.Trace("Creating UI batcher.");
        _uiBatcher = new SpriteBatcher(Context, mainTargetFormat);
    }

    /// <summary>
    /// De-initialize and free the renderer.
    /// </summary>
    public static void Free()
    {
        Debug.Assert(IsInitialized, "Renderer has not been initialized!");
        SDL.WaitForGPUIdle(Context.Device).Check("Wait for GPU idle");

        _uiBatcher.Dispose();

        Texture.Debug.Dispose();
        Texture.Black.Dispose();
        Texture.White.Dispose();

        Context.Dispose();
    }

    public static void DrawImage(Texture texture, Vector2 position)
    {
        _uiBatcher.Draw(new SpriteBatcher.Sprite(
            texture: texture,
            topLeft: position,
            topRight: new Vector2(position.X + texture.Size.Width, position.Y),
            bottomLeft: new Vector2(position.X, position.Y + texture.Size.Height),
            bottomRight: new Vector2(position.X + texture.Size.Width, position.Y + texture.Size.Height),
            tint: Color.White
        ));
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

        bool hasCleared = false;

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, 1280, 720, 0, -1, 1);
        Matrix4x4 transform = Matrix4x4.Identity;
        _uiBatcher.Render(cb, swapchainTexture, new SpriteBatcher.TransformMatrices(projection, transform), !hasCleared);

        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }
}