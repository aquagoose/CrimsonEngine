using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Graphics.SDLGPU;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// An image that can be applied to <see cref="Material"/>s and <see cref="Sprite"/>s.
/// Differs from a <see cref="Bitmap"/> in that it is stored in Video Memory and therefore can be used by the GPU,
/// however cannot be easily modified.
/// </summary>
public class Texture : IDisposable
{
    private readonly GPUContext _context;

    /// <summary>
    /// The SDLGPU texture handle.
    /// </summary>
    internal readonly SDL.GPUTexture Handle;

    /// <summary>
    /// The size, in pixels.
    /// </summary>
    public readonly Size<uint> Size;

    /// <summary>
    /// The <see cref="PixelFormat"/>.
    /// </summary>
    public readonly PixelFormat Format;

    /// <summary>
    /// Create a <see cref="Texture"/> from pixel data.
    /// </summary>
    /// <param name="data">The pixel data to use. If <see langword="null"/> is provided, an empty texture will be created.</param>
    /// <param name="size">The size, in pixels, of the texture.</param>
    /// <param name="format">The <see cref="PixelFormat"/>.</param>
    /// <param name="generateMips">Whether to generate mipmaps.</param>
    public unsafe Texture(byte[]? data, Size<uint> size, PixelFormat format, bool generateMips = true)
    {
        _context = Renderer.Context;
        Size = size;
        Format = format;

        SDL.GPUTextureCreateInfo textureInfo = new()
        {
            Type = SDL.GPUTextureType.Type2d,
            Width = Size.Width,
            Height = Size.Height,
            Format = Format.ToSDL(),
            NumLevels = 1, // todo calculate mip levels
            LayerCountOrDepth = 1,
            Usage = SDL.GPUTextureUsageFlags.Sampler,
            SampleCount = SDL.GPUSampleCount.Count1
        };

        Logger.Trace($"Creating {Size} texture.");
        Handle = SDL.CreateGPUTexture(_context.Device, &textureInfo).Check("Create texture");

        if (data != null)
            _context.CopyDataToTexture(Handle, 0, 0, Size, Format, data);
    }

    /// <summary>
    /// Create a <see cref="Texture"/> from a <see cref="Bitmap"/>.
    /// </summary>
    /// <param name="bitmap">The <see cref="Bitmap"/> containing the pixel data.</param>
    /// <param name="generateMips">Whether to generate mipmaps.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Texture(Bitmap bitmap, bool generateMips = true) : this(bitmap.Data, bitmap.Size, bitmap.Format, generateMips) {}

    /// <summary>
    /// Create a <see cref="Texture"/> from a file path.
    /// </summary>
    /// <param name="path">The path to the image.</param>
    /// <param name="generateMips">Whether to generate mipmaps.</param>
    /// <remarks>
    /// Internally this calls <see cref="Bitmap(string)"/>, and therefore uses stb_image to load the image, meaning that
    /// this constructor supports all filetypes that it supports.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Texture(string path, bool generateMips = true) : this(new Bitmap(path), generateMips) { }

    public void Dispose()
    {
        SDL.ReleaseGPUTexture(_context.Device, Handle);
    }
}