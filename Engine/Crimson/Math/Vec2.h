#pragma once

#include "Coredefs.h"
#include "Utils.h"

#include <format>
#include <cmath>

namespace cge
{
    /**
     * A 2-dimensional Vector with an X and Y component.
     * @tparam T A numeric type.
     */
    template<typename T>
    struct Vec2 final
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
         * Construct a Vec2 from X and Y components.
         * @param x The X component.
         * @param y The Y component.
         */
        Vec2(T x, T y) : X(x), Y(y) { }

        /**
         * Construct a Vec2 from a scalar value.
         * @param scalar The scalar value to apply to all components.
         */
        explicit Vec2(T scalar) : X(scalar), Y(scalar) { }  // todo do we want explicit here?

        /**
         * Construct an empty Vec2 with all components initialized to 0.
         */
        Vec2()
        {
            X = 0;
            Y = 0;
        }

        /**
         * Cast the components of this Vec2 to another type.
         * @tparam TOther The numeric type to cast to.
         * @return A Vec2 with the component type cast to the given type.
         */
        template<typename TOther>
        Vec2<TOther> As() const
        {
            return {
                static_cast<TOther>(X),
                static_cast<TOther>(Y)
            };
        }

        /**
         * Calculate the squared length/magnitude of this Vec2.
         */
        T LengthSquared() const
        {
            return Dot(*this, *this);
        }

        /**
         * Calculate the length/magnitude of this Vec2.
         */
        T Length() const
        {
            return std::sqrt(LengthSquared());
        }

        /**
         * Get the normalized value of this Vec2.
         */
        Vec2 Normalized() const
        {
            return *this / Length();
        }

        friend Vec2 operator +(const Vec2& lhs, const Vec2& rhs)
        {
            return {
                lhs.X + rhs.X,
                lhs.Y + rhs.Y
            };
        }

        friend Vec2 operator -(const Vec2& lhs, const Vec2& rhs)
        {
            return {
                lhs.X - rhs.X,
                lhs.Y - rhs.Y
            };
        }

        friend Vec2 operator *(const Vec2& lhs, const Vec2& rhs)
        {
            return {
                lhs.X * rhs.X,
                lhs.Y * rhs.Y
            };
        }

        friend Vec2 operator *(const Vec2& lhs, T rhs)
        {
            return {
                lhs.X * rhs,
                lhs.Y * rhs
            };
        }

        friend Vec2 operator /(const Vec2& lhs, const Vec2& rhs)
        {
            return {
                lhs.X / rhs.X,
                lhs.Y / rhs.Y
            };
        }

        friend Vec2 operator /(const Vec2& lhs, T rhs)
        {
            return {
                lhs.X / rhs,
                lhs.Y / rhs
            };
        }

        /**
         * Get a Vec2 where the X component is 1, and all other components are 0.
         */
        static Vec2 UnitX()
        {
            return { 1, 0 };
        }

        /**
         * Get a Vec2 where the Y component is 1, and all other components are 0.
         */
        static Vec2 UnitY()
        {
            return { 0, 1 };
        }

        /**
         * Calculate the dot product of two Vec2s.
         * @param a The first vector.
         * @param b The second vector.
         */
        static T Dot(const Vec2& a, const Vec2& b)
        {
            return a.X * b.X +
                   a.Y * b.Y;
        }

        /**
         * Linearly interpolate between two Vec2 by the amount.
         * @param from The Vec2 to interpolate from.
         * @param to The Vec2 to interpolate to.
         * @param amount The amount, between 0 and 1, to interpolate by.
         * @return A Vec2 between from and to, interpolated by amount.
         */
        static Vec2 Lerp(const Vec2& from, const Vec2& to, T amount)
        {
            return {
                CGE_LERP(from.X, to.X, amount),
                CGE_LERP(from.Y, to.Y, amount)
            };
        }
    };

    using Vec2s = Vec2<i8>;
    using Vec2b = Vec2<u8>;
    using Vec2i = Vec2<i32>;
    using Vec2u = Vec2<u32>;
    using Vec2f = Vec2<f32>;
    using Vec2d = Vec2<f64>;
}

template<typename T>
struct std::formatter<cge::Vec2<T>>
{
    template<class ParseContext>
    constexpr ParseContext::iterator parse(ParseContext& ctx) { return ctx.begin(); }

    template<class FmtContext>
    FmtContext::iterator format(cge::Vec2<T> vec, FmtContext& ctx) const
    {
        return std::format_to(ctx.out(), "X: {}, Y: {}", vec.X, vec.Y);
    }
};