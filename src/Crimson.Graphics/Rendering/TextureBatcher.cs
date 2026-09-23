using System.Numerics;
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

    public void AddToBatch(ref readonly Draw draw)
    {
        _draws.Add(draw);
    }

    public void Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget)
    {
        
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