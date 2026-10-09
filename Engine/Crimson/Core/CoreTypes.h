#pragma once

#include <cstdint>
#include <memory>

namespace cge
{
    using i8 = std::int8_t;
    using i16 = std::int16_t;
    using i32 = std::int32_t;
    using i64 = std::int64_t;
    using isize = std::intptr_t;

    using u8 = std::uint8_t;
    using u16 = std::uint16_t;
    using u32 = std::uint32_t;
    using u64 = std::uint64_t;
    using usize = std::uintptr_t;

    using f32 = float;
    using f64 = double;

    template<typename T>
    using Ptr = std::unique_ptr<T>;

    template<typename T, typename... Args>
    Ptr<T> MakeUnique(Args&&... args)
    {
        return std::make_unique<T>(std::forward<Args...>(args...));
    }
}