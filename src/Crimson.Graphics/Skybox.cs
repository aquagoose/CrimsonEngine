using System.Diagnostics;
using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// A skybox is a 6-sided textured cubemap that can provide an environment backdrop.
/// </summary>
public class Skybox : IDisposable
{
    private readonly RenderContext _context;
    private readonly SDL.GPUTexture _cubemap;
    
    public unsafe Skybox(Bitmap left, Bitmap right, Bitmap top, Bitmap bottom, Bitmap front, Bitmap back)
    {
        _context = Renderer.Context;
        // todo add some debug assertions for checking sizes and formats are equal
        Size<uint> size = left.Size;
        PixelFormat format = left.Format;

        SDL.GPUTextureCreateInfo textureInfo = new()
        {
            Type = SDL.GPUTextureType.TypeCube,
            Width = size.Width,
            Height = size.Height,
            LayerCountOrDepth = 6,
            Format = format.ToSDL(),
            NumLevels = 1,
            SampleCount = SDL.GPUSampleCount.Count1,
            Usage = SDL.GPUTextureUsageFlags.Sampler
        };
        
        Logger.Trace("Creating cubemap texture.");
        _cubemap = SDL.CreateGPUTexture(_context.Device, &textureInfo).Check("Create cubemap");

        uint dataSize = size.Width * size.Height * format.BytesPerPixel;
        uint totalDataSize = dataSize * 6; // multiply by 6 as cube has 6 faces... duh
        SDL.GPUTransferBuffer transBuffer = _context.GetUploadBuffer(totalDataSize, out uint offset, out bool cycle);
        nint mapped = SDL.MapGPUTransferBuffer(_context.Device, transBuffer, cycle).Check("Map transfer buffer");
        
        // yeah this is
        // this is some code of all time
        fixed (void* pLeft = left.Data)
        fixed (void* pRight = right.Data)
        fixed (void* pTop = top.Data)
        fixed (void* pBottom = bottom.Data)
        fixed (void* pFront = front.Data)
        fixed (void* pBack = back.Data)
        {
            Unsafe.CopyBlock((byte*) (mapped + offset + (dataSize * 0)), pLeft, dataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (dataSize * 1)), pRight, dataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (dataSize * 2)), pTop, dataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (dataSize * 3)), pBottom, dataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (dataSize * 4)), pFront, dataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (dataSize * 5)), pBack, dataSize);
        }
        
        SDL.UnmapGPUTransferBuffer(_context.Device, transBuffer);

        Logger.Trace($"Uploading {totalDataSize / 1024}KiB data to cubemap.");
        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(_context.Device).Check("Acquire command buffer");

        SDL.GPUCopyPass pass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUTextureTransferInfo src = new()
        {
            TransferBuffer = transBuffer,
            Offset = offset,
            PixelsPerRow = size.Width,
            RowsPerLayer = size.Height
        };
        
        SDL.GPUTextureRegion dest = new()
        {
            Texture = _cubemap,
            X = 0,
            Y = 0,
            Z = 0,
            W = size.Width,
            H = size.Height,
            D = 1,
            Layer = 0,
            MipLevel = 0
        };
        
        SDL.UploadToGPUTexture(pass, &src, &dest, false);
        src.Offset += dataSize;
        dest.Layer++;
        SDL.UploadToGPUTexture(pass, &src, &dest, false);
        src.Offset += dataSize;
        dest.Layer++;
        SDL.UploadToGPUTexture(pass, &src, &dest, false);
        src.Offset += dataSize;
        dest.Layer++;
        SDL.UploadToGPUTexture(pass, &src, &dest, false);
        src.Offset += dataSize;
        dest.Layer++;
        SDL.UploadToGPUTexture(pass, &src, &dest, false);
        src.Offset += dataSize;
        dest.Layer++;
        SDL.UploadToGPUTexture(pass, &src, &dest, false);
        
        SDL.EndGPUCopyPass(pass);
        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }

    public Skybox(string left, string right, string top, string bottom, string front, string back) : this(
        new Bitmap(left), new Bitmap(right), new Bitmap(top), new Bitmap(bottom), new Bitmap(front),
        new Bitmap(back)) { }

    public void Dispose()
    {
        SDL.ReleaseGPUTexture(_context.Device, _cubemap);
    }
}