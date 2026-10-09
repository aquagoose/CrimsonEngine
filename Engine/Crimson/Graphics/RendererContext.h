#pragma once

#include "Core/CoreTypes.h"

#include <vulkan/vulkan.h>
#include <SDL3/SDL.h>

#include <vector>
#include <queue>
#include <tuple>

namespace cge
{
    class RendererContext
    {
        VkInstance _instance;
        VkSurfaceKHR _surface;
        VkPhysicalDevice _physicalDevice;
        VkDevice _device;
        VkSwapchainKHR _swapchain{};
        std::vector<VkImage> _swapchainImages;
        std::vector<VkImageView> _swapchainImageViews;

        u32 _graphicsQueueIndex;
        VkQueue _graphicsQueue;
        u32 _presentQueueIndex;
        VkQueue _presentQueue;
        u32 _computeQueueIndex;
        VkQueue _computeQueue;

        VkCommandPool _commandPool;
        std::queue<VkCommandBuffer> _availableCommandBuffers;
        std::queue<VkFence> _availableFences;
        std::vector<std::tuple<VkCommandBuffer, VkFence>> _submittedCommandBuffers;

        void RecreateSwapchain(u32 width, u32 height, VkPresentModeKHR presentMode);

    public:
        explicit RendererContext(SDL_Window* window);
        ~RendererContext();

        VkImageView CreateImageView(VkImage image, VkFormat format) const;

        VkCommandBuffer AcquireCommandBuffer();
        void SubmitCommandBuffer(VkCommandBuffer cb);
    };
}
