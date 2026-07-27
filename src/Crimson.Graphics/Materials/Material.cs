using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.Materials;

/// <summary>
/// A material that is used during rendering.
/// </summary>
public abstract class Material : IDisposable
{
    private readonly SDL.GPUDevice _device;
    
    internal readonly SDL.GPUGraphicsPipeline Pipeline;
    
    public Texture Albedo;

    public Texture Normal;

    public Texture Metallic;

    public Texture Roughness;

    public Texture Occlusion;

    public Texture Emission;

    public Color AlbedoTint;

    public float MetallicMultiplier;

    public float RoughnessMultiplier;
    
    /// <summary>
    /// Create a <see cref="Material"/> from the given definition.
    /// </summary>
    /// <param name="definition">The <see cref="MaterialDefinition"/> that describes how the material should be created.</param>
    protected unsafe Material(in MaterialDefinition definition, string shader)
    {
        Albedo = definition.Albedo;
        Normal = definition.Normal ?? Texture.EmptyNormal;
        Metallic = definition.Metallic ?? Texture.White;
        Roughness = definition.Roughness ?? Texture.White;
        Occlusion = definition.Occlusion ?? Texture.White;
        Emission = definition.Emission ?? Texture.Black;

        AlbedoTint = definition.AlbedoTint;
        MetallicMultiplier = definition.MetallicMultiplier;
        RoughnessMultiplier = definition.RoughnessMultiplier;

        _device = Renderer.Device;

        // TODO: Probably best not to load this shader every time a material is created.
        ShaderUtils.LoadGraphicsShader(_device, shader, out SDL.GPUShader? vertexShader, out SDL.GPUShader? pixelShader);

        SDL.GPUVertexBufferDescription vertexBufferDesc = new()
        {
            Slot = 0,
            InputRate = SDL.GPUVertexInputRate.Vertex,
            InstanceStepRate = 0,
            Pitch = Vertex.SizeInBytes
        };

        SDL.GPUVertexAttribute* vertexAttributes = stackalloc SDL.GPUVertexAttribute[]
        {
            new SDL.GPUVertexAttribute // Position
                { Format = SDL.GPUVertexElementFormat.Float3, Offset = 0, BufferSlot = 0, Location = 0 },
            new SDL.GPUVertexAttribute // TexCoord
                { Format = SDL.GPUVertexElementFormat.Float2, Offset = 12, BufferSlot = 0, Location = 1 },
            new SDL.GPUVertexAttribute // Color
                { Format = SDL.GPUVertexElementFormat.Float4, Offset = 20, BufferSlot = 0, Location = 2 },
            new SDL.GPUVertexAttribute // Normal
                { Format = SDL.GPUVertexElementFormat.Float3, Offset = 36, BufferSlot = 0, Location = 3 }
        };

        SDL.GPUColorTargetDescription* colorTargets = stackalloc SDL.GPUColorTargetDescription[]
        {
            new SDL.GPUColorTargetDescription { Format = SDL.GPUTextureFormat.R32g32b32a32Float }, // Albedo
            new SDL.GPUColorTargetDescription { Format = SDL.GPUTextureFormat.R32g32b32a32Float }, // Position
            new SDL.GPUColorTargetDescription { Format = SDL.GPUTextureFormat.R32g32b32a32Float }, // Normal
            new SDL.GPUColorTargetDescription { Format = SDL.GPUTextureFormat.R32g32b32a32Float } // MetallicRoughness
        };

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vertexShader.Value,
            FragmentShader = pixelShader.Value,
            TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo()
            {
                NumColorTargets = 4,
                ColorTargetDescriptions = colorTargets,
                HasDepthStencilTarget = true,
                DepthStencilFormat = SDL.GPUTextureFormat.D32Float
            },
            VertexInputState = new SDL.GPUVertexInputState()
            {
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBufferDesc,
                NumVertexAttributes = 4,
                VertexAttributes = vertexAttributes
            },
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist,
            DepthStencilState = new SDL.GPUDepthStencilState()
            {
                EnableDepthTest = true,
                EnableDepthWrite = true,
                CompareOp = SDL.GPUCompareOp.Less
            },
            RasterizerState = new SDL.GPURasterizerState()
            {
                FillMode = SDL.GPUFillMode.Fill,
                CullMode = definition.RenderFace switch
                {
                    RenderFace.Front => SDL.GPUCullMode.Back,
                    RenderFace.Back => SDL.GPUCullMode.Front,
                    RenderFace.Both => SDL.GPUCullMode.None,
                    _ => throw new ArgumentOutOfRangeException()
                },
                FrontFace = definition.WindingOrder switch
                {
                    WindingOrder.CounterClockwise => SDL.GPUFrontFace.CounterClockwise,
                    WindingOrder.Clockwise => SDL.GPUFrontFace.Clockwise,
                    _ => throw new ArgumentOutOfRangeException()
                },
            }
        };

        Pipeline = SDL.CreateGPUGraphicsPipeline(_device, &pipelineInfo);
        
        SDL.ReleaseGPUShader(_device, pixelShader.Value);
        SDL.ReleaseGPUShader(_device, vertexShader.Value);
    }

    /// <summary>
    /// Dispose of this <see cref="Material"/>.
    /// </summary>
    public void Dispose()
    {
        SDL.ReleaseGPUGraphicsPipeline(_device, Pipeline);
    }
}