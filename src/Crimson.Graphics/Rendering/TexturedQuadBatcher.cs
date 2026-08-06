using System.Numerics;
using Crimson.Graphics.SDLGPU;
using piko.SDL3;
using piko.SDL3.ShaderCross;

namespace Crimson.Graphics.Rendering;

/// <summary>
/// Batches 2D textured quads together.
/// </summary>
internal unsafe class TexturedQuadBatcher : IDisposable
{
    /// <summary>
    /// The initial number of quads the batch can support, before expansion.
    /// </summary>
    private const uint InitialBatchSize = 4096;

    /// <summary>
    /// The number of vertices per quad.
    /// </summary>
    private const uint NumVertices = 4;

    /// <summary>
    /// The number of indices per quad.
    /// </summary>
    private const uint NumIndices = 6;

    private readonly GPUContext _context;

    // dynamically resizing vertex and index lists
    private Vertex[] _vertices;
    private uint[] _indices;

    // dynamic resizing vertex and index buffers
    private SDL.GPUBuffer _vertexBuffer;
    private SDL.GPUBuffer _indexBuffer;

    private readonly SDL.GPUGraphicsPipeline _pipeline;

    public TexturedQuadBatcher(GPUContext context, SDL.GPUTextureFormat targetFormat)
    {
        _context = context;

        _vertices = new Vertex[InitialBatchSize * NumVertices];
        _indices = new uint[InitialBatchSize * NumIndices];

        _vertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, (uint) (_vertices.Length * sizeof(Vertex)));
        _indexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, (uint) (_indices.Length * sizeof(uint)));

        SDL.GPUShader vShader = _context.CreateShader(SDLShaderCross.ShaderStage.Vertex, "Texture", "VSMain");
        SDL.GPUShader pShader = _context.CreateShader(SDLShaderCross.ShaderStage.Fragment, "Texture", "PSMain");

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
    }

    public void Dispose()
    {
        SDL.ReleaseGPUGraphicsPipeline(_context.Device, _pipeline);

        SDL.ReleaseGPUBuffer(_context.Device, _indexBuffer);
        SDL.ReleaseGPUBuffer(_context.Device, _vertexBuffer);
    }

    public readonly struct Vertex
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
}