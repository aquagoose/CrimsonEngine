using System.Runtime.CompilerServices;

namespace Crimson.Core;

public static class BitUtils
{
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