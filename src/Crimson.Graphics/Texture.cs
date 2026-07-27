using System.Runtime.CompilerServices;
using Crimson.Content;
using Crimson.Core;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// A texture image that is used during rendering.
/// </summary>
public class Texture : IContentResource<Texture>, IDisposable
{
    public bool IsDisposed { get; private set; }
    
    private readonly SDL.GPUDevice _device;
    private readonly bool _generateMipmaps;
    private readonly bool _isOwnedByRenderer;
    
    internal readonly SDL.GPUTexture TextureHandle;

    /// <summary>
    /// The texture's friendly name, if any.
    /// </summary>
    public readonly string? Name;

    /// <summary>
    /// The texture's size in pixels.
    /// </summary>
    public readonly Size<int> Size;

    /// <summary>
    /// The Texture's ID.
    /// </summary>
    public ulong ID => (ulong) TextureHandle.Handle;

    /// <summary>
    /// Create an empty <see cref="Texture"/>.
    /// </summary>
    /// <param name="size">The size, in pixels. If <see langword="null"/> is provided, a blank texture will be created.</param>
    /// <param name="format">The <see cref="PixelFormat"/> of the texture.</param>
    /// <param name="name">The texture's name used during debugging, if any.</param>
    /// <param name="numMipMaps">The number of mipmaps the texture contains. Set to 1 for no mipmaps. Set to 0 to
    /// generate a full set of mipmaps automatically, if the <paramref name="format"/> allows. For any other value, you
    /// will be expected to upload mipmaps manually through the <see cref="Update"/> method.</param>
    public unsafe Texture(in Size<int> size, PixelFormat format, string? name = null, uint numMipMaps = 0)
    {
        Name = name;
        Size = size;

        _device = Renderer.Device;
        SDL.GPUTextureUsageFlags flags = SDL.GPUTextureUsageFlags.Sampler;

        // Don't generate mipmaps if the format doesn't allow (i.e. compressed formats, etc.)
        if (numMipMaps == 0 && !format.IsCompressed())
        {
            numMipMaps = SdlUtils.CalculateMipLevels((uint) size.Width, (uint) size.Height);

            // Only enable mipmap generation if the calculated number of mipmaps is > 1, otherwise it is pointless.
            if (numMipMaps > 1)
            {
                _generateMipmaps = true;
                flags |= SDL.GPUTextureUsageFlags.ColorTarget;
            }
        }

        SDL.GPUTextureCreateInfo textureInfo = new()
        {
            Type = SDL.GPUTextureType.Type2d,
            Format = format.ToSdl(out _),
            Width = (uint) size.Width,
            Height = (uint) size.Height,
            LayerCountOrDepth = 1,
            NumLevels = numMipMaps,
            Usage = flags
        };

        Logger.Trace("Creating texture.");
        TextureHandle = SDL.CreateGPUTexture(_device, &textureInfo).Check("Create texture");
        
        if (name != null)
            SDL.SetGPUTextureName(_device, TextureHandle, name);
    }
    
    /// <summary>
    /// Create a <see cref="Texture"/>.
    /// </summary>
    /// <param name="size">The size, in pixels. If <see langword="null"/> is provided, a blank texture will be created.</param>
    /// <param name="data">The data.</param>
    /// <param name="format">The <see cref="PixelFormat"/> of the texture.</param>
    /// <param name="name">The texture's name used during debugging, if any.</param>
    /// <param name="numMipMaps">The number of mipmaps the texture contains. Set to 1 for no mipmaps. Set to 0 to
    /// generate a full set of mipmaps automatically, if the <paramref name="format"/> allows. For any other value, you
    /// will be expected to upload mipmaps manually through the <see cref="Update"/> method.</param>
    public Texture(in Size<int> size, byte[] data, PixelFormat format, string? name = null, uint numMipMaps = 0)
        : this(in size, format, name, numMipMaps)
    {
        Update(new Rectangle<int>(Vector2T<int>.Zero, Size), data);
    }

    /// <summary>
    /// Create a <see cref="Texture"/> from a bitmap.
    /// </summary>
    /// <param name="bitmap">The <see cref="Bitmap"/> to use.</param>
    /// <param name="name">The texture's name used during debugging, if any.</param>
    /// <param name="numMipMaps">The number of mipmaps the texture contains. Set to 1 for no mipmaps. Set to 0 to
    /// generate a full set of mipmaps automatically, if the format allows. For any other value, you will be expected
    /// to upload mipmaps manually through the <see cref="Update"/> method.</param>
    public Texture(Bitmap bitmap, string? name = null, uint numMipMaps = 0)
        : this(bitmap.Size, bitmap.Data, bitmap.Format, name, numMipMaps) { }

    /// <summary>
    /// Create a <see cref="Texture"/> from a <see cref="DDS"/>.
    /// </summary>
    /// <param name="dds">The <see cref="DDS"/> containing the texture data.</param>
    /// <param name="name">The texture's name used during debugging, if any.</param>
    /// <remarks>
    /// DDS files <b>MUST</b> contain pre-generated mipmaps if mipmapping is to be used. Textures cannot be
    /// configured to automatically generate mipmaps when created from a DDS.<br />
    /// Textures will always be created from the first array layer. Any other array layers are ignored.
    /// </remarks>
    public Texture(DDS dds, string? name = null) : this(dds.Size, dds.Format, name, dds.MipLevels)
    {
        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(_device).Check("Acquire command buffer");
        
        for (uint i = 0; i < dds.MipLevels; i++)
        {
            Bitmap bitmap = dds.Bitmaps[0, i];
            
            SDL.GPUTextureRegion dest = new()
            {
                Texture = TextureHandle,
                X = 0,
                Y = 0,
                W = (uint) bitmap.Size.Width,
                H = (uint) bitmap.Size.Height,
                D = 1,
                MipLevel = i
            };
        
            Renderer.UpdateTexture(cb, dest, bitmap.Data);
        }
        
        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }

    /// <summary>
    /// Create a <see cref="Texture"/> from the given path. 
    /// </summary>
    /// <param name="path">The path to load from.</param>
    ///// <param name="name">The texture's name used during debugging, if any. If nothing is provided, the path will be used.</param>
    public Texture(string path) : this(new Bitmap(path), path) { }

    internal Texture(SDL.GPUTexture textureHandle, Size<int> size, string name)
    {
        Name = name;
        Size = size;
        TextureHandle = textureHandle;
        _isOwnedByRenderer = true;
    }

    /// <summary>
    /// Update a region of texture data.
    /// </summary>
    /// <param name="region">The region to update.</param>
    /// <param name="data">The bitmap data.</param>
    /// <param name="mipLevel">The mipmap level, if applicable.</param>
    public void Update(Rectangle<int> region, byte[] data, uint mipLevel = 0)
    {
        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(_device).Check("Acquire command buffer");

        SDL.GPUTextureRegion dest = new()
        {
            Texture = TextureHandle,
            X = (uint) region.X,
            Y = (uint) region.Y,
            W = (uint) region.Width,
            H = (uint) region.Height,
            D = 1,
            MipLevel = mipLevel
        };
        
        Renderer.UpdateTexture(cb, dest, data);

        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");

        if (_generateMipmaps)
            Renderer.MipmapQueue.Add(TextureHandle);
    }

    /// <summary>
    /// Dispose of this <see cref="Texture"/>.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        if (_isOwnedByRenderer)
            return;
        
        SDL.ReleaseGPUTexture(_device, TextureHandle);
    }
    
    public static Texture White { get; internal set; }
    
    public static Texture Black { get; internal set; }
    
    public static Texture EmptyNormal { get; internal set; }
    
    public static Texture LoadResource(string fullPath, bool hasExtension)
    {
        if (hasExtension)
        {
            if (DDS.IsDDSFile(fullPath))
                return new Texture(new DDS(fullPath));
            
            return new Texture(fullPath);
        }

        string[] extensions = [".dds", ".png", ".jpg", ".bmp"];

        foreach (string extension in extensions)
        {
            fullPath = Path.ChangeExtension(fullPath, extension);
            if (!File.Exists(fullPath))
                continue;
            
            if (DDS.IsDDSFile(fullPath))
                return new Texture(new DDS(fullPath));
                
            return new Texture(fullPath);
        }

        throw new FileNotFoundException(fullPath);
    }
}