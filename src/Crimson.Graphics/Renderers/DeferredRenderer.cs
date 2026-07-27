using System.Numerics;
using Crimson.Graphics.Renderers.Structs;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.Renderers;

internal class DeferredRenderer : IDisposable
{
    private const SDL.GPUTextureFormat GBufferFormat = SDL.GPUTextureFormat.R32g32b32a32Float;
    
    private readonly SDL.GPUDevice _device;
    
    private SDL.GPUTexture _albedoTexture;
    private SDL.GPUTexture _positionTexture;
    private SDL.GPUTexture _normalTexture;
    private SDL.GPUTexture _metallicRoughnessTexture;

    private readonly SDL.GPUGraphicsPipeline _passPipeline;
    private readonly SDL.GPUSampler _passSampler;

    private readonly List<DrawCall> _drawQueue;

    public Texture[] DebugTextures;
    
    public unsafe DeferredRenderer(SDL.GPUDevice device, Size<int> size, SDL.GPUTextureFormat outFormat)
    {
        _device = device;

        _albedoTexture = CreateGBufferTexture(_device, size, GBufferFormat);
        SDL.SetGPUTextureName(_device, _albedoTexture, "Albedo GBuffer");
        _positionTexture = CreateGBufferTexture(_device, size, GBufferFormat);
        SDL.SetGPUTextureName(_device, _positionTexture, "Position GBuffer");
        _normalTexture = CreateGBufferTexture(_device, size, GBufferFormat);
        SDL.SetGPUTextureName(_device, _normalTexture, "Normal GBuffer");
        _metallicRoughnessTexture = CreateGBufferTexture(_device, size, GBufferFormat);
        SDL.SetGPUTextureName(_device, _metallicRoughnessTexture, "Metallic-Roughness-Occlusion GBuffer");

        DebugTextures =
        [
            new Texture(_albedoTexture, size, "Albedo"),
            new Texture(_positionTexture, size, "Position"),
            new Texture(_normalTexture, size, "Normals"),
            new Texture(_metallicRoughnessTexture, size, "Metallic-Roughness-Occlusion")
        ];

        ShaderUtils.LoadGraphicsShader(device, "Deferred/DeferredPass", out SDL.GPUShader? passVtx, out SDL.GPUShader? passPxl);

        SDL.GPUColorTargetDescription targetDesc = new()
        {
            Format = outFormat
        };

        SDL.GPUGraphicsPipelineCreateInfo passPipelineInfo = new()
        {
            VertexShader = passVtx.Value,
            FragmentShader = passPxl.Value,
            TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo()
            {
                NumColorTargets = 1,
                ColorTargetDescriptions = &targetDesc
            },
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist
        };

        _passPipeline = SDL.CreateGPUGraphicsPipeline(_device, &passPipelineInfo).Check("Create graphics pipeline");
        
        SDL.ReleaseGPUShader(_device, passPxl.Value);
        SDL.ReleaseGPUShader(_device, passVtx.Value);

        SDL.GPUSamplerCreateInfo samplerInfo = new()
        {
            MinFilter = SDL.GPUFilter.Linear,
            MagFilter = SDL.GPUFilter.Linear,
            MipmapMode = SDL.GPUSamplerMipmapMode.Linear,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            MaxLod = 1000
        };

        _passSampler = SDL.CreateGPUSampler(_device, &samplerInfo).Check("Create sampler");

        _drawQueue = [];
    }

    public void AddToQueue(Renderable renderable, Matrix4x4 worldMatrix)
    {
        _drawQueue.Add(new DrawCall(renderable, worldMatrix));
    }

    public unsafe bool Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture compositeTarget, SDL.GPUTexture depthTexture, CameraMatrices camera)
    {
        // Don't bother rendering if there is nothing to draw.
        if (_drawQueue.Count == 0)
            return false;
        
        SDL.PushGPUVertexUniformData(cb, 0, &camera, CameraMatrices.SizeInBytes);
        
        #region GBuffer Pass
        
        SdlUtils.PushDebugGroup(cb, "GBuffer Pass");

        SDL.GPUColorTargetInfo* gBufferTargets = stackalloc SDL.GPUColorTargetInfo[]
        {
            new SDL.GPUColorTargetInfo
            {
                Texture = _albedoTexture, ClearColor = new SDL.FColor(), LoadOp = SDL.GPULoadOp.Clear,
                StoreOp = SDL.GPUStoreOp.Store
            },
            new SDL.GPUColorTargetInfo
            {
                Texture = _positionTexture, ClearColor = new SDL.FColor(), LoadOp = SDL.GPULoadOp.Clear,
                StoreOp = SDL.GPUStoreOp.Store
            },
            new SDL.GPUColorTargetInfo
            {
                Texture = _normalTexture, ClearColor = new SDL.FColor(), LoadOp = SDL.GPULoadOp.Clear,
                StoreOp = SDL.GPUStoreOp.Store
            },
            new SDL.GPUColorTargetInfo
            {
                Texture = _metallicRoughnessTexture, ClearColor = new SDL.FColor(), LoadOp = SDL.GPULoadOp.Clear,
                StoreOp = SDL.GPUStoreOp.Store
            }
        };

        SDL.GPUDepthStencilTargetInfo depthInfo = new()
        {
            Texture = depthTexture,
            ClearDepth = 1.0f,
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass gBufferPass = SDL.BeginGPURenderPass(cb, gBufferTargets, 4, &depthInfo)
            .Check("Begin gbuffer pass");

        // TODO: Position field in CameraMatrices
        if (!Matrix4x4.Invert(camera.View, out Matrix4x4 invView))
            invView = Matrix4x4.Identity;
        Vector3 cameraPosition = invView.Translation;
        
        IOrderedEnumerable<DrawCall> frontToBack = _drawQueue.OrderBy(renderable =>
            Vector3.Distance(cameraPosition, renderable.WorldMatrix.Translation));

        const int numSamplerBindings = 6;
        SDL.GPUTextureSamplerBinding* bindings = stackalloc SDL.GPUTextureSamplerBinding[numSamplerBindings];
        
        foreach (DrawCall draw in frontToBack)
        {
            Renderable renderable = draw.Renderable;
            
            Matrix4x4 world = draw.WorldMatrix;
            SDL.PushGPUVertexUniformData(cb, 1, &world, 64);

            MaterialProperties matProps = draw.MaterialProperties;
            SDL.PushGPUFragmentUniformData(cb, 0, &matProps, (uint) sizeof(MaterialProperties));

            // TODO: Have a sampler per material.
            bindings[0] = new SDL.GPUTextureSamplerBinding
            {
                Texture = renderable.Material.Albedo.TextureHandle,
                Sampler = _passSampler
            };
            bindings[1] = new SDL.GPUTextureSamplerBinding
            {
                Texture = renderable.Material.Normal.TextureHandle,
                Sampler = _passSampler,
            };
            bindings[2] = new SDL.GPUTextureSamplerBinding
            {
                Texture = renderable.Material.Metallic.TextureHandle,
                Sampler = _passSampler,
            };
            bindings[3] = new SDL.GPUTextureSamplerBinding
            {
                Texture = renderable.Material.Roughness.TextureHandle,
                Sampler = _passSampler,
            };
            bindings[4] = new SDL.GPUTextureSamplerBinding
            {
                Texture = renderable.Material.Occlusion.TextureHandle,
                Sampler = _passSampler,
            };
            bindings[5] = new SDL.GPUTextureSamplerBinding
            {
                Texture = renderable.Material.Emission.TextureHandle,
                Sampler = _passSampler,
            };

            SDL.BindGPUFragmentSamplers(gBufferPass, 0, bindings, numSamplerBindings);

            SDL.BindGPUGraphicsPipeline(gBufferPass, renderable.Material.Pipeline);
            
            SDL.GPUBufferBinding vertexBinding = new()
            {
                Buffer = renderable.VertexBuffer,
                Offset = 0
            };
            
            SDL.BindGPUVertexBuffers(gBufferPass, 0, &vertexBinding, 1);

            SDL.GPUBufferBinding indexBinding = new()
            {
                Buffer = renderable.IndexBuffer,
                Offset = 0
            };
            
            SDL.BindGPUIndexBuffer(gBufferPass, &indexBinding, SDL.GPUIndexElementSize.Size32bit);

            SDL.DrawGPUIndexedPrimitives(gBufferPass, renderable.NumIndices, 1, 0, 0, 0);
        }
        
        SDL.EndGPURenderPass(gBufferPass);
        
        SdlUtils.PopDebugGroup(cb);
        
        #endregion

        #region Lighting Pass
        
        SdlUtils.PushDebugGroup(cb, "Deferred Lighting Pass");

        SDL.GPUColorTargetInfo compositeInfo = new()
        {
            Texture = compositeTarget,
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store,
            ClearColor = new SDL.FColor(0.0f, 0.0f, 0.0f, 1.0f)
        };

        SDL.GPURenderPass lightingPass = SDL.BeginGPURenderPass(cb, &compositeInfo, 1, null)
            .Check("Begin lighting pass");
        
        SDL.PushGPUFragmentUniformData(cb, 0, &camera, CameraMatrices.SizeInBytes);
        
        SDL.BindGPUGraphicsPipeline(lightingPass, _passPipeline);

        SDL.GPUTextureSamplerBinding* passBindings = stackalloc SDL.GPUTextureSamplerBinding[]
        {
            new SDL.GPUTextureSamplerBinding
            {
                Texture = _albedoTexture,
                Sampler = _passSampler
            },
            new SDL.GPUTextureSamplerBinding
            {
                Texture = _positionTexture,
                Sampler = _passSampler
            },
            new SDL.GPUTextureSamplerBinding
            {
                Texture = _normalTexture,
                Sampler = _passSampler
            },
            new SDL.GPUTextureSamplerBinding
            {
                Texture = _metallicRoughnessTexture,
                Sampler = _passSampler
            }
        };

        SDL.BindGPUFragmentSamplers(lightingPass, 0, passBindings, 4);
        
        SDL.DrawGPUPrimitives(lightingPass, 6, 1, 0, 0);
        
        SDL.EndGPURenderPass(lightingPass);
        
        SdlUtils.PopDebugGroup(cb);

        #endregion
        
        // TODO: Multi camera support.
        _drawQueue.Clear();

        return true;
    }

    public void Resize(Size<int> newSize)
    {
        SDL.ReleaseGPUTexture(_device, _albedoTexture);
        SDL.ReleaseGPUTexture(_device, _positionTexture);
        SDL.ReleaseGPUTexture(_device, _normalTexture);
        SDL.ReleaseGPUTexture(_device, _metallicRoughnessTexture);

        _albedoTexture = CreateGBufferTexture(_device, newSize, GBufferFormat);
        _positionTexture = CreateGBufferTexture(_device, newSize, GBufferFormat);
        _normalTexture = CreateGBufferTexture(_device, newSize, GBufferFormat);
        _metallicRoughnessTexture = CreateGBufferTexture(_device, newSize, GBufferFormat);
        
        DebugTextures =
        [
            new Texture(_albedoTexture, newSize, "Albedo"),
            new Texture(_positionTexture, newSize, "Position"),
            new Texture(_normalTexture, newSize, "Normals"),
            new Texture(_metallicRoughnessTexture, newSize, "Metallic-Roughness-Occlusion")
        ];
    }
    
    public void Dispose()
    {
        SDL.ReleaseGPUSampler(_device, _passSampler);
        SDL.ReleaseGPUGraphicsPipeline(_device, _passPipeline);
        SDL.ReleaseGPUTexture(_device, _metallicRoughnessTexture);
        SDL.ReleaseGPUTexture(_device, _normalTexture);
        SDL.ReleaseGPUTexture(_device, _positionTexture);
        SDL.ReleaseGPUTexture(_device, _albedoTexture);
    }

    private static SDL.GPUTexture CreateGBufferTexture(SDL.GPUDevice device, Size<int> size, SDL.GPUTextureFormat format)
    {
        return SdlUtils.CreateTexture2D(device, (uint) size.Width, (uint) size.Height, format, 1,
            SDL.GPUTextureUsageFlags.Sampler | SDL.GPUTextureUsageFlags.ColorTarget);
    }
}