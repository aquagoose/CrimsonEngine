#include "TextureBatcher.h"

#include "Graphics/Private/SDLUtils.h"

namespace cge::Private
{
    TextureBatcher::TextureBatcher(RenderContext& context, SDL_GPUTextureFormat outFormat) : _context(context)
    {
        _batchSize = InitialBatchSize;
        _vertexBuffer = _context.CreateBuffer(SDL_GPU_BUFFERUSAGE_VERTEX, _batchSize * NumVertices * sizeof(Vertex));
        _indexBuffer = _context.CreateBuffer(SDL_GPU_BUFFERUSAGE_INDEX, _batchSize * NumIndices * sizeof(Index));

        SDL_GPUShader* vtxShader = _context.CreateShader(ShaderStage::Vertex, "TextureBatcher", "VSMain",  { .NumUniforms = 1 });
        SDL_GPUShader* pxlShader = _context.CreateShader(ShaderStage::Pixel, "TextureBatcher", "PSMain", { .NumSamplers = 1 });

        SDL_GPUVertexBufferDescription vertexBuffer
        {
            .slot = 0,
            .pitch = sizeof(Vertex),
            .input_rate = SDL_GPU_VERTEXINPUTRATE_VERTEX,
            .instance_step_rate = 0
        };

        constexpr u32 numVertexAttributes = 3;
        SDL_GPUVertexAttribute vertexAttributes[numVertexAttributes]
        {
            {
                .location = 0,
                .buffer_slot = 0,
                .format = SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
                .offset = offsetof(Vertex, Position)
            },
            {
                .location = 1,
                .buffer_slot = 0,
                .format = SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
                .offset = offsetof(Vertex, TexCoord)
            },
            {
                .location = 2,
                .buffer_slot = 0,
                .format = SDL_GPU_VERTEXELEMENTFORMAT_FLOAT4,
                .offset = offsetof(Vertex, Tint)
            }
        };

        SDL_GPUColorTargetDescription targetDesc
        {
            .format = outFormat,
            .blend_state = SDLUtils::PremultipliedBlend
        };

        SDL_GPUGraphicsPipelineCreateInfo pipelineInfo
        {
            .vertex_shader = vtxShader,
            .fragment_shader = pxlShader,
            .vertex_input_state = {
                .vertex_buffer_descriptions = &vertexBuffer,
                .num_vertex_buffers = 1,
                .vertex_attributes = vertexAttributes,
                .num_vertex_attributes = numVertexAttributes
            },
            .primitive_type = SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,
            .rasterizer_state = {
                .fill_mode = SDL_GPU_FILLMODE_FILL,
                .cull_mode = SDL_GPU_CULLMODE_BACK,
                .front_face = SDL_GPU_FRONTFACE_CLOCKWISE
            },
            .multisample_state = {},
            .depth_stencil_state = {
                .enable_depth_test = false,
                .enable_depth_write = false
            },
            .target_info = {
                .color_target_descriptions = &targetDesc,
                .num_color_targets = 1,
                .has_depth_stencil_target = false,
            }
        };

        _pipeline = SDL_CreateGPUGraphicsPipeline(_context.Device, &pipelineInfo);
        CGE_SDL_CHECK(_pipeline, "Create pipeline");

        SDL_ReleaseGPUShader(_context.Device, pxlShader);
        SDL_ReleaseGPUShader(_context.Device, vtxShader);
    }

    TextureBatcher::~TextureBatcher()
    {
        SDL_ReleaseGPUGraphicsPipeline(_context.Device, _pipeline);
        SDL_ReleaseGPUBuffer(_context.Device, _indexBuffer);
        SDL_ReleaseGPUBuffer(_context.Device, _vertexBuffer);
    }

    void TextureBatcher::AddToBatch(const Draw &draw)
    {
        _draws.emplace_back(draw);
    }
}
