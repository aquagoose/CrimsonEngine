using System.Runtime.CompilerServices;
using piko.Core;
using piko.SDL3;

namespace Crimson.Graphics.SDLGPU;

internal static class SDLUtils
{
    /// <summary>
    /// Check the result of an SDL operation that returns a boolean value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Check(this bool b, string operation)
    {
        if (!b)
            throw new Exception($"SDL operation '{operation}' failed: {SDL.GetError()}");
    }

    /// <summary>
    /// Check the result of an SDL operation that returns a handle type. The handle will be returned if successful.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Check<T>(this T handle, string operation) where T : IHandle
    {
        if (handle.IsNull)
            throw new Exception($"SDL operation '{operation}' failed: {SDL.GetError()}");

        return handle;
    }

    public static uint CalculateMipLevels(uint width, uint height)
    {
        return (uint) double.Floor(double.Log2(double.Max(width, height))) + 1;
    }

    public static SDL.GPUTextureFormat ToSDL(this PixelFormat format)
    {
        return format switch
        {
            PixelFormat.RGBA8 => SDL.GPUTextureFormat.R8g8b8a8Unorm,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}