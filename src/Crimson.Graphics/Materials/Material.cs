using Crimson.Core;
using Crimson.Graphics.Utils;
using piko.SDL3;

namespace Crimson.Graphics.Materials;

/// <summary>
/// A material can be applied to a <see cref="Mesh"/>, and determines the mesh's appearance.
/// </summary>
public abstract class Material : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Material"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }
    
    private readonly RenderContext _context;
    
    internal readonly SDL.GPUGraphicsPipeline Pipeline;

    public readonly MaterialInfo Info;

    protected unsafe Material(string shader, ref readonly MaterialInfo info)
    {
        _context = Renderer.Context;
        Info = info;

        SDL.GPUShader vertexShader = _context.CreateShader(SDL.GPUShaderStage.Vertex, shader);
        SDL.GPUShader pixelShader = _context.CreateShader(SDL.GPUShaderStage.Fragment, shader);

        SDL.GPUColorTargetDescription colorTarget = new()
        {
            Format = Renderer.MainRendererTargetFormat,
            BlendState = SDLUtils.NoBlend
        };

        SDL.GPUVertexBufferDescription vertexBuffer = new()
        {
            Slot = 0,
            Pitch = (uint) sizeof(Vertex),
            InputRate = SDL.GPUVertexInputRate.Vertex,
            InstanceStepRate = 0
        };

        const int numVertexAttributes = 4;
        SDL.GPUVertexAttribute* inputLayout = stackalloc SDL.GPUVertexAttribute[numVertexAttributes]
        {
            new SDL.GPUVertexAttribute(0, 0, SDL.GPUVertexElementFormat.Float3, 0), // Position
            new SDL.GPUVertexAttribute(1, 0, SDL.GPUVertexElementFormat.Float2, 12), // TexCoord
            new SDL.GPUVertexAttribute(2, 0, SDL.GPUVertexElementFormat.Float3, 20), // Normal
            new SDL.GPUVertexAttribute(3, 0, SDL.GPUVertexElementFormat.Float4, 32) // Color
        };

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vertexShader,
            FragmentShader = pixelShader,
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist,
            TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo
            {
                NumColorTargets = 1,
                ColorTargetDescriptions = &colorTarget,
                // todo depth target
            },
            VertexInputState = new SDL.GPUVertexInputState
            {
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBuffer,
                NumVertexAttributes = numVertexAttributes,
                VertexAttributes = inputLayout
            },
            DepthStencilState = new SDL.GPUDepthStencilState // todo depth buffer
            {
                EnableDepthTest = false,
                EnableDepthWrite = false,
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
        
        Logger.Trace("Creating graphics pipeline.");
        Pipeline = SDL.CreateGPUGraphicsPipeline(_context.Device, &pipelineInfo).Check("Create pipeline");
        
        SDL.ReleaseGPUShader(_context.Device, pixelShader);
        SDL.ReleaseGPUShader(_context.Device, vertexShader);
    }
    
    /// <summary>
    /// Dispose of this <see cref="Material"/>.
    /// </summary>
    public virtual void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        SDL.ReleaseGPUGraphicsPipeline(_context.Device, Pipeline);
    }
}