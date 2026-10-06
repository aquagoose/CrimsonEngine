using System.Runtime.CompilerServices;

namespace Crimson.Core;

/// <summary>
/// Utilities for bitwise operations.
/// </summary>
public static class BitUtils
{
    /// <summary>
    /// Round a value to the next power of 2.
    /// </summary>
    /// <param name="value">The value to round.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint RoundToNextPowerOf2(uint value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return ++value;
    }
}