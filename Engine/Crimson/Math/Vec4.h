#pragma once

#include "Coredefs.h"

#include <format>

namespace cge
{
    /**
     * A 4-dimensional Vector with an X, Y, Z, and W component.
     * @tparam T A numeric type.
     */
    template<typename T>
    struct Vec4 final
    {
        /**
         * The X component.
         */
        const T X;

        /**
         * The Y component.
         */
        const T Y;

        /**
         * The Z component.
         */
        const T Z;

        /**
         * The W component.
         */
        const T W;

        /**
         * Construct a Vec4 from X, Y, Z, and W components.
         * @param x The X component.
         * @param y The Y component.
         * @param z The Z component.
         * @param w The W component.
         */
        Vec4(T x, T y, T z, T w) : X(x), Y(y), Z(z), W(w) { }

        /**
         * Construct a Vec4 from a scalar value.
         * @param scalar The scalar value to apply to all components.
         */
        explicit Vec4(T scalar) : X(scalar), Y(scalar), Z(scalar), W(scalar) { }  // todo do we want explicit here?

        /**
         * Construct an empty Vec4 with all components initialized to 0.
         */
        Vec4()
        {
            X = 0;
            Y = 0;
            Z = 0;
            W = 0;
        }

        template<typename TOther>
        Vec4<TOther> As()
        {
            return {
                static_cast<TOther>(X),
                static_cast<TOther>(Y),
                static_cast<TOther>(Z),
                static_cast<TOther>(W),
            };
        }

        friend Vec4 operator +(const Vec4& lhs, const Vec4& rhs)
        {
            return {
                lhs.X + rhs.X,
                lhs.Y + rhs.Y,
                lhs.Z + rhs.Z,
                lhs.W + rhs.W
            };
        }

        friend Vec4 operator -(const Vec4& lhs, const Vec4& rhs)
        {
            return {
                lhs.X - rhs.X,
                lhs.Y - rhs.Y,
                lhs.Z - rhs.Z,
                lhs.W - rhs.W
            };
        }

        friend Vec4 operator *(const Vec4& lhs, const Vec4& rhs)
        {
            return {
                lhs.X * rhs.X,
                lhs.Y * rhs.Y,
                lhs.Z * rhs.Z,
                lhs.W * rhs.W
            };
        }

        friend Vec4 operator *(const Vec4& lhs, T rhs)
        {
            return {
                lhs.X * rhs,
                lhs.Y * rhs,
                lhs.Z * rhs,
                lhs.W * rhs
            };
        }

        friend Vec4 operator /(const Vec4& lhs, const Vec4& rhs)
        {
            return {
                lhs.X / rhs.X,
                lhs.Y / rhs.Y,
                lhs.Z / rhs.Z,
                lhs.W / rhs.W
            };
        }

        friend Vec4 operator /(const Vec4& lhs, T rhs)
        {
            return {
                lhs.X / rhs,
                lhs.Y / rhs,
                lhs.Z / rhs,
                lhs.W / rhs
            };
        }

        static Vec4 UnitX()
        {
            return { 1, 0, 0, 0 };
        }

        static Vec4 UnitY()
        {
            return { 0, 1, 0, 0 };
        }

        static Vec4 UnitZ()
        {
            return { 0, 0, 1, 0 };
        }

        static Vec4 UnitW()
        {
            return { 0, 0, 0, 1 };
        }
    };

    using Vec4s = Vec4<i8>;
    using Vec4b = Vec4<u8>;
    using Vec4i = Vec4<i32>;
    using Vec4u = Vec4<u32>;
    using Vec4f = Vec4<f32>;
    using Vec4d = Vec4<f64>;
}

template<typename T>
struct std::formatter<cge::Vec4<T>>
{
    template<class ParseContext>
    constexpr ParseContext::iterator parse(ParseContext& ctx) { return ctx.begin(); }

    template<class FmtContext>
    FmtContext::iterator format(cge::Vec4<T> vec, FmtContext& ctx) const
    {
        return std::format_to(ctx.out(), "X: {}, Y: {}, Z: {}, W: {}", vec.X, vec.Y, vec.Z, vec.W);
    }
};