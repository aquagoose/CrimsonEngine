#include "RenderContext.h"

#include "SDLUtils.h"

#include "Core/Logger.h"
#include "Core/BitUtils.h"

#include <cstring>

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

        _transferBufferSize = TransferBufferInitialSize;
        _transferBufferOffset = 0;
        _transferBuffer = CreateTransferBuffer(SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD, _transferBufferSize);
    }

    RenderContext::~RenderContext()
    {
        SDL_ReleaseGPUTransferBuffer(Device, _transferBuffer);
        SDL_ReleaseWindowFromGPUDevice(Device, Window);
        SDL_DestroyGPUDevice(Device);
    }

    SDL_GPUTransferBuffer* RenderContext::CreateTransferBuffer(SDL_GPUTransferBufferUsage usage, u32 size) const
    {
        SDL_GPUTransferBufferCreateInfo bufferInfo
        {
            .usage = usage,
            .size = size
        };

        CGE_TRACE("Creating {}KiB transfer buffer.", size / 1024);
        SDL_GPUTransferBuffer* buffer = SDL_CreateGPUTransferBuffer(Device, &bufferInfo);
        CGE_SDL_CHECK(buffer, "Create transfer buffer");

        return buffer;
    }

    SDL_GPUTransferBuffer* RenderContext::GetUploadBuffer(u32 size, u32& offset, bool& shouldCycle)
    {
        if (size >= _transferBufferSize)
        {
            CGE_DEBUG("Requested size ({}KiB) is larger than the current transfer buffer ({}KiB)! A new one will be created.", size / 1024, _transferBufferSize / 1024);
            _transferBufferSize = BitUtils::RoundToNearestPowerOf2(size);
            _transferBuffer = CreateTransferBuffer(SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD, _transferBufferSize);

            _transferBufferOffset = 0;
        }

        if (size + _transferBufferOffset >= _transferBufferSize)
        {
            shouldCycle = true;
            _transferBufferOffset = 0;
        }

        offset = _transferBufferOffset;
        _transferBufferOffset += size;

        return _transferBuffer;
    }

    void RenderContext::CopyDataToTexture(SDL_GPUTexture* texture, void* data, const Vec2u& pos, const Sizeu& size, SDL_GPUTextureFormat format)
    {
        // multiply by 4 as RGBA8 is the only supported texture format right now
        // todo calculate the size of the pixel format once more pixelformats are supported
        u32 dataSize = size.Width * size.Height * 4;

        u32 offset;
        bool shouldCycle;
        SDL_GPUTransferBuffer* transBuffer = GetUploadBuffer(dataSize, offset, shouldCycle);

        CGE_TRACE("Transferring {}KiB of data to texture {} (offset: {}, cycle: {})",
            dataSize / 1024, reinterpret_cast<usize>(texture), offset, shouldCycle);

        void* mapped = SDL_MapGPUTransferBuffer(Device, transBuffer, shouldCycle);
        CGE_SDL_CHECK(mapped, "Map buffer");
        std::memcpy(static_cast<u8*>(mapped) + offset, data, dataSize);
        SDL_UnmapGPUTransferBuffer(Device, _transferBuffer);

        SDL_GPUCommandBuffer* cb = SDL_AcquireGPUCommandBuffer(Device);
        CGE_SDL_CHECK(cb, "Acquire command buffer");

        SDL_GPUCopyPass* pass = SDL_BeginGPUCopyPass(cb);
        CGE_SDL_CHECK(pass, "Begin copy pass");

        SDL_GPUTextureTransferInfo src
        {
            .transfer_buffer = transBuffer,
            .offset = offset,
            .pixels_per_row = size.Width,
            .rows_per_layer = size.Height
        };

        SDL_GPUTextureRegion dest
        {
            .texture = texture,
            .mip_level = 0,
            .layer = 0,
            .x = pos.X,
            .y = pos.Y,
            .z = 0,
            .w = size.Width,
            .h = size.Height,
            .d = 1
        };

        SDL_UploadToGPUTexture(pass, &src, &dest, false);

        SDL_EndGPUCopyPass(pass);
        CGE_SDL_CHECK(SDL_SubmitGPUCommandBuffer(cb), "Submit command buffer");
    }
}
