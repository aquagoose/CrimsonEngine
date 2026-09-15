#pragma once

#include "Math/Coredefs.h"
#include "Math/Size.h"
#include "Math/Vec2.h"

#include <SDL3/SDL.h>

#include <unordered_set>

namespace cge::Private
{
    enum class ShaderStage
    {
        Vertex,
        Pixel,
    };

    struct ShaderInfo
    {
        u32 NumSamplers;
        u32 NumUniforms;
        u32 NumStorageTextures;
        u32 NumStorageBuffers;
    };

    /**
     * The core renderer context, containing the core objects for the renderer.
     */
    class RenderContext final
    {
        // 32MiB initial transfer buffer size
        static constexpr u32 TransferBufferInitialSize = 32 * 1024 * 1024;

        u32 _transferBufferSize;
        u32 _transferBufferOffset;
        SDL_GPUTransferBuffer* _transferBuffer;

    public:
        SDL_Window* Window;
        SDL_GPUDevice* Device;

        std::unordered_set<SDL_GPUTexture*> MipmapQueue;

        explicit RenderContext(SDL_Window* window);
        ~RenderContext();

        [[nodiscard]] SDL_GPUShader* CreateShader(ShaderStage stage, const std::string& name, const std::string& entryPoint, const ShaderInfo& info);
        [[nodiscard]] SDL_GPUTransferBuffer* CreateTransferBuffer(SDL_GPUTransferBufferUsage usage, u32 size) const;
        [[nodiscard]] SDL_GPUBuffer* CreateBuffer(SDL_GPUBufferUsageFlags usage, u32 size) const;

        [[nodiscard]] SDL_GPUTransferBuffer* GetUploadBuffer(u32 size, u32& offset, bool& shouldCycle);

        void CopyDataToTexture(SDL_GPUTexture* texture, void* data, const Vec2u& pos, const Sizeu& size, SDL_GPUTextureFormat format);
    };
}
