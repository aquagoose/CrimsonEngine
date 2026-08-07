using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Graphics.SDLGPU;
using Crimson.Math;
using piko.SDL3;
using piko.SDL3.ShaderCross;

namespace Crimson.Graphics.Rendering;

/// <summary>
/// Batches sprites together.
/// </summary>
internal unsafe class SpriteBatcher : IDisposable
{
    /// <summary>
    /// The initial number of sprites the batch can support, before expansion.
    /// </summary>
    private const uint InitialBatchSize = 4096;

    /// <summary>
    /// The number of vertices per sprite.
    /// </summary>
    private const uint NumVertices = 4;

    /// <summary>
    /// The number of indices per sprite.
    /// </summary>
    private const uint NumIndices = 6;

    private readonly GPUContext _context;

    private uint _maxSprites;
    private List<Sprite> _sprites;
    private List<Batch> _batches;

    // dynamic resizing vertex and index buffers
    private SDL.GPUBuffer _vertexBuffer;
    private SDL.GPUBuffer _indexBuffer;

    private readonly SDL.GPUGraphicsPipeline _pipeline;

    // todo: set sampler per texture (obviously making it as efficient as possible)
    private readonly SDL.GPUSampler _temporarySampler;

    public SpriteBatcher(GPUContext context, SDL.GPUTextureFormat targetFormat)
    {
        _context = context;
        _sprites = [];
        _batches = [];

        _maxSprites = InitialBatchSize;

        _vertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, (uint) (InitialBatchSize * NumVertices * sizeof(Vertex)));
        _indexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, InitialBatchSize * NumIndices * sizeof(uint));

        SDL.GPUShader vShader = _context.CreateShader(SDLShaderCross.ShaderStage.Vertex, "SpriteBatcher", "VSMain");
        SDL.GPUShader pShader = _context.CreateShader(SDLShaderCross.ShaderStage.Fragment, "SpriteBatcher", "PSMain");

        SDL.GPUColorTargetDescription targetDesc = new()
        {
            Format = targetFormat,
            BlendState = SDL.GPUColorTargetBlendState.NonPremultiplied
        };

        SDL.GPUVertexBufferDescription vertexBufferDesc = new()
        {
            Slot = 0,
            Pitch = (uint) sizeof(Vertex),
            InputRate = SDL.GPUVertexInputRate.Vertex
        };

        SDL.GPUVertexAttribute* inputLayout = stackalloc SDL.GPUVertexAttribute[3]
        {
            new SDL.GPUVertexAttribute // position
            {
                Format = SDL.GPUVertexElementFormat.Float2,
                Offset = 0,
                Location = 0,
                BufferSlot = 0
            },
            new SDL.GPUVertexAttribute // texcoord
            {
                Format = SDL.GPUVertexElementFormat.Float2,
                Offset = 8,
                Location = 1,
                BufferSlot = 0
            },
            new SDL.GPUVertexAttribute // tint
            {
                Format = SDL.GPUVertexElementFormat.Float4,
                Offset = 16,
                Location = 2,
                BufferSlot = 0
            }
        };

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vShader,
            FragmentShader = pShader,
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist,
            TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo
            {
                NumColorTargets = 1,
                ColorTargetDescriptions = &targetDesc,
                HasDepthStencilTarget = false
            },
            VertexInputState = new SDL.GPUVertexInputState
            {
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBufferDesc,
                NumVertexAttributes = 3,
                VertexAttributes = inputLayout
            },
            DepthStencilState = new SDL.GPUDepthStencilState
            {
                EnableDepthTest = false,
                EnableDepthWrite = false
            },
            RasterizerState = new SDL.GPURasterizerState
            {
                CullMode = SDL.GPUCullMode.Back,
                FrontFace = SDL.GPUFrontFace.Clockwise,
                FillMode = SDL.GPUFillMode.Fill
            },
            MultisampleState = new SDL.GPUMultisampleState
            {
                SampleCount = SDL.GPUSampleCount.Count1
            }
        };

        _pipeline = SDL.CreateGPUGraphicsPipeline(_context.Device, &pipelineInfo).Check("Create pipeline");

        SDL.ReleaseGPUShader(_context.Device, pShader);
        SDL.ReleaseGPUShader(_context.Device, vShader);

        SDL.GPUSamplerCreateInfo samplerInfo = new()
        {
            MinFilter = SDL.GPUFilter.Linear,
            MagFilter = SDL.GPUFilter.Linear,
            MipmapMode = SDL.GPUSamplerMipmapMode.Linear,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            MinLod = 0,
            MaxLod = float.MaxValue,
            CompareOp = SDL.GPUCompareOp.GreaterOrEqual
        };
        Logger.Trace("Creating temporary sampler");
        _temporarySampler = SDL.CreateGPUSampler(_context.Device, &samplerInfo).Check("Create sampler");
    }

    public void Draw(in Sprite sprite)
    {
        _sprites.Add(sprite);
    }

    public bool Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, TransformMatrices matrices, bool needsClear)
    {
        uint numSprites = (uint) _sprites.Count;

        // just don't even bother
        if (numSprites == 0)
            return false;

        // resize the buffers if needed
        if (numSprites > _maxSprites)
        {
            Logger.Debug($"The number of sprites ({numSprites}) exceeds the maximum the batcher can support ({_maxSprites})! The buffers will be resized.");
            _maxSprites = BitUtils.RoundToNextPowerOf2(numSprites);

            SDL.ReleaseGPUBuffer(_context.Device, _vertexBuffer);
            SDL.ReleaseGPUBuffer(_context.Device, _indexBuffer);

            _vertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, (uint) (_maxSprites * NumVertices * sizeof(Vertex)));
            _indexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, _maxSprites * NumIndices * sizeof(uint));
        }

        #region Add sprites to batch

        uint verticesSize = numSprites * NumVertices * (uint) sizeof(Vertex);
        uint indicesSize = numSprites * NumIndices * sizeof(uint);
        uint totalUploadSize = verticesSize + indicesSize;

        // we're writing directly to the transfer buffer itself, so map it before we do anything
        SDL.GPUTransferBuffer transBuf = _context.GetTransferBuffer(totalUploadSize, out uint offset, out bool cycle);
        void* mapped = SDL.MapGPUTransferBuffer(_context.Device, transBuf, (byte) (cycle ? 1 : 0));
        if (mapped == null)
            throw new Exception($"Failed to map buffer: {SDL.GetError()}");

        // for our convenience...
        Vertex* vertices = (Vertex*) ((byte*) mapped + offset);
        uint* indices = (uint*) ((byte*) mapped + offset + verticesSize);

        Texture? currentTexture = null;
        uint currentSpriteCount = 0;
        uint spriteCountAtLastTextureChange = 0;
        foreach (Sprite sprite in _sprites)
        {
            // on texture change, add the batch to the batches so it can be batched by the batcher
            if (currentTexture != sprite.Texture && currentTexture != null)
            {
                _batches.Add(new Batch(currentTexture, spriteCountAtLastTextureChange,
                    currentSpriteCount - spriteCountAtLastTextureChange));

                spriteCountAtLastTextureChange = currentSpriteCount;
            }

            currentTexture = sprite.Texture;

            uint vOffset = currentSpriteCount * NumVertices;
            uint iOffset = currentSpriteCount * NumIndices;

            vertices[vOffset + 0] = new Vertex(sprite.TopLeft, new Vector2(0, 0), sprite.Tint);
            vertices[vOffset + 1] = new Vertex(sprite.TopRight, new Vector2(1, 0), sprite.Tint);
            vertices[vOffset + 2] = new Vertex(sprite.BottomRight, new Vector2(1, 1), sprite.Tint);
            vertices[vOffset + 3] = new Vertex(sprite.BottomLeft, new Vector2(0, 1), sprite.Tint);

            indices[iOffset + 0] = 0 + vOffset;
            indices[iOffset + 1] = 1 + vOffset;
            indices[iOffset + 2] = 3 + vOffset;
            indices[iOffset + 3] = 1 + vOffset;
            indices[iOffset + 4] = 2 + vOffset;
            indices[iOffset + 5] = 3 + vOffset;

            currentSpriteCount++;
        }

        // manually add the last batch
        Debug.Assert(currentTexture != null);
        _batches.Add(new Batch(currentTexture, spriteCountAtLastTextureChange,
            currentSpriteCount - spriteCountAtLastTextureChange));

        SDL.UnmapGPUTransferBuffer(_context.Device, transBuf);

        #endregion

        #region Copy Pass

        SDL.GPUTransferBufferLocation src = new()
        {
            TransferBuffer = transBuf
        };

        SDL.GPUCopyPass copyPass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUBufferRegion vtxDest = new()
        {
            Buffer = _vertexBuffer,
            Offset = 0,
            Size = verticesSize
        };
        src.Offset = offset + 0;
        SDL.UploadToGPUBuffer(copyPass, &src, &vtxDest, 0);

        SDL.GPUBufferRegion idxDest = new()
        {
            Buffer = _indexBuffer,
            Offset = 0,
            Size = indicesSize
        };
        src.Offset = offset + verticesSize;
        SDL.UploadToGPUBuffer(copyPass, &src, &idxDest, 0);

        SDL.EndGPUCopyPass(copyPass);

        #endregion

        SDL.PushGPUVertexUniformData(cb, 0, &matrices, (uint) sizeof(TransformMatrices));

        #region Render pass

        SDL.GPUColorTargetInfo targetInfo = new()
        {
            Texture = colorTarget,
            ClearColor = new SDL.FColor(0, 0, 0, 1),
            LoadOp = needsClear ? SDL.GPULoadOp.Clear : SDL.GPULoadOp.Load,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass renderPass = SDL.BeginGPURenderPass(cb, &targetInfo, 1, null)
            .Check("Begin render pass");

        SDL.BindGPUGraphicsPipeline(renderPass, _pipeline);

        SDL.GPUBufferBinding vtxBinding = new()
        {
            Buffer = _vertexBuffer,
            Offset = 0
        };
        SDL.BindGPUVertexBuffers(renderPass, 0, &vtxBinding, 1);

        SDL.GPUBufferBinding idxBinding = new()
        {
            Buffer = _indexBuffer,
            Offset = 0
        };
        SDL.BindGPUIndexBuffer(renderPass, &idxBinding, SDL.GPUIndexElementSize.Size32bit);

        foreach (Batch batch in _batches)
        {
            SDL.GPUTextureSamplerBinding textureBinding = new()
            {
                Texture = batch.Texture.Handle,
                Sampler = _temporarySampler
            };
            SDL.BindGPUFragmentSamplers(renderPass, 0, &textureBinding, 1);

            // to prevent rebinding of the vertex buffers, we draw with an offset into the index buffer.
            uint iOffset = batch.SpriteOffset * NumIndices;
            uint numIndices = batch.NumSprites * NumIndices;
            SDL.DrawGPUIndexedPrimitives(renderPass, numIndices, 1, iOffset, 0, 0);
        }

        SDL.EndGPURenderPass(renderPass);

        #endregion

        // reset state
        _sprites.Clear();
        _batches.Clear();

        return true;
    }

    public void Dispose()
    {
        SDL.ReleaseGPUSampler(_context.Device, _temporarySampler);
        SDL.ReleaseGPUGraphicsPipeline(_context.Device, _pipeline);

        SDL.ReleaseGPUBuffer(_context.Device, _indexBuffer);
        SDL.ReleaseGPUBuffer(_context.Device, _vertexBuffer);
    }

    public readonly struct Sprite
    {
        public readonly Texture Texture;
        public readonly Vector2 TopLeft;
        public readonly Vector2 TopRight;
        public readonly Vector2 BottomLeft;
        public readonly Vector2 BottomRight;
        public readonly Color Tint;

        public Sprite(Texture texture, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Color tint)
        {
            Texture = texture;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Tint = tint;
        }
    }

    public readonly struct TransformMatrices
    {
        public readonly Matrix4x4 Projection;
        public readonly Matrix4x4 Transform;

        public TransformMatrices(Matrix4x4 projection, Matrix4x4 transform)
        {
            Projection = projection;
            Transform = transform;
        }
    }

    private readonly struct Vertex
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;
        public readonly Color Color;

        public Vertex(Vector2 position, Vector2 texCoord, Color color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
    }

    private readonly struct Batch
    {
        /// <summary>
        /// The texture to use in the batch.
        /// </summary>
        public readonly Texture Texture;

        /// <summary>
        /// The sprite index offset.
        /// </summary>
        public readonly uint SpriteOffset;

        /// <summary>
        /// The number of sprites in this batch.
        /// </summary>
        public readonly uint NumSprites;

        public Batch(Texture texture, uint spriteOffset, uint numSprites)
        {
            Texture = texture;
            SpriteOffset = spriteOffset;
            NumSprites = numSprites;
        }
    }
}