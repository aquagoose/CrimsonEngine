#include "Renderer.h"

namespace cge
{
    Renderer::Renderer(SDL_Window* window)
    {
        _context = std::make_unique<RendererContext>(window);
    }

    void Renderer::Render()
    {
        VkCommandBuffer cb = _context->GetCommandBuffer();
        VkImageView swapchainImage = _context->GetNextSwapchainImage(cb);

        VkRenderingAttachmentInfo colorTarget
        {
            .sType = VK_STRUCTURE_TYPE_RENDERING_ATTACHMENT_INFO,
            .imageView = swapchainImage,
            .imageLayout = VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL,
            .loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR,
            .storeOp = VK_ATTACHMENT_STORE_OP_STORE,
            .clearValue = { 1.0f, 0.5f, 0.25f, 1.0f }
        };

        VkRenderingInfo renderInfo
        {
            .sType = VK_STRUCTURE_TYPE_RENDERING_INFO,
            .renderArea = { .extent = _context->SwapchainSize },
            .layerCount = 1,
            .colorAttachmentCount = 1,
            .pColorAttachments = &colorTarget
        };

        vkCmdBeginRendering(cb, &renderInfo);
        vkCmdEndRendering(cb);

        _context->SubmitAndPresent(cb);
    }
}
