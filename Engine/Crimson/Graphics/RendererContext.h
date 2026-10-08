#pragma once

#include "Core/CoreTypes.h"

#include <vulkan/vulkan.h>
#include <SDL3/SDL.h>

namespace cge
{
    class RendererContext
    {
        VkInstance _instance;
        VkSurfaceKHR _surface;
        VkPhysicalDevice _physicalDevice;
        VkDevice _device;

        u32 _graphicsQueueIndex;
        u32 _presentQueueIndex;
        u32 _computeQueueIndex;

    public:
        explicit RendererContext(SDL_Window* window);
        ~RendererContext();
    };
}
