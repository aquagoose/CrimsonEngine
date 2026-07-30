using Crimson.Math;
using StbImageSharp;

namespace Crimson.Graphics;

/// <summary>
/// A 2D image represented by a size, pixel format and pixel data.
/// </summary>
public class Bitmap
{
    /// <summary>
    /// The pixel data. This will be in the format denoted by <see cref="Format"/>.
    /// </summary>
    public readonly byte[] Data;

    /// <summary>
    /// The size in pixels.
    /// </summary>
    public readonly Size<uint> Size;

    /// <summary>
    /// The <see cref="PixelFormat"/>.
    /// </summary>
    public readonly PixelFormat Format;

    /// <summary>
    /// Load a bitmap from a file path.
    /// </summary>
    /// <param name="path">The file path to load from.</param>
    /// <remarks>This uses stb_image to load the image, and can load all filetypes that it supports.</remarks>
    public Bitmap(string path)
    {
        using FileStream stream = File.OpenRead(path);
        ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        Data = result.Data;
        Size = new Size<uint>((uint) result.Width, (uint) result.Height);
        Format = PixelFormat.RGBA8; // stb_image always returns RGBA8 data (when specifying RGBA color components)
    }
}