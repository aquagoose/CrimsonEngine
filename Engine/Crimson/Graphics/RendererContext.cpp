#include "RendererContext.h"
#include "Core/Logger.h"
#include "Utils/VkUtils.h"

#include <SDL3/SDL_vulkan.h>
#define VMA_IMPLEMENTATION
#include <vk_mem_alloc.h>

#include <vector>
#include <optional>
#include <unordered_set>

#define CGE_VK_API_VERSION VK_API_VERSION_1_3
#define CGE_VK_CLAMP(value, min, max) value < min ? min : value > max ? max : value

namespace cge
{
    void RendererContext::RecreateSwapchain(u32 width, u32 height, VkPresentModeKHR presentMode)
    {
        CGE_VK_CHECK(vkQueueWaitIdle(_presentQueue), "Wait for queue idle");

        VkSwapchainKHR oldSwapchain = _swapchain;

        VkSurfaceCapabilitiesKHR surfaceCapabilities;
        vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_physicalDevice, _surface, &surfaceCapabilities);

        CGE_TRACE("Requesting swapchain size {}x{}", width, height);

        // clamp the width & height to within the supported range.
        VkExtent2D extent
        {
            .width = CGE_VK_CLAMP(width, surfaceCapabilities.minImageExtent.width, surfaceCapabilities.maxImageExtent.width),
            .height = CGE_VK_CLAMP(height, surfaceCapabilities.minImageExtent.height, surfaceCapabilities.maxImageExtent.height)
        };

        CGE_DEBUG("Swapchain size: {}x{}", extent.width, extent.height);
        SwapchainSize = extent;

        u32 imageCount = CGE_VK_CLAMP(2, surfaceCapabilities.minImageCount, surfaceCapabilities.maxImageCount);
        CGE_DEBUG("Image Count: {}", imageCount);

        /*u32 numPresentModes;
        vkGetPhysicalDeviceSurfacePresentModesKHR(_physicalDevice, _surface, &numPresentModes, nullptr);
        std::vector<VkPresentModeKHR> presentModes(numPresentModes);
        vkGetPhysicalDeviceSurfacePresentModesKHR(_physicalDevice, _surface, &numPresentModes, presentModes.data());*/

        VkSwapchainCreateInfoKHR swapchainInfo
        {
            .sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR,
            .surface = _surface,
            .minImageCount = imageCount,
            .imageFormat = VK_FORMAT_B8G8R8A8_UNORM,
            .imageColorSpace = VK_COLOR_SPACE_SRGB_NONLINEAR_KHR,
            .imageExtent = extent,
            .imageArrayLayers = 1,
            .imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT,
            .imageSharingMode = VK_SHARING_MODE_EXCLUSIVE,
            .preTransform = VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR,
            .compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR,
            .presentMode = presentMode, // todo check present mode is supported
            .clipped = false,
            .oldSwapchain = oldSwapchain
        };

        if (_graphicsQueueIndex != _presentQueueIndex)
            CGE_FATAL("Separate graphics and present queues are not yet supported!");

        CGE_TRACE("Creating swapchain.");
        CGE_VK_CHECK(vkCreateSwapchainKHR(_device, &swapchainInfo, nullptr, &_swapchain), "Create swapchain");

        if (oldSwapchain)
        {
            CGE_TRACE("Destroying old swapchain images.");
            for (VkImageView imageView : _swapchainImageViews)
                vkDestroyImageView(_device, imageView, nullptr);

            CGE_TRACE("Destroying old swapchain.")
            vkDestroySwapchainKHR(_device, oldSwapchain, nullptr);
        }

        _swapchainImages.clear();
        _swapchainImageViews.clear();

        u32 numSwapchainImages;
        vkGetSwapchainImagesKHR(_device, _swapchain, &numSwapchainImages, nullptr);
        _swapchainImages.resize(numSwapchainImages);
        vkGetSwapchainImagesKHR(_device, _swapchain, &numSwapchainImages, _swapchainImages.data());

        for (VkImage image : _swapchainImages)
            _swapchainImageViews.push_back(CreateImageView(image, swapchainInfo.imageFormat));
    }

    RendererContext::RendererContext(SDL_Window* window) : _window(window)
    {
        const char* appName = SDL_GetAppMetadataProperty(SDL_PROP_APP_METADATA_NAME_STRING);
        if (!appName)
            appName = "Crimson Application";

        uint32_t currentInstanceVersion;
        CGE_VK_CHECK(vkEnumerateInstanceVersion(&currentInstanceVersion), "Enumerate instance version");
        if (currentInstanceVersion < CGE_VK_API_VERSION)
        {
            CGE_FATAL("Current instance version ({}) is older than the required instance version ({})!",
                      VkUtils::APIVersionToString(currentInstanceVersion),
                      VkUtils::APIVersionToString(CGE_VK_API_VERSION));
        }

        VkApplicationInfo appInfo
        {
            .sType = VK_STRUCTURE_TYPE_APPLICATION_INFO,
            .pApplicationName = appName,
            .applicationVersion = VK_MAKE_VERSION(1, 0, 0),
            .pEngineName = "Crimson",
            .engineVersion = VK_MAKE_VERSION(1, 0, 0),
            .apiVersion = CGE_VK_API_VERSION
        };

        u32 numExtensions;
        char const* const* instanceExtensions = SDL_Vulkan_GetInstanceExtensions(&numExtensions);

        VkInstanceCreateInfo instanceInfo
        {
            .sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
            .pApplicationInfo = &appInfo,
            .enabledExtensionCount = numExtensions,
            .ppEnabledExtensionNames = instanceExtensions
        };

        CGE_TRACE("Creating instance.");
        CGE_VK_CHECK(vkCreateInstance(&instanceInfo, nullptr, &_instance), "Create instance");

        CGE_TRACE("Creating surface.");
        if (!SDL_Vulkan_CreateSurface(window, _instance, nullptr, &_surface))
            CGE_FATAL("Failed to create surface: {}", SDL_GetError());

        CGE_TRACE("Enumerating physical devices.");
        u32 numPhysicalDevices;
        vkEnumeratePhysicalDevices(_instance, &numPhysicalDevices, nullptr);
        std::vector<VkPhysicalDevice> physicalDevices(numPhysicalDevices);
        vkEnumeratePhysicalDevices(_instance, &numPhysicalDevices, physicalDevices.data());

        _physicalDevice = {};
        for (const auto physicalDevice : physicalDevices)
        {
            VkPhysicalDeviceProperties deviceProps;
            vkGetPhysicalDeviceProperties(physicalDevice, &deviceProps);

            if (deviceProps.driverVersion < CGE_VK_API_VERSION)
                continue;

            _physicalDevice = physicalDevice;
            break;
        }

        if (!_physicalDevice)
            CGE_FATAL("No physical devices supporting Vulkan {} found!", VkUtils::APIVersionToString(CGE_VK_API_VERSION));

        VkPhysicalDeviceProperties deviceProps;
        vkGetPhysicalDeviceProperties(_physicalDevice, &deviceProps);

        CGE_INFO("Using device: {} (vendor: {}, type: {})", deviceProps.deviceName, deviceProps.vendorID,
                 VkUtils::PhysicalDeviceTypeToString(deviceProps.deviceType));

        u32 numQueueFamilies;
        vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &numQueueFamilies, nullptr);
        std::vector<VkQueueFamilyProperties> queueFamilies(numQueueFamilies);
        vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &numQueueFamilies, queueFamilies.data());

        std::optional<u32> graphicsQueue;
        std::optional<u32> presentQueue;
        std::optional<u32> computeQueue;

        for (u32 i = 0; i < queueFamilies.size(); i++)
        {
            const VkQueueFamilyProperties& family = queueFamilies[i];
            if (family.queueFlags & VK_QUEUE_GRAPHICS_BIT)
                graphicsQueue = i;
            if (SDL_Vulkan_GetPresentationSupport(_instance, _physicalDevice, i))
                presentQueue = i;
            if (family.queueFlags & VK_QUEUE_COMPUTE_BIT)
                computeQueue = i;

            if (graphicsQueue && presentQueue && computeQueue)
                break;
        }

        // crimson makes heavy use of compute for the forward+ renderer so we list compute as a requirement
        // todo maybe 2D only games can remove the compute requirement?
        if (!graphicsQueue || !presentQueue || !computeQueue)
        {
            CGE_FATAL("One or more required queues were not found! Graphics: {}, Present: {}, Compute: {}",
                      graphicsQueue ? "Found" : "Not Found", presentQueue ? "Found" : "Not Found",
                      computeQueue ? "Found" : "Not Found");
        }

        _graphicsQueueIndex = *graphicsQueue;
        _presentQueueIndex = *presentQueue;
        _computeQueueIndex = *computeQueue;

        std::unordered_set uniqueQueueFamilies { _graphicsQueueIndex, _presentQueueIndex, _computeQueueIndex };
        std::vector<VkDeviceQueueCreateInfo> queueCreateInfos;
        queueCreateInfos.reserve(uniqueQueueFamilies.size());
        f32 queuePriority = 1.0f;
        for (u32 family : uniqueQueueFamilies)
            queueCreateInfos.emplace_back(VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO, nullptr, 0, family, 1, &queuePriority);

        VkPhysicalDeviceFeatures enabledFeatures{};

        VkPhysicalDeviceDynamicRenderingFeatures dynamicRendering
        {
            .sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_DYNAMIC_RENDERING_FEATURES,
            .dynamicRendering = true
        };

        std::vector deviceExtensions { VK_KHR_SWAPCHAIN_EXTENSION_NAME };

        VkDeviceCreateInfo deviceInfo
        {
            .sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO,
            .pNext = &dynamicRendering,
            .queueCreateInfoCount = static_cast<u32>(queueCreateInfos.size()),
            .pQueueCreateInfos = queueCreateInfos.data(),
            .enabledExtensionCount = static_cast<u32>(deviceExtensions.size()),
            .ppEnabledExtensionNames = deviceExtensions.data(),
            .pEnabledFeatures = &enabledFeatures
        };

        CGE_TRACE("Creating device.");
        CGE_VK_CHECK(vkCreateDevice(_physicalDevice, &deviceInfo, nullptr, &_device), "Create device");

        CGE_TRACE("Getting device queues.");
        vkGetDeviceQueue(_device, _graphicsQueueIndex, 0, &_graphicsQueue);
        vkGetDeviceQueue(_device, _presentQueueIndex, 0, &_presentQueue);
        vkGetDeviceQueue(_device, _computeQueueIndex, 0, &_computeQueue);

        VmaAllocatorCreateInfo allocatorInfo
        {
            .physicalDevice = _physicalDevice,
            .device = _device,
            .instance = _instance,
            .vulkanApiVersion = CGE_VK_API_VERSION,
        };

        CGE_TRACE("Creating VMA allocator.");
        CGE_VK_CHECK(vmaCreateAllocator(&allocatorInfo, &_allocator), "Create VMA allocator");

        VkCommandPoolCreateInfo poolInfo
        {
            .sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,
            .flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT,
            .queueFamilyIndex = _graphicsQueueIndex
        };

        CGE_TRACE("Creating command pool.");
        CGE_VK_CHECK(vkCreateCommandPool(_device, &poolInfo, nullptr, &_commandPool), "Create command pool");

        int width, height;
        SDL_GetWindowSizeInPixels(window, &width, &height);
        RecreateSwapchain(static_cast<u32>(width), static_cast<u32>(height), VK_PRESENT_MODE_FIFO_KHR);

        VkFenceCreateInfo fenceInfo
        {
            .sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO
        };

        CGE_TRACE("Creating image available fence.")
        CGE_VK_CHECK(vkCreateFence(_device, &fenceInfo, nullptr, &_imageAvailableFence), "Create image available fence");
    }

    RendererContext::~RendererContext()
    {
        vkDeviceWaitIdle(_device);

        CGE_TRACE("Destroying image available fence.");
        vkDestroyFence(_device, _imageAvailableFence, nullptr);

        CGE_TRACE("Destroying swapchain images.");
        for (VkImageView view : _swapchainImageViews)
            vkDestroyImageView(_device, view, nullptr);

        CGE_TRACE("Destroying swapchain.");
        vkDestroySwapchainKHR(_device, _swapchain, nullptr);

        CGE_TRACE("Destroying fences.");
        for (const auto& [_, fence] : _submittedCommandBuffers)
            vkDestroyFence(_device, fence, nullptr);

        while (!_availableFences.empty())
        {
            vkDestroyFence(_device, _availableFences.front(), nullptr);
            _availableFences.pop();
        }

        CGE_TRACE("Destroying command pool.");
        vkDestroyCommandPool(_device, _commandPool, nullptr);

        CGE_TRACE("Destroying VMA allocator.");
        vmaDestroyAllocator(_allocator);

        CGE_TRACE("Destroying device.");
        vkDestroyDevice(_device, nullptr);

        CGE_TRACE("Destroying surface.");
        SDL_Vulkan_DestroySurface(_instance, _surface, nullptr);

        CGE_TRACE("Destroying instance.");
        vkDestroyInstance(_instance, nullptr);
    }

    VkImageView RendererContext::CreateImageView(VkImage image, VkFormat format) const
    {
        VkImageViewCreateInfo viewInfo
        {
            .sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
            .image = image,
            .viewType = VK_IMAGE_VIEW_TYPE_2D,
            .format = format,
            .components =
            {
                .r = VK_COMPONENT_SWIZZLE_IDENTITY,
                .g = VK_COMPONENT_SWIZZLE_IDENTITY,
                .b = VK_COMPONENT_SWIZZLE_IDENTITY,
                .a = VK_COMPONENT_SWIZZLE_IDENTITY
            },
            .subresourceRange =
            {
                .aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
                .baseMipLevel = 0,
                .levelCount = 1,
                .baseArrayLayer = 0,
                .layerCount = 1
            }
        };

        CGE_TRACE("Creating image view.");
        VkImageView view;
        CGE_VK_CHECK(vkCreateImageView(_device, &viewInfo, nullptr, &view), "Create image view");

        return view;
    }

    VulkanBuffer RendererContext::CreateBuffer(u32 usageFlags, u32 size, bool dynamic)
    {
        u32 vmaFlags = 0;

        // non-dynamic buffers must allow data to be uploaded from a transfer buffer, so automatically set that flag
        // however if the buffer IS a transfer buffer, then we don't need to set the flag.
        if (dynamic || (usageFlags & VK_BUFFER_USAGE_TRANSFER_SRC_BIT))
            vmaFlags |= VMA_ALLOCATION_CREATE_HOST_ACCESS_SEQUENTIAL_WRITE_BIT;
        else
            usageFlags |= VK_BUFFER_USAGE_TRANSFER_DST_BIT;

        VkBufferCreateInfo bufferInfo
        {
            .sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO,
            .size = size,
            .usage = usageFlags
        };

        VmaAllocationCreateInfo allocInfo
        {
            .usage = VMA_MEMORY_USAGE_AUTO,
        };

        CGE_TRACE("Creating {}KiB buffer.", size / 1024);
        VulkanBuffer buffer{};
        CGE_VK_CHECK(vmaCreateBuffer(_allocator, &bufferInfo, &allocInfo, &buffer.Buffer, &buffer.Allocation, nullptr), "Create buffer");

        return buffer;
    }

    void RendererContext::DestroyBuffer(VulkanBuffer& buffer)
    {
        vmaDestroyBuffer(_allocator, buffer.Buffer, buffer.Allocation);
        buffer.Buffer = VK_NULL_HANDLE;
        buffer.Allocation = VMA_NULL;
    }

    VkCommandBuffer RendererContext::GetCommandBuffer()
    {
        VkCommandBuffer cb;
        // return an available command buffer if there is one.
        if (!_availableCommandBuffers.empty())
        {
            cb = _availableCommandBuffers.front();
            _availableCommandBuffers.pop();
        }
        else
        {
            // otherwise allocate a new command buffer and return it.
            VkCommandBufferAllocateInfo allocInfo
            {
                .sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
                .commandPool = _commandPool,
                .level = VK_COMMAND_BUFFER_LEVEL_PRIMARY,
                .commandBufferCount = 1
            };

            CGE_TRACE("Allocating new command buffer.");
            CGE_VK_CHECK(vkAllocateCommandBuffers(_device, &allocInfo, &cb), "Allocate command buffer");
        }

        VkCommandBufferBeginInfo beginInfo
        {
            .sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
            .flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT,
        };

        CGE_VK_CHECK(vkBeginCommandBuffer(cb, &beginInfo), "Begin command buffer");

        return cb;
    }

    void RendererContext::SubmitCommandBuffer(VkCommandBuffer cb)
    {
        CGE_VK_CHECK(vkEndCommandBuffer(cb), "End command buffer");

        VkSubmitInfo submitInfo
        {
            .sType = VK_STRUCTURE_TYPE_SUBMIT_INFO,
            .commandBufferCount = 1,
            .pCommandBuffers = &cb,
        };

        VkFence fence;
        if (!_availableFences.empty())
        {
            fence = _availableFences.front();
            _availableFences.pop();
        }
        else
        {
            VkFenceCreateInfo fenceInfo
            {
                .sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO
            };

            CGE_TRACE("Creating fence.");
            CGE_VK_CHECK(vkCreateFence(_device, &fenceInfo, nullptr, &fence), "Create fence");
        }

        CGE_VK_CHECK(vkQueueSubmit(_graphicsQueue, 1, &submitInfo, fence), "Submit queue");

        _submittedCommandBuffers.emplace_back(cb, fence);

        for (size_t i = 0; i < _submittedCommandBuffers.size(); i++)
        {
            const auto& [submittedBuffer, submittedFence] = _submittedCommandBuffers[i];

            if (vkGetFenceStatus(_device, submittedFence) != VK_SUCCESS)
                continue;

            CGE_VK_CHECK(vkResetFences(_device, 1, &submittedFence), "Reset fences");
            _availableCommandBuffers.push(submittedBuffer);
            _availableFences.push(submittedFence);

            std::erase(_submittedCommandBuffers, _submittedCommandBuffers[i]);
            i--;
        }
    }

    VkImageView RendererContext::GetNextSwapchainImage(VkCommandBuffer cb)
    {
        VkResult result = vkAcquireNextImageKHR(_device, _swapchain, UINT64_MAX, VK_NULL_HANDLE, _imageAvailableFence, &_currentImage);
        switch (result)
        {
            case VK_SUCCESS: break;
            // recreate swapchain if needed
            case VK_SUBOPTIMAL_KHR:
            {
                int w, h;
                SDL_GetWindowSizeInPixels(_window, &w, &h);
                RecreateSwapchain(static_cast<u32>(w), static_cast<u32>(h), VK_PRESENT_MODE_FIFO_KHR);
                break;
            }
            default:
                CGE_VK_CHECK(result, "Acquire next image");
                break;
        }

        vkWaitForFences(_device, 1, &_imageAvailableFence, VK_TRUE, UINT64_MAX);
        vkResetFences(_device, 1, &_imageAvailableFence);

        VkImageMemoryBarrier barrier
        {
            .sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
            .srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_READ_BIT,
            .dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT,
            .oldLayout = VK_IMAGE_LAYOUT_UNDEFINED,
            .newLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL,
            .image = _swapchainImages[_currentImage],
            .subresourceRange =
            {
                .aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
                .baseMipLevel = 0,
                .levelCount = 1,
                .baseArrayLayer = 0,
                .layerCount = 1
            }
        };

        vkCmdPipelineBarrier(cb, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT,
                             VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, 0, 0, nullptr, 0, nullptr, 1, &barrier);

        return _swapchainImageViews[_currentImage];
    }

    void RendererContext::SubmitAndPresent(VkCommandBuffer cb)
    {
        VkImageMemoryBarrier barrier
        {
            .sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
            .srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT,
            .dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_READ_BIT,
            .oldLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL,
            .newLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR,
            .image = _swapchainImages[_currentImage],
            .subresourceRange =
            {
                .aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
                .baseMipLevel = 0,
                .levelCount = 1,
                .baseArrayLayer = 0,
                .layerCount = 1
            }
        };

        vkCmdPipelineBarrier(cb, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT,
                             VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, 0, 0, nullptr, 0, nullptr, 1, &barrier);

        SubmitCommandBuffer(cb);

        VkPresentInfoKHR presentInfo
        {
            .sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR,
            .swapchainCount = 1,
            .pSwapchains = &_swapchain,
            .pImageIndices = &_currentImage,
        };

        VkResult result = vkQueuePresentKHR(_graphicsQueue, &presentInfo);
        switch (result)
        {
            case VK_SUCCESS: break;
            // recreate swapchain if needed
            case VK_SUBOPTIMAL_KHR:
            {
                int w, h;
                SDL_GetWindowSizeInPixels(_window, &w, &h);
                RecreateSwapchain(w, h, VK_PRESENT_MODE_FIFO_KHR);
                break;
            }
            default:
                CGE_VK_CHECK(result, "Present");
                break;
        }
    }

    void RendererContext::BeginRenderPass(VkCommandBuffer cb, std::span<VkRenderingAttachmentInfo> colorAttachments)
    {
        VkRenderingInfo renderingInfo
        {
            .sType = VK_STRUCTURE_TYPE_RENDERING_INFO,
            .renderArea = { .extent = SwapchainSize },
            .layerCount = 1,
            .colorAttachmentCount = static_cast<u32>(colorAttachments.size()),
            .pColorAttachments = colorAttachments.data(),
        };

        vkCmdBeginRendering(cb, &renderingInfo);
    }

    void RendererContext::EndRenderPass(VkCommandBuffer cb)
    {
        vkCmdEndRendering(cb);
    }
}
