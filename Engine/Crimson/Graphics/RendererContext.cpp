#include "RendererContext.h"

#include <SDL3/SDL_vulkan.h>

#include "Core/Logger.h"
#include "Utils/VkUtils.h"

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
    }

    RendererContext::~RendererContext()
    {
        CGE_TRACE("Destroying instance.");
        vkDestroyInstance(_instance, nullptr);
    }
}
