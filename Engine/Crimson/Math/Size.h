#pragma once

#include <format>
#include <string>

namespace cge
{
    /**
     * Represents a 2-dimensional size with a Width and Height.
     * @tparam T A numeric type.
     */
    template<typename T>
    struct Size
    {
        /**
         * The width.
         */
        T Width;

        /**
         * The height.
         */
        T Height;

        /**
         * Construct a Size from a width and height.
         * @param width The width.
         * @param height The height.
         */
        Size(T width, T height)
        {
            Width = width;
            Height = height;
        }

        /**
         * Construct a Size from a scalar value.
         * @param wh The scalar value to apply to the width and height.
         */
        explicit Size(T wh)
        {
            Width = wh;
            Height = wh;
        }

        /**
         * Construct an empty Size.
         */
        Size()
        {
            Width = 0;
            Height = 0;
        }

        /**
         * Get this size as a string.
         * @return {width}x{height}
         */
        [[nodiscard]] std::string ToString() const
        {
            return std::format("{}x{}", Width, Height);
        }
    };
}