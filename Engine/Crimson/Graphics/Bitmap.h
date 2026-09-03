#pragma once
#include "Math/Size.h"

#include <string>

namespace cge
{
    /**
     * Defines various supported pixel formats.
     */
    enum class PixelFormat
    {
        /**
         * 8-bit RGBA, 32bpp.
         */
        RGBA8
    };

    struct Bitmap
    {
        /**
         * The pixel data, in the format denoted by @ref Format
         */
        u8* Data;

        /**
         * The size in pixels.
         */
        Sizeu Size;

        /**
         * The pixel format that the image data is in.
         */
        PixelFormat Format;

        ~Bitmap();

        /**
         * Load a bitmap from an image path.
         * @param path The path to the image.
         */
        explicit Bitmap(const std::string& path);
    };
}
