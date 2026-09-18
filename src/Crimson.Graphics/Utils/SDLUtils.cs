using piko.Core;
using piko.SDL3;

namespace Crimson.Graphics.Utils;

internal static class SDLUtils
{
    public static void Check(this bool b, string operation)
    {
        if (!b)
            throw new Exception($"SDL operation \"{operation}\" failed: {SDL.GetError()}");
    }

    public static nint Check(this nint value, string operation)
    {
        if (value == 0)
            throw new Exception($"SDL operation \"{operation}\" failed: {SDL.GetError()}");

        return value;
    }

    public static T Check<T>(this T handle, string operation) where T : IHandle
    {
        if (handle.IsNull)
            throw new Exception($"SDL operation \"{operation}\" failed: {SDL.GetError()}");

        return handle;
    }

    public static uint CalculateNumMips(uint width, uint height)
        => (uint) double.Floor(double.Log2(double.Max(width, height))) + 1;
}