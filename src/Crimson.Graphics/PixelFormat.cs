namespace Crimson.Graphics;

/// <summary>
/// Represents supported pixel formats.
/// </summary>
public enum PixelFormat
{
    /// <summary>
    /// 32-bit RGBA, 8bpp.
    /// </summary>
    RGBA8
}

public static class PixelFormatExtensions
{
    extension(PixelFormat fmt)
    {
        public uint BytesPerPixel => fmt switch
        {
            PixelFormat.RGBA8 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(fmt), fmt, null)
        };

        public uint BitsPerPixel => fmt.BytesPerPixel * 8;
    }
}