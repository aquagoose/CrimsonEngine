using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Graphics.Renderers;
using Crimson.Graphics.Renderers.Structs;
using Crimson.Graphics.Utils;
using Crimson.Math;
using Hexa.NET.ImGui;
using piko.SDL3;
using piko.SDL3.ShaderCross;
using Color = Crimson.Math.Color;

namespace Crimson.Graphics;

/// <summary>
/// The graphics subsystem, containing everything used to render.
/// </summary>
public static class Renderer
{
    private static uint _transferBufferOffset;
    private static uint _transferBufferSize = 32 * 1024 * 1024;
    
    private static SDL.Window _window;
    private static Backend _backend;
    
    private static SDL.GPUTexture _depthTexture;

    private static bool _vsyncEnabled;
    private static Size<int> _swapchainSize;

    private static SDL.GPUTransferBuffer _transferBuffer;
    
    private static TextureBatcher _uiBatcher;
    private static ImGuiRenderer? _imGuiRenderer;
    private static DeferredRenderer? _deferredRenderer;
    private static SpriteRenderer? _spriteRenderer;

    internal static SDL.GPUDevice Device;

    internal static SDL.GPUTextureFormat MainTargetFormat;

    internal static HashSet<SDL.GPUTexture> MipmapQueue;

    public static Backend Backend => _backend;

    /// <summary>
    /// The 3D <see cref="Crimson.Graphics.Camera"/> that will be used when drawing.
    /// </summary>
    public static Camera Camera;

    /// <summary>
    /// Get the render area size in pixels.
    /// </summary>
    public static Size<int> RenderSize => _swapchainSize;

    /// <summary>
    /// Enable/disable vertical sync.
    /// </summary>
    public static bool VSync
    {
        get => _vsyncEnabled;
        set
        {
            if (value == _vsyncEnabled)
                return;
            
            _vsyncEnabled = value;
            SDL.SetGPUSwapchainParameters(Device, _window, SDL.GPUSwapchainComposition.Sdr,
                value ? SDL.GPUPresentMode.Vsync : SDL.GPUPresentMode.Immediate).Check("Set swapchain parameters");
        }
    }

    /// <summary>
    /// Gets the ImGUI context pointer.
    /// </summary>
    public static ImGuiContextPtr? ImGuiContext
    {
        get
        {
            //Debug.Assert(_imGuiRenderer != null, "Renderer has not been created with CreateImGuiRenderer enabled.");
            return _imGuiRenderer?.Context;
        }
    }

    /// <summary>
    /// The main renderer debug textures.
    /// </summary>
    public static Texture[] DebugRendererTextures => _deferredRenderer!.DebugTextures;
    
    /// <summary>
    /// Create the graphics subsystem.
    /// </summary>
    /// <param name="appName">The application name.</param>
    /// <param name="surface">The <see cref="SurfaceInfo"/> to use when creating the subsystem.</param>
    public static void Create(string appName, in RendererOptions options, in SurfaceInfo surface)
    {
        _swapchainSize = surface.Size;

        _window = surface.Handle;

        SDL.SetAppMetadata(appName, null!, null!);

        uint props = SDL.CreateProperties();
        SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersSpirvBoolean, 1);

        if (options.Debug)
        {
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateDebugmodeBoolean, 1);
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreatePreferlowpowerBoolean, 1);
        }

        if (OperatingSystem.IsWindows() && options.Backend is Backend.Unknown or Backend.D3D12)
        {
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersDxilBoolean, 1);
            // Use D3D12 on windows
            SDL.SetStringProperty(props, SDL.Prop.GpuDeviceCreateNameString, "direct3d12");
        }

        if (OperatingSystem.IsMacOS() && options.Backend is Backend.Unknown or Backend.Metal)
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersMslBoolean, 1);

        Logger.Trace("Creating device.");
        Device = SDL.CreateGPUDeviceWithProperties(props).Check("Create device");

        _backend = SDL.GetGPUDeviceDriver(Device) switch
        {
            "vulkan" => Backend.Vulkan,
            "direct3d12" => Backend.D3D12,
            "metal" => Backend.Metal,
            _ => Backend.Unknown
        };
        
        Logger.Debug($"Using SDL backend: {_backend}");
        
        Logger.Trace("Claiming window for device.");
        SDL.ClaimWindowForGPUDevice(Device, _window).Check("Claim window for device");
        
        VSync = true;

        _depthTexture = SdlUtils.CreateTexture2D(Device, (uint) _swapchainSize.Width, (uint) _swapchainSize.Height,
            SDL.GPUTextureFormat.D32Float, 1, SDL.GPUTextureUsageFlags.DepthStencilTarget);

        MainTargetFormat = SDL.GetGPUSwapchainTextureFormat(Device, _window);

        // Create transfer buffer for use in various upload operations.
        _transferBuffer =
            SdlUtils.CreateTransferBuffer(Device, SDL.GPUTransferBufferUsage.Upload, _transferBufferSize);

        MipmapQueue = [];
        Logger.Trace("Initializing ShaderCross");
        SDLShaderCross.Init();

        Logger.Debug($"options.Type: {options.Type}");
        Logger.Debug($"options.ImGui.CreateRenderer: {options.ImGui.CreateRenderer}");
        
        Logger.Trace("Creating UI renderer.");
        _uiBatcher = new TextureBatcher(Device, SDL.GetGPUSwapchainTextureFormat(Device, _window));

        if (options.ImGui.CreateRenderer)
        {
            Logger.Trace("Creating ImGUI renderer.");
            _imGuiRenderer = new ImGuiRenderer(Device, RenderSize, MainTargetFormat, options.ImGui);
        }

        if ((options.Type & RendererType.Create3D) != 0)
        {
            Logger.Trace("Creating deferred 3D renderer.");
            _deferredRenderer = new DeferredRenderer(Device, _swapchainSize, MainTargetFormat);
        }

        if ((options.Type & RendererType.Create2D) != 0)
        {
            Logger.Trace("Creating sprite renderer.");
            _spriteRenderer = new SpriteRenderer(Device, MainTargetFormat);
        }

        Logger.Trace("Creating default textures.");
        Texture.White = new Texture(new Size<int>(1), [255, 255, 255, 255], PixelFormat.RGBA8);
        Texture.Black = new Texture(new Size<int>(1), [0, 0, 0, 255], PixelFormat.RGBA8);
        Texture.EmptyNormal = new Texture(new Size<int>(1), [128, 128, 255, 255], PixelFormat.RGBA8);

        Camera = new Camera()
        {
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(float.DegreesToRadians(45),
                _swapchainSize.Width / (float)_swapchainSize.Height, 0.1f, 100f),
            ViewMatrix = Matrix4x4.CreateLookAt(new Vector3(0, 0, 3), Vector3.Zero, Vector3.UnitY)
        };
    }

    /// <summary>
    /// Destroy the graphics subsystem.
    /// </summary>
    public static void Destroy()
    {
        Texture.EmptyNormal.Dispose();
        Texture.EmptyNormal = null!;
        Texture.Black.Dispose();
        Texture.Black = null!;
        Texture.White.Dispose();
        Texture.White = null!;
        
        _deferredRenderer?.Dispose();
        _imGuiRenderer?.Dispose();
        _uiBatcher.Dispose();
        
        SDLShaderCross.Quit();

        SDL.ReleaseGPUTransferBuffer(Device, _transferBuffer);

        SDL.ReleaseGPUTexture(Device, _depthTexture);
        SDL.ReleaseWindowFromGPUDevice(Device, _window);
        SDL.DestroyGPUDevice(Device);
    }

    /// <summary>
    /// Draw a <see cref="Renderable"/> to the screen using the built-in renderers.
    /// </summary>
    /// <param name="renderable">The <see cref="Renderable"/> to draw.</param>
    /// <param name="worldMatrix">The world matrix.</param>
    public static void DrawRenderable(Renderable renderable, Matrix4x4 worldMatrix)
    {
        Debug.Assert(_deferredRenderer != null, "Renderer has not been created with 3D rendering enabled.");
        _deferredRenderer.AddToQueue(renderable, worldMatrix);
    }

    /// <summary>
    /// Draw a <see cref="Sprite"/> to the screen using the built-in renderers.
    /// </summary>
    /// <param name="sprite">The sprite to draw.</param>
    /// <param name="matrix">The matrix.</param>
    /// <remarks>Set the sprite's color to red to see just how deep the rabbit hole goes.</remarks>
    public static void DrawSprite(Sprite sprite, Matrix<float> matrix)
    {
        Debug.Assert(_spriteRenderer != null, "Renderer has not been created with 2D rendering enabled.");
        _spriteRenderer.DrawSprite(in sprite, matrix);
    }
    
    /// <summary>
    /// Draw an image to the screen with the given size.
    /// </summary>
    /// <param name="texture">The texture to use as the image.</param>
    /// <param name="position">The position, in pixels.</param>
    /// <param name="size">The size, in pixels.</param>
    /// <param name="source">The source rectangle to use, if any.</param>
    /// <param name="tint">The tint to use, if any.</param>
    /// <param name="blend">The blending mode to use on draw.</param>
    public static void DrawImage(Texture texture, Vector2T<int> position, Size<int> size, Rectangle<int>? source = null,
        Color? tint = null, BlendMode blend = BlendMode.Blend)
    {
        Rectangle<int> src = source ?? new Rectangle<int>(Vector2T<int>.Zero, texture.Size);
        
        Vector2T<int> topLeft = position;
        Vector2T<int> topRight = position + new Vector2T<int>(size.Width, 0);
        Vector2T<int> bottomLeft = position + new Vector2T<int>(0, size.Height);
        Vector2T<int> bottomRight = position + new Vector2T<int>(size.Width, size.Height);

        _uiBatcher.AddToDrawQueue(new TextureBatcher.Draw(texture, topLeft.As<float>(), topRight.As<float>(),
            bottomLeft.As<float>(), bottomRight.As<float>(), src, tint ?? Color.White, blend));
    }

    /// <summary>
    /// Draw an image to the screen.
    /// </summary>
    /// <param name="texture">The texture to use as the image.</param>
    /// <param name="position">The position, in pixels.</param>
    /// <param name="source">The source rectangle to use, if any.</param>
    /// <param name="tint">The tint to use, if any.</param>
    /// <param name="blend">The blending mode to use on draw.</param>
    public static void DrawImage(Texture texture, Vector2T<int> position, Rectangle<int>? source = null,
        Color? tint = null, BlendMode blend = BlendMode.Blend)
    {
        DrawImage(texture, position, source?.Size ?? texture.Size, source, tint, blend);
    }

    /// <summary>
    /// Draw text to the screen.
    /// </summary>
    /// <param name="font">The <see cref="Font"/> to use.</param>
    /// <param name="position">The position, in pixels.</param>
    /// <param name="size">The font size, in pixels.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="color">The text's color.</param>
    public static void DrawText(Font font, Vector2T<int> position, uint size, string text, Color color)
    {
        font.Draw(_uiBatcher, position, size, text, color);
    }

    /// <summary>
    /// Draw a line from a to b.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="color">The line color.</param>
    /// <param name="thickness">The line thickness in pixels.</param>
    public static void DrawLine(in Vector2T<int> a, in Vector2T<int> b, in Color color, int thickness)
    {
        float halfThickness = thickness / 2.0f;

        Vector2T<float> topLeft;
        Vector2T<float> bottomLeft;
        Vector2T<float> topRight;
        Vector2T<float> bottomRight;

        // Use fast paths if drawing hline or vline as it avoids the complex matrix calculations
        if (a.X == b.X)
        {
            topLeft = new Vector2T<float>(a.X - halfThickness, a.Y);
            topRight = new Vector2T<float>(a.X + halfThickness, a.Y);
            bottomLeft = new Vector2T<float>(b.X - halfThickness, b.Y);
            bottomRight = new Vector2T<float>(b.X + halfThickness, b.Y);
        }
        else if (a.Y == b.Y)
        {
            topLeft = new Vector2T<float>(a.X, a.Y - halfThickness);
            bottomLeft = new Vector2T<float>(a.X, a.Y + halfThickness);
            topRight = new Vector2T<float>(b.X, b.Y - halfThickness);
            bottomRight = new Vector2T<float>(b.X, b.Y + halfThickness);
        }
        else
        {
            Vector2T<float> fA = a.As<float>();
            Vector2T<float> fB = b.As<float>();
            
            float rot = float.Atan2(b.Y - a.Y, b.X - a.X);
            float dist = Vector2T.Distance(fA, fB);

            Matrix<float> rotMatrix = Matrix.RotateZ(rot);

            topLeft = Vector2T.Transform(new Vector2T<float>(0, -halfThickness), rotMatrix) + fA;
            bottomLeft = Vector2T.Transform(new Vector2T<float>(0, +halfThickness), rotMatrix) + fA;
            topRight = Vector2T.Transform(new Vector2T<float>(dist, -halfThickness), rotMatrix) + fA;
            bottomRight = Vector2T.Transform(new Vector2T<float>(dist, +halfThickness), rotMatrix) + fA;
        }

        _uiBatcher.AddToDrawQueue(new TextureBatcher.Draw(Texture.White, topLeft, topRight, bottomLeft, bottomRight,
            new Rectangle<int>(0, 0, 1, 1), color, BlendMode.Blend));
    }

    public static void DrawFilledRectangle(Vector2T<int> position, Size<int> size, Color color)
    {
        Vector2T<int> topLeft = position;
        Vector2T<int> topRight = new Vector2T<int>(position.X + size.Width, position.Y);
        Vector2T<int> bottomLeft = new Vector2T<int>(position.X, position.Y + size.Height);
        Vector2T<int> bottomRight = new Vector2T<int>(position.X + size.Width, position.Y + size.Height);

        _uiBatcher.AddToDrawQueue(new TextureBatcher.Draw(Texture.White, topLeft.As<float>(), topRight.As<float>(),
            bottomLeft.As<float>(), bottomRight.As<float>(), new Rectangle<int>(0, 0, 1, 1), color, BlendMode.Blend));
    }

    public static void DrawBorderRectangle(Vector2T<int> position, Size<int> size, int borderWidth, Color color)
    {
        Size<int> hSize = new Size<int>(size.Width, borderWidth);
        Size<int> vSize = new Size<int>(borderWidth, size.Height);
        
        // Top
        DrawFilledRectangle(position, hSize, color);
        // Left
        DrawFilledRectangle(position, vSize, color);
        // Bottom
        DrawFilledRectangle(position + new Vector2T<int>(0, size.Height - borderWidth), hSize, color);
        // Right
        DrawFilledRectangle(position + new Vector2T<int>(size.Width - borderWidth, 0), vSize, color);
    }

    public static void DrawRectangle(Vector2T<int> position, Size<int> size, Color fillColor, int borderWidth,
        Color borderColor)
    {
        DrawFilledRectangle(position, size, fillColor);
        DrawBorderRectangle(position, size, borderWidth, borderColor);
    }

    public static void NewFrame()
    {
        Metrics.BeginPerformanceMetric(Metrics.RenderTimeMetric);
        
        Camera = default;
    }

    /// <summary>
    /// Render and present to the surface.
    /// </summary>
    public static unsafe void Render()
    {
        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(Device).Check("Acquire command buffer");

        foreach (SDL.GPUTexture texture in MipmapQueue)
        {
            Logger.Trace($"Generating mipmaps for texture handle {texture}.");
            SDL.GenerateMipmapsForGPUTexture(cb, texture);
        }

        MipmapQueue.Clear();

        SDL.WaitAndAcquireGPUSwapchainTexture(cb, _window, out SDL.GPUTexture swapchainTexture, null, null)
            .Check("Acquire swapchain texture");

        if (swapchainTexture.IsNull)
            return;
        
        // Each Render() method returns a boolean. If true, it means the renderer has cleared the target provided as the
        // color/compositeTarget parameter. Each renderer has the ability to clear the render/depth targets (if applicable)
        // if necessary.
        // This acts as a small optimization. Each renderer will not bother rendering if there is nothing to do.

        bool hasCleared = _deferredRenderer?.Render(cb, swapchainTexture, _depthTexture, Camera.Matrices) ?? false;

        if (Camera.Skybox?.Render(cb, swapchainTexture, _depthTexture, !hasCleared, Camera.Matrices) ?? false)
            hasCleared = true;

        if (_spriteRenderer?.Render(cb, swapchainTexture, !hasCleared, _swapchainSize, Camera.Matrices) ?? false)
            hasCleared = true;

        Matrix4x4 projection =
            Matrix4x4.CreateOrthographicOffCenter(0, _swapchainSize.Width, _swapchainSize.Height, 0, -1, 1);

        if (_uiBatcher.Render(cb, swapchainTexture, !hasCleared, _swapchainSize,
                new CameraMatrices(projection, Matrix4x4.Identity)))
        {
            hasCleared = true;
        }

        _imGuiRenderer?.Render(cb, swapchainTexture, !hasCleared);
        
        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
        Metrics.EndPerformanceMetric(Metrics.RenderTimeMetric);
    }

    /// <summary>
    /// Resize the renderer.
    /// </summary>
    /// <param name="newSize">The new size to set.</param>
    public static void Resize(Size<int> newSize)
    {
        _swapchainSize = newSize;
        
        SDL.ReleaseGPUTexture(Device, _depthTexture);

        _depthTexture = SdlUtils.CreateTexture2D(Device, (uint) newSize.Width, (uint) newSize.Height,
            SDL.GPUTextureFormat.D32Float, 1, SDL.GPUTextureUsageFlags.DepthStencilTarget);
        
        _deferredRenderer?.Resize(newSize);
        _imGuiRenderer?.Resize(newSize);
    }

    internal static SDL.GPUTransferBuffer GetTransferBuffer(uint size, out uint transferOffset)
    {
        if (size >= _transferBufferSize)
        {
            uint newSize = MathHelper.ToNextPowerOf2(size);
            Logger.Debug($"Resizing transfer buffer from {_transferBufferSize / 1024}KiB to {newSize / 1024}KiB");
            _transferBufferSize = newSize;
            SDL.ReleaseGPUTransferBuffer(Device, _transferBuffer);
            _transferBuffer =
                SdlUtils.CreateTransferBuffer(Device, SDL.GPUTransferBufferUsage.Upload, _transferBufferSize);
            _transferBufferOffset = 0;
        }

        if (_transferBufferOffset + size >= _transferBufferSize)
            _transferBufferOffset = 0;

        transferOffset = _transferBufferOffset;
        _transferBufferOffset += size;
        return _transferBuffer;
    }

    internal static unsafe void UpdateBuffer<T>(SDL.GPUCommandBuffer cb, SDL.GPUBuffer buffer, uint offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        // First we ensure the transfer buffer is large enough to fit the buffer in its memory. If it isn't, then create
        // a new buffer.
        // It's rare that a buffer upload will exceed even 4mb, so cycling the buffer for each upload is extremely
        // wasteful. Instead we write to the buffer at an offset, and only cycle when there's no more space left in the
        // buffer. In theory, a more advanced algorithm could be used to only cycle when absolutely necessary, but this
        // algorithm is good enough, and SDL should be able to ignore the cycle instruction if the buffer is not bound.
        // Doing it this way saves a significant amount of GPU memory as it will not need to create multiple large
        // transfer buffers for every buffer update.
        
        uint dataLength = (uint) (data.Length * sizeof(T));
        SDL.GPUTransferBuffer transferBuffer = GetTransferBuffer(dataLength, out uint transferOffset);

        bool cycle = transferOffset == 0;
        Logger.Trace($"Updating buffer {buffer}: Cycle: {cycle}, Offset: {transferOffset}");
        void* map = (void*) SDL.MapGPUTransferBuffer(Device, transferBuffer, (byte) (cycle ? 1 : 0));
        fixed (void* pData = data)
            Unsafe.CopyBlock((byte*) map + transferOffset, pData, dataLength);
        SDL.UnmapGPUTransferBuffer(Device, transferBuffer);

        SDL.GPUCopyPass pass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUTransferBufferLocation src = new()
        {
            TransferBuffer = transferBuffer,
            Offset = transferOffset
        };

        SDL.GPUBufferRegion dest = new()
        {
            Buffer = buffer,
            Offset = offset,
            Size = dataLength
        };
        
        SDL.UploadToGPUBuffer(pass, &src, &dest, 0);
        
        SDL.EndGPUCopyPass(pass);
    }

    internal static unsafe void UpdateTexture(SDL.GPUCommandBuffer cb, SDL.GPUTextureRegion region, byte[] data)
    {
        // A similar reuse-buffer-but-cycle-when-necessary method is used here, see the comment in UpdateBuffer to get
        // an idea of how this works.
        
        uint dataLength = (uint) data.Length;
        SDL.GPUTransferBuffer transferBuffer = GetTransferBuffer(dataLength, out uint transferOffset);
        
        bool cycle = transferOffset == 0;
        Logger.Trace($"Updating texture {region.Texture}: Cycle: {cycle}, Offset: {transferOffset}");
        void* map = (void*) SDL.MapGPUTransferBuffer(Device, transferBuffer, 1);
        fixed (byte* pData = data)
            Unsafe.CopyBlock((byte*) map + transferOffset, pData, dataLength);
        SDL.UnmapGPUTransferBuffer(Device, transferBuffer);

        SDL.GPUCopyPass pass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUTextureTransferInfo transferInfo = new()
        {
            TransferBuffer = _transferBuffer,
            Offset = transferOffset,
            PixelsPerRow = region.W
        };
        
        SDL.UploadToGPUTexture(pass, &transferInfo, &region, 0);
        
        SDL.EndGPUCopyPass(pass);
    }
}