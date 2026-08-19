using Crimson.Core;
using Crimson.Graphics.SDLGPU;
using piko.SDL3;
using piko.SDL3.ShaderCross;

namespace Crimson.Graphics.Materials;

/// <summary>
/// A material describes the appearance of a <see cref="Mesh"/>, containing a shader, textures, and various parameters.
/// </summary>
public abstract class Material : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Material"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    private readonly GPUContext _context;

    internal readonly SDL.GPUGraphicsPipeline Pipeline;

    protected unsafe Material(ref readonly MaterialInfo info, string shader)
    {
        _context = Renderer.Context;

        SDL.GPUShader vShader = _context.CreateShader(SDLShaderCross.ShaderStage.Vertex, shader, "VSMain");
        SDL.GPUShader pShader = _context.CreateShader(SDLShaderCross.ShaderStage.Fragment, shader, "PSMain");

        SDL.GPUColorTargetDescription colorTarget = new()
        {
            Format = Renderer.TargetFormat3D,
            BlendState = new SDL.GPUColorTargetBlendState()
            {
                EnableBlend = false
            }
        };

        SDL.GPUGraphicsPipelineTargetInfo targetInfo = new()
        {
            NumColorTargets = 1,
            ColorTargetDescriptions = &colorTarget
        };

        SDL.GPUVertexAttribute* inputLayout = stackalloc SDL.GPUVertexAttribute[4]
        {
            new SDL.GPUVertexAttribute // position
            {
                Location = 0,
                Format = SDL.GPUVertexElementFormat.Float3,
                Offset = 0,
                BufferSlot = 0
            },
            new SDL.GPUVertexAttribute // texcoord
            {
                Location = 1,
                Format = SDL.GPUVertexElementFormat.Float2,
                Offset = 12,
                BufferSlot = 0
            },
            new SDL.GPUVertexAttribute // normal
            {
                Location = 2,
                Format = SDL.GPUVertexElementFormat.Float3,
                Offset = 20,
                BufferSlot = 0
            },
            new SDL.GPUVertexAttribute // color
            {
                Location = 3,
                Format = SDL.GPUVertexElementFormat.Float4,
                Offset = 32,
                BufferSlot = 0
            }
        };

        SDL.GPUVertexBufferDescription vertexBufferDesc = new()
        {
            Slot = 0,
            Pitch = (uint) sizeof(Vertex),
            InputRate = SDL.GPUVertexInputRate.Vertex,
            InstanceStepRate = 0
        };

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vShader,
            FragmentShader = pShader,
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist,
            TargetInfo = targetInfo,
            VertexInputState = new SDL.GPUVertexInputState
            {
                NumVertexAttributes = 1,
                VertexAttributes = inputLayout,
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBufferDesc
            },
            DepthStencilState = new SDL.GPUDepthStencilState
            {
                EnableDepthTest = false
            },
            RasterizerState = new SDL.GPURasterizerState
            {
                CullMode = SDL.GPUCullMode.None,
                FillMode = SDL.GPUFillMode.Fill
            },
            MultisampleState = new SDL.GPUMultisampleState
            {
                SampleCount = SDL.GPUSampleCount.Count1
            }
        };

        Logger.Trace("Creating material pipeline.");
        Pipeline = SDL.CreateGPUGraphicsPipeline(_context.Device, &pipelineInfo).Check("Create pipeline");
    }

    /// <summary>
    /// Unload any resources such as textures that have been created in this material.
    /// </summary>
    protected virtual void Unload() { }

    /// <summary>
    /// Dispose of this <see cref="Material"/>.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        Unload();

        SDL.ReleaseGPUGraphicsPipeline(_context.Device, Pipeline);
    }
}