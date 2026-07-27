using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// Describes the underlying windowing information for a surface.
/// </summary>
public readonly struct SurfaceInfo
{
    /// <summary>
    /// The native handle to the surface.
    /// </summary>
    public readonly SDL.Window Handle;

    /// <summary>
    /// The surface's size.
    /// </summary>
    public readonly Size<int> Size;

    public SurfaceInfo(SDL.Window handle, Size<int> size)
    {
        Handle = handle;
        Size = size;
    }
}