#pragma once

#include <cstdint>
#include <limits>

#define U8_MIN 0
#define U8_MAX UINT8_MAX
#define U16_MIN 0
#define U16_MAX UINT16_MAX
#define U32_MIN 0
#define U32_MAX UINT32_MAX
#define U64_MIN 0
#define U64_MAX UINT64_MAX
#define USIZE_MIN 0
#define USIZE_MAX UINTPTR_MAX

#define I8_MIN INT8_MIN
#define I8_MAX INT8_MAX
#define I16_MIN INT16_MIN
#define I16_MAX INT16_MAX
#define I32_MIN INT32_MIN
#define I32_MAX INT32_MAX
#define I64_MIN INT64_MIN
#define I64_MAX INT64_MAX
#define ISIZE_MIN INTPTR_MIN
#define ISIZE_MAX INTPTR_MAX

#define F32_MAX std::numeric_limits<f32>::max()
#define F32_MIN std::numeric_limits<f32>::min()
#define F64_MAX std::numeric_limits<f64>::max()
#define F64_MIN std::numeric_limits<f64>::min()

namespace cge
{
    using u8 = uint8_t;
    using u16 = uint16_t;
    using u32 = uint32_t;
    using u64 = uint64_t;
    using usize = uintptr_t;

    using i8 = int8_t;
    using i16 = int16_t;
    using i32 = int32_t;
    using i64 = int64_t;
    using isize = intptr_t;

    using f32 = float;
    using f64 = double;
}
