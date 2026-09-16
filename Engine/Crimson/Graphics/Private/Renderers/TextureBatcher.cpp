#include "TextureBatcher.h"

#include "Core/BitUtils.h"
#include "Graphics/Private/SDLUtils.h"
#include "Math/Matrix.h"

#include <cassert>
#include <functional>

namespace cge::Private
{
    TextureBatcher::TextureBatcher(RenderContext& context, SDL_GPUTextureFormat outFormat) : _context(context)
    {
        _maxDraws = InitialMaxDraws;
        _vertexBuffer = _context.CreateBuffer(SDL_GPU_BUFFERUSAGE_VERTEX, _maxDraws * NumVertices * sizeof(Vertex));
        _indexBuffer = _context.CreateBuffer(SDL_GPU_BUFFERUSAGE_INDEX, _maxDraws * NumIndices * sizeof(Index));

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

        SDL_GPUSamplerCreateInfo samplerInfo
        {
            .min_filter = SDL_GPU_FILTER_LINEAR,
            .mag_filter = SDL_GPU_FILTER_LINEAR,
            .mipmap_mode = SDL_GPU_SAMPLERMIPMAPMODE_LINEAR,
            .address_mode_u = SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
            .address_mode_v = SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
            .min_lod = 0,
            .max_lod = F32_MAX,
        };

        CGE_TRACE("Creating temporary sampler.");
        _sampler = SDL_CreateGPUSampler(_context.Device, &samplerInfo);
        CGE_SDL_CHECK(_sampler, "Create sampler");
    }

    TextureBatcher::~TextureBatcher()
    {
        SDL_ReleaseGPUSampler(_context.Device, _sampler);
        SDL_ReleaseGPUGraphicsPipeline(_context.Device, _pipeline);
        SDL_ReleaseGPUBuffer(_context.Device, _indexBuffer);
        SDL_ReleaseGPUBuffer(_context.Device, _vertexBuffer);
    }

    void TextureBatcher::Clear()
    {
        _draws.clear();
        _batches.clear();
    }

    void TextureBatcher::AddToBatch(const Draw &draw)
    {
        _draws.emplace_back(draw);
    }

    bool TextureBatcher::Render(SDL_GPUCommandBuffer* cb, SDL_GPUTexture* texture, bool clear, const Camera& camera)
    {
        // if there's nothing to draw, don't bother.
        if (_draws.empty())
            return false;

        // resize the batcher if the number of draws exceeds the current maximum allowed number of draws
        if (_draws.size() >= _maxDraws)
        {
            CGE_TRACE(
                "Draw count ({} sprites) is greater than the current maximum draw count! ({} sprites). The batcher will be resized.",
                _draws.size(), _maxDraws);

            _maxDraws = BitUtils::RoundToNearestPowerOf2(_draws.size());
            SDL_ReleaseGPUBuffer(_context.Device, _vertexBuffer);
            SDL_ReleaseGPUBuffer(_context.Device, _indexBuffer);

            _vertexBuffer = _context.CreateBuffer(SDL_GPU_BUFFERUSAGE_VERTEX, _maxDraws * NumVertices * sizeof(Vertex));
            _indexBuffer = _context.CreateBuffer(SDL_GPU_BUFFERUSAGE_INDEX, _maxDraws * NumIndices * sizeof(Index));
        }

        u32 totalVerticesSize = _draws.size() * NumVertices * sizeof(Vertex);
        u32 totalIndicesSize = _draws.size() * NumIndices * sizeof(Index);
        u32 totalSize = totalVerticesSize + totalIndicesSize;

        u32 offset;
        bool cycle;
        SDL_GPUTransferBuffer* transBuffer = _context.GetUploadBuffer(totalSize, offset, cycle);

        void* mapped = SDL_MapGPUTransferBuffer(_context.Device, transBuffer, cycle);
        CGE_SDL_CHECK(mapped, "Map transfer buffer");
        // ensure the mapped pointer starts at the correct offset
        mapped = static_cast<void*>(static_cast<u8*>(mapped) + offset);

        auto vertices = static_cast<Vertex*>(mapped);
        auto indices = static_cast<Index*>(static_cast<void*>(static_cast<u8*>(mapped) + totalVerticesSize));

        _batches.clear(); // ensure the batches are clear incase Clear is not called in this frame.
        std::optional<std::reference_wrapper<Texture>> currentTexture = {};
        u32 totalDraws = 0;
        u32 currentNumDraws = 0;
        u32 currentOffset = 0;
        for (const Draw& draw : _draws)
        {
            if (currentNumDraws > 0 && currentTexture.has_value() && &draw.Texture != &currentTexture->get())
            {
                _batches.emplace_back(currentTexture->get(), currentOffset, currentNumDraws);
                currentOffset = totalDraws;
                currentNumDraws = 0;
            }

            currentTexture = draw.Texture;

            u32 vOffset = totalDraws * NumVertices;
            u32 iOffset = totalDraws * NumIndices;

            vertices[vOffset + 0] = { .Position = draw.TopLeft, .TexCoord = { 0, 0 }, .Tint = draw.Tint };
            vertices[vOffset + 1] = { .Position = draw.TopRight, .TexCoord = { 1, 0 }, .Tint = draw.Tint };
            vertices[vOffset + 2] = { .Position = draw.BottomRight, .TexCoord = { 1, 1 }, .Tint = draw.Tint };
            vertices[vOffset + 3] = { .Position = draw.BottomLeft, .TexCoord = { 0, 1 }, .Tint = draw.Tint };

            indices[iOffset + 0] = 0 + vOffset;
            indices[iOffset + 1] = 1 + vOffset;
            indices[iOffset + 2] = 3 + vOffset;
            indices[iOffset + 3] = 1 + vOffset;
            indices[iOffset + 4] = 2 + vOffset;
            indices[iOffset + 5] = 3 + vOffset;

            currentNumDraws++;
            totalDraws++;
        }

        assert(currentNumDraws > 0);
        assert(currentTexture.has_value());
        _batches.emplace_back(currentTexture->get(), currentOffset, currentNumDraws);

        SDL_UnmapGPUTransferBuffer(_context.Device, transBuffer);

        SDL_GPUTransferBufferLocation vertSrc
        {
            .transfer_buffer = transBuffer,
            .offset = offset
        };

        SDL_GPUBufferRegion vertDest
        {
            .buffer = _vertexBuffer,
            .offset = 0,
            .size = totalVerticesSize
        };

        SDL_GPUTransferBufferLocation idxSrc
        {
            .transfer_buffer = transBuffer,
            .offset = offset + totalVerticesSize
        };

        SDL_GPUBufferRegion idxDest
        {
            .buffer = _indexBuffer,
            .offset = 0,
            .size = totalIndicesSize
        };

        SDL_GPUCopyPass* copyPass = SDL_BeginGPUCopyPass(cb);
        CGE_SDL_CHECK(copyPass, "Begin copy pass");

        SDL_UploadToGPUBuffer(copyPass, &vertSrc, &vertDest, false);
        SDL_UploadToGPUBuffer(copyPass, &idxSrc, &idxDest, false);

        SDL_EndGPUCopyPass(copyPass);

        SDL_PushGPUVertexUniformData(cb, 0, &camera, sizeof(Camera));

        SDL_GPUColorTargetInfo colorTarget
        {
            .texture = texture,
            .clear_color = { 0.0f, 0.0f, 0.0f, 1.0f },
            .load_op = clear ? SDL_GPU_LOADOP_CLEAR : SDL_GPU_LOADOP_LOAD,
            .store_op = SDL_GPU_STOREOP_STORE
        };

        SDL_GPURenderPass* renderPass = SDL_BeginGPURenderPass(cb, &colorTarget, 1, nullptr);
        CGE_SDL_CHECK(renderPass, "Begin render pass");

        SDL_BindGPUGraphicsPipeline(renderPass, _pipeline);

        SDL_GPUBufferBinding vertexBuffer { .buffer = _vertexBuffer, .offset = 0 };
        SDL_BindGPUVertexBuffers(renderPass, 0, &vertexBuffer, 1);

        SDL_GPUBufferBinding indexBuffer { .buffer = _indexBuffer, .offset = 0 };
        SDL_BindGPUIndexBuffer(renderPass, &indexBuffer, SDL_GPU_INDEXELEMENTSIZE_32BIT);

        for (const Batch& batch : _batches)
        {
            SDL_GPUTextureSamplerBinding textureBinding
            {
                .texture = static_cast<SDL_GPUTexture*>(batch.Texture.Handle()),
                .sampler = _sampler
            };

            SDL_BindGPUFragmentSamplers(renderPass, 0, &textureBinding, 1);

            SDL_DrawGPUIndexedPrimitives(renderPass, batch.NumDraws * NumIndices, 1, batch.DrawOffset * NumIndices, 0, 0);
        }

        SDL_EndGPURenderPass(renderPass);

        return true;
    }
}
