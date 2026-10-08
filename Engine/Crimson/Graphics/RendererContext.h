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
        VkSwapchainKHR _swapchain{};

        u32 _graphicsQueueIndex;
        VkQueue _graphicsQueue;
        u32 _presentQueueIndex;
        VkQueue _presentQueue;
        u32 _computeQueueIndex;
        VkQueue _computeQueue;

        void RecreateSwapchain(u32 width, u32 height, VkPresentModeKHR presentMode);

    public:
        explicit RendererContext(SDL_Window* window);
        ~RendererContext();
    };
}
