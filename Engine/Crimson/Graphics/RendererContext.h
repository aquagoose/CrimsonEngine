#pragma once

#include <vulkan/vulkan.h>
#include <SDL3/SDL.h>

namespace cge
{
    class RendererContext
    {
        VkInstance _instance;
        VkPhysicalDevice _physicalDevice;
        VkDevice _device;

    public:
        explicit RendererContext(SDL_Window* window);
        ~RendererContext();
    };
}
