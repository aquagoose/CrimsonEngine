#include "../../include/Crimson/Graphics/Private/RenderContext.h"

#include "SDLUtils.h"
#include "Crimson/Core/Logger.h"

namespace cge::pvt
{
    RenderContext::RenderContext(SDL_Window* window) : Window(window)
    {
        SDL_PropertiesID createProps = SDL_CreateProperties();

#if defined(SDL_PLATFORM_WINDOWS) // use d3d12 on windows
        SDL_SetBooleanProperty(createProps, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_DXIL_BOOLEAN, true);
#elif defined(SDL_PLATFORM_APPLE) // use metal on apple
        SDL_SetBooleanProperty(createProps, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_MSL_BOOLEAN, true);
#else // use vulkan on everything else
        SDL_SetBooleanProperty(createProps, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_SPIRV_BOOLEAN, true);
#endif

#ifndef NDEBUG // enable debug when running in debug
        SDL_SetBooleanProperty(createProps, SDL_PROP_GPU_DEVICE_CREATE_DEBUGMODE_BOOLEAN, true);
#endif

        CGE_TRACE("Creating device.");
        Device = SDL_CreateGPUDeviceWithProperties(createProps);
        SDL_CHECK(Device, "Create device");
        SDL_DestroyProperties(createProps);

        SDL_PropertiesID deviceProps = SDL_GetGPUDeviceProperties(Device);
        CGE_INFO("Backend: {}", SDL_GetGPUDeviceDriver(Device));
        CGE_INFO("Device: {}", SDL_GetStringProperty(deviceProps, SDL_PROP_GPU_DEVICE_NAME_STRING, "unknown"));
        CGE_INFO("Driver: {}", SDL_GetStringProperty(deviceProps, SDL_PROP_GPU_DEVICE_DRIVER_INFO_STRING, "unknown"));
        SDL_DestroyProperties(deviceProps);

        CGE_TRACE("Claiming window for device.")
        SDL_CHECK(SDL_ClaimWindowForGPUDevice(Device, Window), "Claim window for device");
    }

    RenderContext::~RenderContext()
    {
        SDL_ReleaseWindowFromGPUDevice(Device, Window);
        SDL_DestroyGPUDevice(Device);
    }
}
