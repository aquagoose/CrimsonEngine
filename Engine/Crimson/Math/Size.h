#pragma once

#include "Coredefs.h"

namespace cge
{
    /**
     * A 2-dimensional size with a Width and Height.
     * @tparam T A numeric type.
     */
    template<typename T>
    struct Size
    {
        /**
         * The width.
         */
        const T Width;

        /**
         * The height.
         */
        const T Height;

        /**
         * Construct a size with a width and height.
         * @param width The width.
         * @param height The height.
         */
        Size(T width, T height) : Width(width), Height(height) {}

        /**
         * Construct a size with a scalar value.
         * @param wh The value to apply to both the width and height.
         */
        explicit Size(T wh) : Width(wh), Height(wh) {}

        friend bool operator ==(const Size& lhs, const Size& rhs)
        {
            return lhs.Width == rhs.Width && lhs.Height == rhs.Height;
        }

        friend bool operator !=(const Size& lhs, const Size& rhs)
        {
            return lhs.Width != rhs.Width || lhs.Height != rhs.Height;
        }
    };

    using Sizes = Size<i8>;
    using Sizeb = Size<u8>;
    using Sizei = Size<i32>;
    using Sizeu = Size<u32>;
    using Sizef = Size<f32>;
    using Sized = Size<f64>;
}