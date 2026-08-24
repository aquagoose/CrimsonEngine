#include "Renderer.h"

#include "Private/SDLUtils.h"

namespace cge
{
    Renderer::Renderer(SDL_Window* window)
    {
        _context = std::make_unique<Private::RenderContext>(window);
    }

    void Renderer::Render()
    {
        SDL_GPUCommandBuffer* cb = SDL_AcquireGPUCommandBuffer(_context->Device);
        CGE_SDL_CHECK(cb, "Acquire command buffer");

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
