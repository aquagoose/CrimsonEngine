using System.Diagnostics;
using System.Runtime.CompilerServices;
using piko.Core;
using piko.SDL3;

namespace Crimson.Graphics.Utils;

internal static unsafe class SdlUtils
{
    public static SDL.GPUColorTargetBlendState NonPremultipliedBlend => new()
    {
        EnableBlend = true,
        SrcColorBlendfactor = SDL.GPUBlendFactor.SrcAlpha,
        DstColorBlendfactor = SDL.GPUBlendFactor.OneMinusSrcAlpha,
        DstAlphaBlendfactor = SDL.GPUBlendFactor.One,
        SrcAlphaBlendfactor = SDL.GPUBlendFactor.One,
        ColorBlendOp = SDL.GPUBlendOp.Add,
        AlphaBlendOp = SDL.GPUBlendOp.Add,
    };

    public static SDL.GPUColorTargetBlendState NoBlend => new()
    {
        EnableBlend = false
    };
    
    public static IntPtr Check(this IntPtr ptr, string operation)
    {
        if (ptr == IntPtr.Zero)
            throw new Exception($"SDL operation '{operation}' failed: {SDL.GetError()}");

        return ptr;
    }

    public static T Check<T>(this T h, string operation) where T : IHandle
    {
        if (h.IsNull)
            throw new Exception($"SDL operation '{operation}' failed: {SDL.GetError()}");

        return h;
    }

    public static void Check(this bool b, string operation)
    {
        if (!b)
            throw new Exception($"SDL operation '{operation}' failed: {SDL.GetError()}");
    }

    [Conditional("DEBUG")]
    public static void PushDebugGroup(SDL.GPUCommandBuffer cb, string name)
    {
        // Doesn't work on directx?
        if (!OperatingSystem.IsWindows())
            SDL.PushGPUDebugGroup(cb, name);
    }

    [Conditional("DEBUG")]
    public static void PopDebugGroup(SDL.GPUCommandBuffer cb)
    {
        if (!OperatingSystem.IsWindows())
            SDL.PopGPUDebugGroup(cb);
    }

    public static uint CalculateMipLevels(uint width, uint height)
    {
        return (uint) double.Floor(double.Log2(uint.Max(width, height))) + 1;
    }

    public static SDL.GPUBuffer CreateBuffer(SDL.GPUDevice device, SDL.GPUBufferUsageFlags usage, uint size)
    {
        SDL.GPUBufferCreateInfo bufferInfo = new()
        {
            Usage = usage,
            Size = size
        };

        return SDL.CreateGPUBuffer(device, &bufferInfo).Check("Create buffer");
    }

    public static unsafe SDL.GPUBuffer CreateBuffer<T>(SDL.GPUDevice device, SDL.GPUBufferUsageFlags usage, ReadOnlySpan<T> data) where T : unmanaged
    {
        uint size = (uint) (data.Length * sizeof(T));
        
        SDL.GPUBuffer buffer = CreateBuffer(device, usage, size);

        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(device).Check("Acquire command buffer");
        Renderer.UpdateBuffer(cb, buffer, 0, data);
        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
        
        return buffer;
    }

    public static SDL.GPUBuffer CreateBuffer<T>(SDL.GPUDevice device, SDL.GPUBufferUsageFlags usage, T[] data) where T : unmanaged
        => CreateBuffer<T>(device, usage, data.AsSpan());

    public static SDL.GPUTransferBuffer CreateTransferBuffer(SDL.GPUDevice device, SDL.GPUTransferBufferUsage usage, uint size)
    {
        SDL.GPUTransferBufferCreateInfo bufferInfo = new()
        {
            Usage = usage,
            Size = size
        };

        return SDL.CreateGPUTransferBuffer(device, &bufferInfo).Check("Create transfer buffer");
    }

    public static SDL.GPUTexture CreateTexture2D(SDL.GPUDevice device, uint width, uint height, SDL.GPUTextureFormat format,
        uint mipLevels, SDL.GPUTextureUsageFlags usage = SDL.GPUTextureUsageFlags.Sampler)
    {
        SDL.GPUTextureCreateInfo textureInfo = new()
        {
            Type = SDL.GPUTextureType.Type2d,
            Width = width,
            Height = height,
            LayerCountOrDepth = 1,
            Format = format,
            Usage = usage,
            NumLevels = mipLevels == 0 ? CalculateMipLevels(width, height) : mipLevels,
            SampleCount = SDL.GPUSampleCount.Count1
        };

        return SDL.CreateGPUTexture(device, &textureInfo).Check("Create texture");
    }

    public static unsafe SDL.GPUTexture CreateTexture2D(SDL.GPUDevice device, nint data, uint width, uint height,
        SDL.GPUTextureFormat format, uint mipLevels, SDL.GPUTextureUsageFlags usage = SDL.GPUTextureUsageFlags.Sampler)
    {
        SDL.GPUTexture texture = CreateTexture2D(device, width, height, format, mipLevels, usage);

        uint size = SDL.CalculateGPUTextureFormatSize(format, width, height, 1);
        SDL.GPUTransferBuffer transferBuffer = CreateTransferBuffer(device, SDL.GPUTransferBufferUsage.Upload, size);

        void* transferData = (void*) SDL.MapGPUTransferBuffer(device, transferBuffer, 0);
        Unsafe.CopyBlock(transferData, (void*) data, size);
        SDL.UnmapGPUTransferBuffer(device, transferBuffer);

        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(device).Check("Acquire command buffer");
        SDL.GPUCopyPass pass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUTextureTransferInfo source = new()
        {
            TransferBuffer = transferBuffer,
            PixelsPerRow = width,
            RowsPerLayer = height,
            Offset = 0
        };

        SDL.GPUTextureRegion dest = new()
        {
            Texture = texture,
            X = 0,
            Y = 0,
            W = width,
            H = height,
            D = 1
        };
        
        SDL.UploadToGPUTexture(pass, &source, &dest, 0);
        
        SDL.EndGPUCopyPass(pass);
        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
        
        SDL.ReleaseGPUTransferBuffer(device, transferBuffer);
        
        return texture;
    }

    public static SDL.GPUTextureFormat ToSdl(this PixelFormat format, out uint rowPitch)
    {
        switch (format)
        {
            case PixelFormat.RGBA8:
                rowPitch = 4;
                return SDL.GPUTextureFormat.R8g8b8a8Unorm;
            case PixelFormat.BGRA8:
                rowPitch = 4;
                return SDL.GPUTextureFormat.B8g8r8a8Unorm;
            case PixelFormat.BC1:
                rowPitch = 1;
                return SDL.GPUTextureFormat.Bc1RgbaUnorm;
            case PixelFormat.BC1Srgb:
                rowPitch = 1;
                return SDL.GPUTextureFormat.Bc1RgbaUnormSrgb;
            case PixelFormat.BC2:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc2RgbaUnorm;
            case PixelFormat.BC2Srgb:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc2RgbaUnormSrgb;
            case PixelFormat.BC3:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc3RgbaUnorm;
            case PixelFormat.BC3Srgb:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc3RgbaUnormSrgb;
            case PixelFormat.BC4U:
                rowPitch = 1;
                return SDL.GPUTextureFormat.Bc4RUnorm;
            /*case PixelFormat.BC4S:
                rowPitch = 1;
                return SDL.GPUTextureFormat.BC4RSnorm;*/
            case PixelFormat.BC5U:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc5RgUnorm;
            /*case PixelFormat.BC5S:
                break;*/
            case PixelFormat.BC6U:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc6hRgbUfloat;
            case PixelFormat.BC6S:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc6hRgbFloat;
            case PixelFormat.BC7:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc7RgbaUnorm;
            case PixelFormat.BC7Srgb:
                rowPitch = 2;
                return SDL.GPUTextureFormat.Bc7RgbaUnormSrgb;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}