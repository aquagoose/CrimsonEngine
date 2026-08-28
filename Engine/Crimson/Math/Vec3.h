#pragma once

#include "Coredefs.h"
#include "Utils.h"

#include <format>
#include <cmath>

namespace cge
{
    /**
     * A 3-dimensional Vector with an X, Y, and Z component.
     * @tparam T A numeric type.
     */
    template<typename T>
    struct Vec3 final
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
         * Construct a Vec3 from X, Y, and Z components.
         * @param x The X component.
         * @param y The Y component.
         * @param z The Z component.
         */
        Vec3(T x, T y, T z) : X(x), Y(y), Z(z) { }

        /**
         * Construct a Vec3 from a scalar value.
         * @param scalar The scalar value to apply to all components.
         */
        explicit Vec3(T scalar) : X(scalar), Y(scalar), Z(scalar) { }  // todo do we want explicit here?

        /**
         * Construct an empty Vec3 with all components initialized to 0.
         */
        Vec3()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        /**
         * Cast the components of this Vec3 to another type.
         * @tparam TOther The numeric type to cast to.
         * @return A Vec3 with the component type cast to the given type.
         */
        template<typename TOther>
        Vec3<TOther> As() const
        {
            return {
                static_cast<TOther>(X),
                static_cast<TOther>(Y),
                static_cast<TOther>(Z),
            };
        }

        /**
         * Calculate the squared length/magnitude of this Vec3.
         */
        T LengthSquared() const
        {
            return Dot(*this, *this);
        }

        /**
         * Calculate the length/magnitude of this Vec3.
         */
        T Length() const
        {
            return std::sqrt(LengthSquared());
        }

        /**
         * Get the normalized value of this Vec3.
         */
        Vec3 Normalized() const
        {
            return *this / Length();
        }

        friend Vec3 operator +(const Vec3& lhs, const Vec3& rhs)
        {
            return {
                lhs.X + rhs.X,
                lhs.Y + rhs.Y,
                lhs.Z + rhs.Z
            };
        }

        friend Vec3 operator -(const Vec3& lhs, const Vec3& rhs)
        {
            return {
                lhs.X - rhs.X,
                lhs.Y - rhs.Y,
                lhs.Z - rhs.Z
            };
        }

        friend Vec3 operator *(const Vec3& lhs, const Vec3& rhs)
        {
            return {
                lhs.X * rhs.X,
                lhs.Y * rhs.Y,
                lhs.Z * rhs.Z
            };
        }

        friend Vec3 operator *(const Vec3& lhs, T rhs)
        {
            return {
                lhs.X * rhs,
                lhs.Y * rhs,
                lhs.Z * rhs
            };
        }

        friend Vec3 operator /(const Vec3& lhs, const Vec3& rhs)
        {
            return {
                lhs.X / rhs.X,
                lhs.Y / rhs.Y,
                lhs.Z / rhs.Z
            };
        }

        friend Vec3 operator /(const Vec3& lhs, T rhs)
        {
            return {
                lhs.X / rhs,
                lhs.Y / rhs,
                lhs.Z / rhs
            };
        }

        /**
         * Get a Vec3 where the X component is 1, and all other components are 0.
         */
        static Vec3 UnitX()
        {
            return { 1, 0, 0 };
        }

        /**
         * Get a Vec3 where the Y component is 1, and all other components are 0.
         */
        static Vec3 UnitY()
        {
            return { 0, 1, 0 };
        }

        /**
         * Get a Vec3 where the Z component is 1, and all other components are 0.
         */
        static Vec3 UnitZ()
        {
            return { 0, 0, 1 };
        }

        /**
         * Calculate the dot product of two Vec3s.
         * @param a The first vector.
         * @param b The second vector.
         */
        static T Dot(const Vec3& a, const Vec3& b)
        {
            return a.X * b.X +
                   a.Y * b.Y +
                   a.Z * b.Z;
        }

        /**
         * Linearly interpolate between two Vec3 by the amount.
         * @param from The Vec3 to interpolate from.
         * @param to The Vec3 to interpolate to.
         * @param amount The amount, between 0 and 1, to interpolate by.
         * @return A Vec3 between from and to, interpolated by amount.
         */
        static Vec3 Lerp(const Vec3& from, const Vec3& to, T amount)
        {
            return {
                CGE_LERP(from.X, to.X, amount),
                CGE_LERP(from.Y, to.Y, amount),
                CGE_LERP(from.Z, to.Z, amount)
            };
        }
    };

    using Vec3s = Vec3<i8>;
    using Vec3b = Vec3<u8>;
    using Vec3i = Vec3<i32>;
    using Vec3u = Vec3<u32>;
    using Vec3f = Vec3<f32>;
    using Vec3d = Vec3<f64>;
}

template<typename T>
struct std::formatter<cge::Vec3<T>>
{
    template<class ParseContext>
    constexpr ParseContext::iterator parse(ParseContext& ctx) { return ctx.begin(); }

    template<class FmtContext>
    FmtContext::iterator format(cge::Vec3<T> vec, FmtContext& ctx) const
    {
        return std::format_to(ctx.out(), "X: {}, Y: {}, Z: {}", vec.X, vec.Y, vec.Z);
    }
};