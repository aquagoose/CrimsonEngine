using Crimson.Math;
using StbImageSharp;

namespace Crimson.Graphics;

/// <summary>
/// A 2-dimensional image containing pixel data.
/// </summary>
public sealed class Bitmap
{
    /// <summary>
    /// The pixel data in the format defined by <see cref="Format"/>.
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
    /// Construct a <see cref="Bitmap"/> from pixel data.
    /// </summary>
    /// <param name="data">The pixel data in the format defined by <paramref name="format"/>.</param>
    /// <param name="size">The size in pixels.</param>
    /// <param name="format">The <see cref="PixelFormat"/>.</param>
    public Bitmap(byte[] data, Size<uint> size, PixelFormat format)
    {
        Data = data;
        Size = size;
        Format = format;
    }

    /// <summary>
    /// Load a <see cref="Bitmap"/> from an image file.
    /// </summary>
    /// <param name="path">The path of the image to load.</param>
    public Bitmap(string path)
    {
        using FileStream stream = File.OpenRead(path);
        // force load as RGBA, as RGB8 is not supported
        // todo support R8/RG8?
        ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        Data = result.Data;
        Size = new Size<uint>((uint) result.Width, (uint) result.Height);
        Format = PixelFormat.RGBA8; // the image is always loaded as 8-bit RGBA
    }
}