using Crimson.Math;
using StbImageSharp;

namespace Crimson.Graphics;

/// <summary>
/// A 2-dimensional bitmap image with a format, size, and pixel data.
/// </summary>
public class Bitmap
{
    /// <summary>
    /// The pixel data. The data will be in the format denoted by <see cref="Format"/>.
    /// </summary>
    public readonly byte[] Data;

    /// <summary>
    /// The size in pixels.
    /// </summary>
    public readonly Size<uint> Size;

    /// <summary>
    /// The <see cref="PixelFormat"/> that the data is stored in.
    /// </summary>
    public readonly PixelFormat Format;

    /// <summary>
    /// Construct a <see cref="Bitmap"/> from raw data.
    /// </summary>
    /// <param name="data">The pixel data. The data will be in the format denoted by <paramref name="format"/>.</param>
    /// <param name="size">The size in pixels.</param>
    /// <param name="format">The <see cref="PixelFormat"/> that the data is stored in.</param>
    public Bitmap(byte[] data, Size<uint> size, PixelFormat format)
    {
        Data = data;
        Size = size;
        Format = format;
    }

    /// <summary>
    /// Load a bitmap from an image file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    public Bitmap(string path)
    {
        using FileStream stream = File.OpenRead(path);
        // force load as RGBA since RGB images are not supported and pixelformat doesn't yet have anything for R/RG
        ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        Data = result.Data;
        Size = new Size<uint>((uint) result.Width, (uint) result.Height);
        Format = PixelFormat.RGBA8; // since we're loading as RGBA, this is the format stbimage provides
    }
}