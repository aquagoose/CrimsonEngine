using Crimson.Core;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// A texture is a GPU-stored bitmap that can be applied to <see cref="Material"/>s and <see cref="Sprite"/>s.
/// </summary>
public sealed class Texture : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Texture"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    private readonly RenderContext _context;

    internal readonly SDL.GPUTexture TextureHandle;

    /// <summary>
    /// The size in pixels.
    /// </summary>
    public readonly Size<uint> Size;

    /// <summary>
    /// The <see cref="PixelFormat"/> of the data contained by the texture.
    /// </summary>
    public readonly PixelFormat Format;

    /// <summary>
    /// Create a <see cref="Texture"/> from pixel data.
    /// </summary>
    /// <param name="data">The pixel data to load onto the texture.</param>
    /// <param name="size">The size in pixels.</param>
    /// <param name="format">The <see cref="PixelFormat"/>.</param>
    /// <param name="generateMips">Whether to generate mipmaps.</param>
    /// <remarks>If <paramref name="data"/> is <see langword="null"/>, the texture will be empty.
    /// On some platforms, this may not necessarily mean the texture is blank.</remarks>
    public unsafe Texture(byte[]? data, Size<uint> size, PixelFormat format, bool generateMips = true)
    {
        _context = Renderer.Context;
        Size = size;
        Format = format;

        SDL.GPUTextureFormat textureFormat = format switch
        {
            PixelFormat.RGBA8 => SDL.GPUTextureFormat.R8g8b8a8Unorm,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        SDL.GPUTextureCreateInfo textureInfo = new()
        {
            Type = SDL.GPUTextureType.Type2d,
            Format = textureFormat,
            Width = size.Width,
            Height = size.Height,
            LayerCountOrDepth = 1,
            NumLevels = 1, // todo generate mips
            Usage = SDL.GPUTextureUsageFlags.Sampler,
            SampleCount = SDL.GPUSampleCount.Count1
        };

        Logger.Trace($"Creating {size} texture.");
        TextureHandle = SDL.CreateGPUTexture(_context.Device, &textureInfo).Check("Create texture");
    }

    /// <summary>
    /// Create a <see cref="Texture"/> from a <see cref="Bitmap"/>.
    /// </summary>
    /// <param name="bitmap">The <see cref="Bitmap"/> containing pixel data.</param>
    /// <param name="generateMips">Whether to generate mipmaps.</param>
    public Texture(Bitmap bitmap, bool generateMips = true) : this(bitmap.Data, bitmap.Size, bitmap.Format, generateMips) { }

    /// <summary>
    /// Load a <see cref="Texture"/> from an image file.
    /// </summary>
    /// <param name="path">The path to the image to load.</param>
    /// <param name="generateMips">Whether to generate mipmaps.</param>
    public Texture(string path, bool generateMips = true) : this(new Bitmap(path), generateMips) { }

    /// <summary>
    /// Dispose of this <see cref="Texture"/>.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        SDL.ReleaseGPUTexture(_context.Device, TextureHandle);
    }
}