#pragma once

#include "Core/CoreTypes.h"

#include <vulkan/vulkan.h>
#include <vk_mem_alloc.h>
#include <SDL3/SDL.h>

#include <vector>
#include <queue>
#include <span>
#include <tuple>

namespace cge
{
    struct VulkanBuffer
    {
        VkBuffer Buffer;
        VmaAllocation Allocation;
    };

    class RendererContext
    {
        SDL_Window* _window;

        VkInstance _instance;
        VkSurfaceKHR _surface;
        VkPhysicalDevice _physicalDevice;
        VkDevice _device;
        VmaAllocator _allocator;

        VkSwapchainKHR _swapchain{};
        std::vector<VkImage> _swapchainImages;
        std::vector<VkImageView> _swapchainImageViews;
        VkFence _imageAvailableFence;
        u32 _currentImage;

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

    public:
        VkExtent2D SwapchainSize;

        RendererContext(const RendererContext&) = delete;
        RendererContext& operator =(const RendererContext&) = delete;

        explicit RendererContext(SDL_Window* window);
        ~RendererContext();

        void RecreateSwapchain(u32 width, u32 height, VkPresentModeKHR presentMode);

        VkImageView CreateImageView(VkImage image, VkFormat format) const;

        VulkanBuffer CreateBuffer(u32 usageFlags, u32 size, bool dynamic = false);
        void DestroyBuffer(VulkanBuffer& buffer);

        VkCommandBuffer GetCommandBuffer();
        void SubmitCommandBuffer(VkCommandBuffer cb);

        VkImageView GetNextSwapchainImage(VkCommandBuffer cb);
        void SubmitAndPresent(VkCommandBuffer cb);

        void BeginRenderPass(VkCommandBuffer cb, std::span<VkRenderingAttachmentInfo> colorAttachments);
        void EndRenderPass(VkCommandBuffer cb);
    };
}
