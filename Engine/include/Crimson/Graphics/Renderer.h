#pragma once

#include <memory>

namespace cge
{
    class RenderContext;

    class Renderer final
    {
        std::unique_ptr<RenderContext> _context;
    };
}