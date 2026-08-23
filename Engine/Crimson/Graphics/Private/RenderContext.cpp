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
        // todo CGE_PLATFORM_WINDOWS
#ifdef SDL_PLATFORM_WINDOWS
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_DXIL_BOOLEAN, true);
#endif

        // use metal on apple platforms
#ifdef SDL_PLATFORM_APPLE
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_MSL_BOOLEAN, true);
#endif

#ifndef NDEBUG
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_DEBUGMODE_BOOLEAN, true);
#endif

        CGE_TRACE("Creating device.");
        Device = SDL_CreateGPUDeviceWithProperties(props);
        CGE_SDL_CHECK(Device, "Create device");
    }

    RenderContext::~RenderContext()
    {
        SDL_DestroyGPUDevice(Device);
    }
}
