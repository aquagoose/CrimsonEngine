#include <Core/Logger.h>
#include <Graphics/RendererContext.h>
#include <SDL3/SDL.h>

int main(int argc, char* argv[])
{
    if (!SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS))
        CGE_FATAL("Failed to initialize SDL: {}", SDL_GetError());

    SDL_Window* window = SDL_CreateWindow("Create Context Test", 1280, 720, SDL_WINDOW_VULKAN);
    if (!window)
        CGE_FATAL("Failed to create window: {}", SDL_GetError());

    auto context = new cge::RendererContext(window);

    bool running = true;
    while (running)
    {
        SDL_Event event;
        while (SDL_PollEvent(&event))
        {
            switch (event.type)
            {
                case SDL_EVENT_QUIT:
                    running = false;
            }
        }

        VkCommandBuffer cb = context->GetCommandBuffer();
        VkImageView image = context->GetNextSwapchainImage(cb);

        VkRenderingAttachmentInfo colorAttachment
        {
            .sType = VK_STRUCTURE_TYPE_RENDERING_ATTACHMENT_INFO,
            .imageView = image,
            .imageLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL,
            .loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR,
            .storeOp = VK_ATTACHMENT_STORE_OP_STORE,
            .clearValue = { 1.0f, 0.5f, 0.25f, 1.0f }
        };

        VkRenderingInfo renderingInfo
        {
            .sType = VK_STRUCTURE_TYPE_RENDERING_INFO,
            .renderArea = { 0, 0, 1280, 720 },
            .layerCount = 1,
            .colorAttachmentCount = 1,
            .pColorAttachments = &colorAttachment,
        };

        vkCmdBeginRendering(cb, &renderingInfo);
        vkCmdEndRendering(cb);

        context->SubmitAndPresent(cb);
    }

    delete context;
    SDL_DestroyWindow(window);
    SDL_Quit();
    return 0;
}
