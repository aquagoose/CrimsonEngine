using System.Numerics;
using System.Runtime.InteropServices;
using Crimson.Core;
using Crimson.Graphics.Materials;
using Crimson.Graphics.Rendering.Structs;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.Rendering;

internal sealed class ForwardPlusRenderer : IDisposable
{
    private readonly RenderContext _context;
    
    private readonly List<Draw> _opaques;
    private readonly Comparison<Draw> _ftbComparison;
    private Camera _currentCamera;

    private readonly SDL.GPUSampler _temporarySampler;
    
    public unsafe ForwardPlusRenderer(RenderContext context)
    {
        _context = context;
        
        _opaques = [];
        _ftbComparison = CompareDrawsFTB;

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
        _temporarySampler = SDL.CreateGPUSampler(_context.Device, &samplerInfo).Check("Create sampler");
    }

    public void Clear()
    {
        _opaques.Clear();
    }

    public void AddToDrawQueue(ref readonly Draw draw)
    {
        _opaques.Add(draw);
    }
    
    public unsafe void Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, SDL.GPUTexture depthTarget,
        Size<uint> viewportSize, ref readonly Camera camera, ref ClearInfo clear)
    {
        _currentCamera = camera;
        // sort opaques front to back, and everything else back to front
        _opaques.Sort(_ftbComparison);

        SceneInfo scene = new SceneInfo(camera);
        SDL.PushGPUVertexUniformData(cb, 0, (nint) (&scene), (uint) sizeof(SceneInfo));
        
        SDL.GPUColorTargetInfo colorTargetInfo = new()
        {
            Texture = colorTarget,
            ClearColor = clear.Color.ToFColor(),
            LoadOp = clear.HasCleared ? SDL.GPULoadOp.Load : SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPUDepthStencilTargetInfo depthTargetInfo = new()
        {
            Texture = depthTarget,
            ClearDepth = 1,
            LoadOp = clear.HasCleared ? SDL.GPULoadOp.Load : SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass pass = SDL.BeginGPURenderPass(cb, &colorTargetInfo, 1, &depthTargetInfo).Check("Begin render pass");

        ReadOnlySpan<Draw> opaques = CollectionsMarshal.AsSpan(_opaques);
        for (int i = 0; i < opaques.Length; i++)
        {
            ref readonly Draw draw = ref opaques[i];
            Renderable renderable = draw.Renderable;
            Material material = renderable.Material;
            
            fixed (Matrix4x4* worldMatrix = &draw.WorldMatrix)
                SDL.PushGPUVertexUniformData(cb, 1, (nint) worldMatrix, (uint) sizeof(Matrix4x4));
            
            SDL.BindGPUGraphicsPipeline(pass, material.Pipeline);
            SDL.BindGPUFragmentTextures(pass, 0, material.Textures, _temporarySampler);
            SDL.BindGPUVertexBuffer(pass, 0, renderable.VertexBuffer);
            SDL.BindGPUIndexBuffer(pass, renderable.IndexBuffer, SDL.GPUIndexElementSize.Size32bit);

            SDL.DrawGPUIndexedPrimitives(pass, renderable.NumElements, 1, 0, 0, 0);
        }
        
        SDL.EndGPURenderPass(pass);
        
        // set this to true as the renderer will always clear if false
        clear.HasCleared = true;
    }

    private int CompareDrawsFTB(Draw draw1, Draw draw2)
    {
        return Vector3.Distance(draw1.WorldMatrix.Translation, _currentCamera.Position)
            .CompareTo(Vector3.Distance(draw2.WorldMatrix.Translation, _currentCamera.Position));
    }

    public struct Draw
    {
        public Renderable Renderable;
        public Matrix4x4 WorldMatrix;

        public Draw(Renderable renderable, Matrix4x4 worldMatrix)
        {
            Renderable = renderable;
            WorldMatrix = worldMatrix;
        }
    }

    public void Dispose()
    {
        SDL.ReleaseGPUSampler(_context.Device, _temporarySampler);
    }
}