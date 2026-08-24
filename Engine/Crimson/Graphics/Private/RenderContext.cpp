#include "RenderContext.h"
#include "SDLUtils.h"

#include <Core/Logger.h>

namespace cge::Private
{
    RenderContext::RenderContext(SDL_Window* window) : Window(window)
    {
        SDL_PropertiesID props = SDL_CreateProperties();
        // always enable vulkan
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_SPIRV_BOOLEAN, true);

        // use d3d12 on windows
#ifdef CGE_PLATFORM_WINDOWS
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_DXIL_BOOLEAN, true);
#endif

        // use metal on apple platforms
#ifdef CGE_PLATFORM_APPLE
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_MSL_BOOLEAN, true);
#endif

#ifndef NDEBUG
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_DEBUGMODE_BOOLEAN, true);
#endif

        CGE_TRACE("Creating device.");
        Device = SDL_CreateGPUDeviceWithProperties(props);
        CGE_SDL_CHECK(Device, "Create device");
        SDL_DestroyProperties(props);

        SDL_PropertiesID deviceProps = SDL_GetGPUDeviceProperties(Device);
        CGE_INFO("Backend: {}", SDL_GetGPUDeviceDriver(Device));
        CGE_INFO("Device: {}", SDL_GetStringProperty(deviceProps, SDL_PROP_GPU_DEVICE_NAME_STRING, "unknown"));
        CGE_INFO("Driver: {}", SDL_GetStringProperty(deviceProps, SDL_PROP_GPU_DEVICE_DRIVER_INFO_STRING, "unknown"));
        SDL_DestroyProperties(deviceProps);

        CGE_TRACE("Associating device with window.");
        CGE_SDL_CHECK(SDL_ClaimWindowForGPUDevice(Device, Window), "Claim window for device");
    }

    RenderContext::~RenderContext()
    {
        SDL_ReleaseWindowFromGPUDevice(Device, Window);
        SDL_DestroyGPUDevice(Device);
    }
}
