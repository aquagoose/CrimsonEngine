#include "Renderer.h"

#include "Private/SDLUtils.h"

namespace cge
{
    Renderer::Renderer(SDL_Window* window)
    {
        _context = std::make_unique<Private::RenderContext>(window);
    }

    Renderer::~Renderer()
    {
        SDL_WaitForGPUIdle(_context->Device);
    }

    std::unique_ptr<Texture> Renderer::CreateTexture(void* data, const Sizeu& size, PixelFormat format, bool generateMips) const
    {
        SDL_GPUTextureFormat texFormat;
        switch (format)
        {
            case PixelFormat::RGBA8:
                texFormat = SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM;
                break;
            default:
                CGE_FATAL("Invalid pixel format!");
        }

        SDL_GPUTextureUsageFlags usage = SDL_GPU_TEXTUREUSAGE_SAMPLER;
        u32 mipLevels = 1;

        if (generateMips)
        {
            usage |= SDL_GPU_TEXTUREUSAGE_COLOR_TARGET;
            mipLevels = Private::SDLUtils::CalculateMipLevels(size);
        }

        SDL_GPUTextureCreateInfo textureInfo
        {
            .type = SDL_GPU_TEXTURETYPE_2D,
            .format = texFormat,
            .usage = usage,
            .width = size.Width,
            .height = size.Height,
            .layer_count_or_depth = 1,
            .num_levels = mipLevels,
            .sample_count = SDL_GPU_SAMPLECOUNT_1
        };

        // todo size tostring
        CGE_TRACE("Creating {}x{} texture.", size.Width, size.Height);
        SDL_GPUTexture* texture = SDL_CreateGPUTexture(_context->Device, &textureInfo);
        CGE_SDL_CHECK(texture, "Create texture")

        if (data)
        {
            _context->CopyDataToTexture(texture, data, Vec2u(0), size, texFormat);
            if (generateMips)
                _context->MipmapQueue.insert(texture);
        }

        return std::unique_ptr<Texture>(new Texture(*_context, texture, generateMips));
    }

    std::unique_ptr<Texture> Renderer::CreateTexture(const Bitmap& bitmap, bool generateMips) const
    {
        return CreateTexture(bitmap.Data, bitmap.Size, bitmap.Format, generateMips);
    }

    std::unique_ptr<Texture> Renderer::CreateTexture(const std::string& path, bool generateMips) const
    {
        Bitmap bitmap(path);
        return CreateTexture(bitmap.Data, bitmap.Size, bitmap.Format, generateMips);
    }

    void Renderer::Render()
    {
        SDL_GPUCommandBuffer* cb = SDL_AcquireGPUCommandBuffer(_context->Device);
        CGE_SDL_CHECK(cb, "Acquire command buffer");

        for (const auto& texture : _context->MipmapQueue)
        {
            CGE_TRACE("Generating mipmaps for texture {}", reinterpret_cast<usize>(texture));
            SDL_GenerateMipmapsForGPUTexture(cb, texture);
        }
        _context->MipmapQueue.clear();

        SDL_GPUTexture* swapchainTexture;
        CGE_SDL_CHECK(SDL_WaitAndAcquireGPUSwapchainTexture(cb, _context->Window, &swapchainTexture, nullptr, nullptr),
                      "Acquire swapchain texture");

        // don't bother rendering if there's nothing to do
        if (!swapchainTexture)
        {
            SDL_CancelGPUCommandBuffer(cb);
            return;
        }

        SDL_GPUColorTargetInfo targetInfo
        {
            .texture = swapchainTexture,
            .clear_color = { 1.0f, 0.5f, 0.25f, 1.0f },
            .load_op = SDL_GPU_LOADOP_CLEAR,
            .store_op = SDL_GPU_STOREOP_STORE,
        };
        SDL_GPURenderPass* pass = SDL_BeginGPURenderPass(cb, &targetInfo, 1, nullptr);
        CGE_SDL_CHECK(pass, "Begin render pass");
        SDL_EndGPURenderPass(pass);
        CGE_SDL_CHECK(SDL_SubmitGPUCommandBuffer(cb), "Submit command buffer");
    }
}
