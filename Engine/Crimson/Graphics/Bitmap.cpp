#include "Bitmap.h"

#include <stb_image.h>

#include "Core/Logger.h"

namespace cge
{
    Bitmap::~Bitmap()
    {
        delete Data;
    }

    Bitmap::Bitmap(const std::string& path)
    {
        int w, h;
        // always load 4 channels as 8-bit RGB is not supported and PixelFormat doesn't support R/RG yet.
        void* data = stbi_load(path.c_str(), &w, &h, nullptr, 4);
        if (!data)
            CGE_FATAL("Failed to load image: {}", stbi_failure_reason());

        Data = static_cast<u8*>(data);
        Size = { static_cast<u32>(w), static_cast<u32>(h) };
        Format = PixelFormat::RGBA8; // stbi always loads 8 bit rgb
    }
}
