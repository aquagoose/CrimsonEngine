#pragma once
#include "Math/Coredefs.h"

namespace cge::BitUtils
{
    inline u32 RoundToNearestPowerOf2(u32 value)
    {
        // https://graphics.stanford.edu/%7Eseander/bithacks.html#RoundUpPowerOf2
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
