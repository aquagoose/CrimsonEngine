#include "RenderContext.h"

#include "SDLUtils.h"

#include "Core/Logger.h"
#include "Core/BitUtils.h"
#include "Core/File.h"
#include "Core/Path.h"

#include <cstring>
#include <SDL3_shadercross/SDL_shadercross.h>

namespace cge::Private
{
    RenderContext::RenderContext(SDL_Window* window) : Window(window)
    {
        SDL_PropertiesID props = SDL_CreateProperties();

#if defined(CGE_PLATFORM_WINDOWS) // use d3d12 on windows
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_DXIL_BOOLEAN, true);
#elif defined(CGE_PLATFORM_APPLE) // use metal on apple platforms
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_MSL_BOOLEAN, true);
#else // use vulkan on everything else
        SDL_SetBooleanProperty(props, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_SPIRV_BOOLEAN, true);
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

        SDL_ShaderCross_Init();

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

    SDL_GPUShader* RenderContext::CreateShader(ShaderStage stage, const std::string& name, const std::string& entryPoint, const ShaderInfo& info)
    {
        /*SDL_GPUShaderFormat format = SDL_GetGPUShaderFormats(Device);
        if ((format & SDL_GPU_SHADERFORMAT_SPIRV) != 0)
            format = SDL_GPU_SHADERFORMAT_SPIRV;
        else if ((format & SDL_GPU_SHADERFORMAT_DXIL) != 0)
            format = SDL_GPU_SHADERFORMAT_DXIL;
        else if ((format & SDL_GPU_SHADERFORMAT_MSL) != 0)
            format = SDL_GPU_SHADERFORMAT_MSL;

        std::string fileExtension;
        switch (format)
        {
            case SDL_GPU_SHADERFORMAT_SPIRV:
                fileExtension = "spv";
                break;
            case SDL_GPU_SHADERFORMAT_DXIL:
                fileExtension = "dxil";
                break;
            case SDL_GPU_SHADERFORMAT_MSL:
                fileExtension = "metal";
                break;
            default:
                CGE_FATAL("Unrecognized shader format {}", format);
        }

        auto fullPath = Path::Combine(CGE_CONTENT_DIR, "Shaders", std::format("{}.{}", name, fileExtension));
        auto data = File::ReadBytes(fullPath);

        SDL_GPUShaderStage sdlStage;
        switch (stage)
        {
            case ShaderStage::Vertex:
                sdlStage = SDL_GPU_SHADERSTAGE_VERTEX;
                break;
            case ShaderStage::Pixel:
                sdlStage = SDL_GPU_SHADERSTAGE_FRAGMENT;
                break;
        }

        SDL_GPUShaderCreateInfo shaderInfo
        {
            .code_size = data.size(),
            .code = data.data(),
            .entrypoint = entryPoint.c_str(),
            .format = format,
            .stage = sdlStage,
            .num_samplers = info.NumSamplers,
            .num_storage_textures = info.NumStorageTextures,
            .num_storage_buffers = info.NumStorageBuffers,
            .num_uniform_buffers = info.NumUniforms
        };

        CGE_TRACE("Creating shader.");
        SDL_GPUShader* shader = SDL_CreateGPUShader(Device, &shaderInfo);
        CGE_SDL_CHECK(shader, "Create shader");

        return shader;*/

        SDL_ShaderCross_ShaderStage shaderStage;
        std::string tag;
        switch (stage)
        {
            case ShaderStage::Vertex:
                shaderStage = SDL_SHADERCROSS_SHADERSTAGE_VERTEX;
                tag = "v";
                break;
            case ShaderStage::Pixel:
                shaderStage = SDL_SHADERCROSS_SHADERSTAGE_FRAGMENT;
                tag = "p";
                break;
        }

        auto fullPath = Path::Combine(CGE_CONTENT_DIR, "Shaders", std::format("{}_{}.spv", name, tag));
        auto data = File::ReadBytes(fullPath);

        SDL_ShaderCross_SPIRV_Info spirvInfo
        {
            .bytecode = data.data(),
            .bytecode_size = data.size(),
            .entrypoint = entryPoint.c_str(),
            .shader_stage = shaderStage,
        };

        SDL_ShaderCross_GraphicsShaderResourceInfo resources
        {
            .num_samplers = info.NumSamplers,
            .num_storage_textures = info.NumStorageTextures,
            .num_storage_buffers = info.NumStorageBuffers,
            .num_uniform_buffers = info.NumUniforms
        };

        CGE_TRACE("Compiling shader.");
        SDL_GPUShader* shader = SDL_ShaderCross_CompileGraphicsShaderFromSPIRV(Device, &spirvInfo, &resources, 0);
        CGE_SDL_CHECK(shader, "Compile shader from SPIR-V");
        return shader;
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

    SDL_GPUBuffer* RenderContext::CreateBuffer(SDL_GPUBufferUsageFlags usage, u32 size) const
    {
        SDL_GPUBufferCreateInfo bufferInfo
        {
            .usage = usage,
            .size = size
        };

        CGE_TRACE("Creating {}KiB buffer", size / 1024);
        SDL_GPUBuffer* buffer = SDL_CreateGPUBuffer(Device, &bufferInfo);
        CGE_SDL_CHECK(buffer, "Create buffer");

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
