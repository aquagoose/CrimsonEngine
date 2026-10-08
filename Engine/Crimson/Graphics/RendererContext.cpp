#include "RendererContext.h"
#include "Core/Logger.h"
#include "Utils/VkUtils.h"

#include <SDL3/SDL_vulkan.h>

#include <vector>
#include <optional>
#include <unordered_set>

#define CGE_VK_API_VERSION VK_API_VERSION_1_3

namespace cge
{
    RendererContext::RendererContext(SDL_Window* window)
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
    }

    RendererContext::~RendererContext()
    {
        vkDeviceWaitIdle(_device);
        CGE_TRACE("Destroying device.");
        vkDestroyDevice(_device, nullptr);

        CGE_TRACE("Destroying surface.");
        SDL_Vulkan_DestroySurface(_instance, _surface, nullptr);

        CGE_TRACE("Destroying instance.");
        vkDestroyInstance(_instance, nullptr);
    }
}
