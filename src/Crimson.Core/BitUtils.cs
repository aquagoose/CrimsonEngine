using System.Runtime.CompilerServices;

namespace Crimson.Core;

/// <summary>
/// Contains various bitwise utility functions.
/// </summary>
public static class BitUtils
{
    /// <summary>
    /// Round an unsigned integer to the next power of 2.
    /// </summary>
    /// <param name="value">The value to round up.</param>
    /// <returns>The value rounded to the next power of 2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint RoundToNextPowerOf2(uint value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value++;

        return value;
    }
}