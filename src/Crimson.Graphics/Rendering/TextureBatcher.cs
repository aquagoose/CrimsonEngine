using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Crimson.Core;
using Crimson.Graphics.Utils;
using piko.SDL3;
using Index = uint;

namespace Crimson.Graphics.Rendering;

internal sealed unsafe class TextureBatcher : IDisposable
{
    /// <summary>
    /// The initial maximum number of draws, before expansion.
    /// </summary>
    private const uint InitialMaxDrawCount = 4096;

    /// <summary>
    /// Number of vertices per sprite.
    /// </summary>
    private const uint NumVertices = 4;

    /// <summary>
    /// Number of indices per sprite.
    /// </summary>
    private const uint NumIndices = 6;

    private readonly RenderContext _context;
    private readonly List<Draw> _draws;
    private readonly List<Batch> _batches;
    
    private readonly SDL.GPUGraphicsPipeline _pipeline;
    private readonly SDL.GPUSampler _tempSampler; // todo sampler per texture
    
    private SDL.GPUBuffer _vertexBuffer;
    private SDL.GPUBuffer _indexBuffer;
    private uint _maxDraws;

    public TextureBatcher(RenderContext context, SDL.GPUTextureFormat targetFormat)
    {
        _context = context;
        _draws = [];
        _batches = [];
        
        _maxDraws = InitialMaxDrawCount;
        _vertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, InitialMaxDrawCount * NumVertices * (uint) sizeof(Vertex));
        _indexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, InitialMaxDrawCount * NumIndices * sizeof(Index));

        SDL.GPUShader vertexShader = _context.CreateShader(SDL.GPUShaderStage.Vertex, "TextureBatcher");
        SDL.GPUShader pixelShader = _context.CreateShader(SDL.GPUShaderStage.Fragment, "TextureBatcher");

        SDL.GPUColorTargetDescription targetDesc = new()
        {
            Format = targetFormat,
            BlendState = SDLUtils.NonPremultipliedBlend
        };

        SDL.GPUVertexBufferDescription vertexBuffer = new()
        {
            Slot = 0,
            Pitch = (uint) sizeof(Vertex),
            InputRate = SDL.GPUVertexInputRate.Vertex,
            InstanceStepRate = 0
        };

        SDL.GPUVertexAttribute* inputLayout = stackalloc SDL.GPUVertexAttribute[3]
        {
            new SDL.GPUVertexAttribute // position
            {
                Location = 0,
                BufferSlot = 0,
                Format = SDL.GPUVertexElementFormat.Float2,
                Offset = 0
            },
            new SDL.GPUVertexAttribute // texcoord
            {
                Location = 1,
                BufferSlot = 0,
                Format = SDL.GPUVertexElementFormat.Float2,
                Offset = 8
            },
            new SDL.GPUVertexAttribute // tint
            {
                Location = 2,
                BufferSlot = 0,
                Format = SDL.GPUVertexElementFormat.Float4,
                Offset = 16
            }
        };

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vertexShader,
            FragmentShader = pixelShader,
            TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo
            {
                NumColorTargets = 1,
                ColorTargetDescriptions = &targetDesc
            },
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist,
            VertexInputState = new SDL.GPUVertexInputState
            {
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBuffer,
                NumVertexAttributes = 3,
                VertexAttributes = inputLayout
            },
            RasterizerState = new SDL.GPURasterizerState
            {
                CullMode = SDL.GPUCullMode.Back,
                FrontFace = SDL.GPUFrontFace.Clockwise
            },
            DepthStencilState = new SDL.GPUDepthStencilState
            {
                EnableDepthTest = false,
                EnableDepthWrite = false
            }
        };
        
        Logger.Trace("Creating batcher pipeline.");
        _pipeline = SDL.CreateGPUGraphicsPipeline(_context.Device, &pipelineInfo).Check("Create pipeline");
        
        SDL.ReleaseGPUShader(_context.Device, pixelShader);
        SDL.ReleaseGPUShader(_context.Device, vertexShader);

        SDL.GPUSamplerCreateInfo samplerInfo = new()
        {
            MinFilter = SDL.GPUFilter.Linear,
            MagFilter = SDL.GPUFilter.Linear,
            MipmapMode = SDL.GPUSamplerMipmapMode.Linear,
            AddressModeU = SDL.GPUSamplerAddressMode.ClampToEdge,
            AddressModeV = SDL.GPUSamplerAddressMode.ClampToEdge,
            AddressModeW = SDL.GPUSamplerAddressMode.ClampToEdge,
            MinLod = 0,
            MaxLod = float.MaxValue
        };

        Logger.Trace("Creating temporary sampler.");
        _tempSampler = SDL.CreateGPUSampler(_context.Device, &samplerInfo).Check("Create sampler");
    }

    public void Clear()
    {
        _draws.Clear();
        _batches.Clear();
    }

    public void AddToBatch(ref readonly Draw draw)
    {
        _draws.Add(draw);
    }

    public void Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, ref readonly Camera camera, ref bool hasCleared)
    {
        // todo a lot of this could be moved to a compute shader
        #region Batching
        
        ReadOnlySpan<Draw> draws = CollectionsMarshal.AsSpan(_draws);
        if (draws.Length == 0) // don't even bother
            return;

        uint totalVerticesSize = (uint) draws.Length * NumVertices * (uint) sizeof(Vertex);
        uint totalIndicesSize = (uint) draws.Length * NumIndices * sizeof(Index);
        uint totalSize = totalVerticesSize + totalIndicesSize;

        SDL.GPUTransferBuffer transBuffer = _context.GetUploadBuffer(totalSize, out uint offset, out bool cycle);
        nint mapped = SDL.MapGPUTransferBuffer(_context.Device, transBuffer, cycle).Check("Map transfer buffer");

        Vertex* vertices = (Vertex*) (mapped + offset);
        Index* indices = (Index*) (mapped + offset + totalVerticesSize);

        _batches.Clear(); // ensure the batches are clear before regenerating new batches
        Texture? currentTexture = null;
        uint previousBatchOffset = 0;
        uint currentDraw;
        for (currentDraw = 0; currentDraw < draws.Length; currentDraw++)
        {
            ref readonly Draw draw = ref draws[(int) currentDraw];

            if (draw.Texture != currentTexture && currentTexture != null)
            {
                _batches.Add(new Batch(currentTexture, previousBatchOffset, currentDraw - previousBatchOffset));
                previousBatchOffset = currentDraw;
            }

            currentTexture = draw.Texture;
            
            uint vOffset = currentDraw * NumVertices;
            uint iOffset = currentDraw * NumIndices;

            vertices[vOffset + 0] = new Vertex(draw.TopLeft, new Vector2(0, 0), draw.Tint);
            vertices[vOffset + 1] = new Vertex(draw.TopRight, new Vector2(1, 0), draw.Tint);
            vertices[vOffset + 2] = new Vertex(draw.BottomRight, new Vector2(1, 1), draw.Tint);
            vertices[vOffset + 3] = new Vertex(draw.BottomLeft, new Vector2(0, 1), draw.Tint);

            indices[iOffset + 0] = 0 + vOffset;
            indices[iOffset + 1] = 1 + vOffset;
            indices[iOffset + 2] = 3 + vOffset;
            indices[iOffset + 3] = 1 + vOffset;
            indices[iOffset + 4] = 2 + vOffset;
            indices[iOffset + 5] = 3 + vOffset;
        }
        
        Debug.Assert(currentTexture != null);
        Debug.Assert(currentDraw != 0);
        _batches.Add(new Batch(currentTexture, previousBatchOffset, currentDraw - previousBatchOffset));
        
        SDL.UnmapGPUTransferBuffer(_context.Device, transBuffer);

        SDL.GPUCopyPass copyPass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUTransferBufferLocation vertSrc = new()
        {
            TransferBuffer = transBuffer,
            Offset = offset
        };

        SDL.GPUBufferRegion vertDest = new()
        {
            Buffer = _vertexBuffer,
            Offset = 0,
            Size = totalVerticesSize
        };
        
        SDL.UploadToGPUBuffer(copyPass, &vertSrc, &vertDest, false);

        SDL.GPUTransferBufferLocation indexSrc = new()
        {
            TransferBuffer = transBuffer,
            Offset = offset + totalVerticesSize
        };

        SDL.GPUBufferRegion indexDest = new()
        {
            Buffer = _indexBuffer,
            Offset = 0,
            Size = totalIndicesSize
        };
        
        SDL.UploadToGPUBuffer(copyPass, &indexSrc, &indexDest, false);
        
        SDL.EndGPUCopyPass(copyPass);
        
        #endregion

        #region Drawing
        
        // 2 * sizeof(Matrix4x4) = 128
        // the shader has its own camera struct that only wants the matrices, so we only need to pass in 2 matrices.
        fixed (Camera* cam = &camera)
            SDL.PushGPUVertexUniformData(cb, 0, (nint) cam, 128);

        SDL.GPUColorTargetInfo target = new()
        {
            Texture = colorTarget,
            ClearColor = new SDL.FColor(0.0f, 0.0f, 0.0f, 1.0f),
            LoadOp = hasCleared ? SDL.GPULoadOp.Load : SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass renderPass = SDL.BeginGPURenderPass(cb, &target, 1, null).Check("Begin render pass");

        SDL.BindGPUGraphicsPipeline(renderPass, _pipeline);
        SDL.BindGPUVertexBuffer(renderPass, 0, _vertexBuffer);
        SDL.BindGPUIndexBuffer(renderPass, _indexBuffer, SDL.GPUIndexElementSize.Size32bit);
        
        ReadOnlySpan<Batch> batches = CollectionsMarshal.AsSpan(_batches);
        for (int i = 0; i < batches.Length; i++)
        {
            ref readonly Batch batch = ref batches[i];
            SDL.BindGPUFragmentTextures(renderPass, 0, [batch.Texture], _tempSampler);
            SDL.DrawGPUIndexedPrimitives(renderPass, batch.Size * NumIndices, 1, batch.Offset * NumIndices, 0, 0);
        }
        
        SDL.EndGPURenderPass(renderPass);
        
        #endregion

        // will always clear if the value is false so we can just set it to true
        hasCleared = true;
    }

    public void Dispose()
    {
        SDL.ReleaseGPUSampler(_context.Device, _tempSampler);
        SDL.ReleaseGPUGraphicsPipeline(_context.Device, _pipeline);
        SDL.ReleaseGPUBuffer(_context.Device, _indexBuffer);
        SDL.ReleaseGPUBuffer(_context.Device, _vertexBuffer);
    }

    public readonly struct Draw
    {
        public readonly Texture Texture;
        public readonly Vector2 TopLeft;
        public readonly Vector2 TopRight;
        public readonly Vector2 BottomLeft;
        public readonly Vector2 BottomRight;
        public readonly Vector4 Tint; // todo color struct

        public Draw(Texture texture, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Vector4 tint)
        {
            Texture = texture;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Tint = tint;
        }
    }

    private readonly struct Vertex
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;
        public readonly Vector4 Tint; // todo color struct

        public Vertex(Vector2 position, Vector2 texCoord, Vector4 tint)
        {
            Position = position;
            TexCoord = texCoord;
            Tint = tint;
        }
    }

    private readonly struct Batch
    {
        public readonly Texture Texture;
        public readonly uint Offset; // number of quads offset
        public readonly uint Size; // number of quads in the batch

        public Batch(Texture texture, uint offset, uint size)
        {
            Texture = texture;
            Offset = offset;
            Size = size;
        }
    }
}