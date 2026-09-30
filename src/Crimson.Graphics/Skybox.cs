using System.Diagnostics;
using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Graphics.Primitives;
using Crimson.Graphics.Rendering.Structs;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// A skybox is a 6-sided textured cubemap that can provide an environment backdrop.
/// </summary>
public class Skybox : IDisposable
{
    private readonly RenderContext _context;
    private readonly SDL.GPUTexture _cubemap;

    private readonly SDL.GPUBuffer _vertexBuffer;
    private readonly SDL.GPUBuffer _indexBuffer;
    private readonly SDL.GPUGraphicsPipeline _pipeline;
    private readonly SDL.GPUSampler _sampler;
    
    public unsafe Skybox(Bitmap right, Bitmap left, Bitmap top, Bitmap bottom, Bitmap front, Bitmap back)
    {
        _context = Renderer.Context;
        // todo add some debug assertions for checking sizes and formats are equal
        Size<uint> size = right.Size;
        PixelFormat format = right.Format;

        SDL.GPUTextureCreateInfo textureInfo = new()
        {
            Type = SDL.GPUTextureType.TypeCube,
            Width = size.Width,
            Height = size.Height,
            LayerCountOrDepth = 6,
            Format = format.ToSDL(),
            NumLevels = 1, // todo skybox mipmaps
            SampleCount = SDL.GPUSampleCount.Count1,
            Usage = SDL.GPUTextureUsageFlags.Sampler
        };
        
        Logger.Trace("Creating cubemap texture.");
        _cubemap = SDL.CreateGPUTexture(_context.Device, &textureInfo).Check("Create cubemap");
        
        Cube cube = new Cube();
        uint vertexSize = (uint) (cube.Vertices.Length * sizeof(Vertex));
        uint indexSize = (uint) (cube.Indices.Length * sizeof(uint));
        
        _vertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, vertexSize);
        _indexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, indexSize);
        
        uint imageDataSize = size.Width * size.Height * format.BytesPerPixel;
        uint bufferDataSize = vertexSize + indexSize;
        uint totalDataSize = imageDataSize * 6 + bufferDataSize; // multiply by 6, one for each side of the cube
        SDL.GPUTransferBuffer transBuffer = _context.GetUploadBuffer(totalDataSize, out uint offset, out bool cycle);
        nint mapped = SDL.MapGPUTransferBuffer(_context.Device, transBuffer, cycle).Check("Map transfer buffer");
        
        // to minimise copy passes, we copy eeeverything over into a single transfer buffer,
        // so that's all 6 images, and the vertex + index data for the cube
        fixed (void* pRight = right.Data)
        fixed (void* pLeft = left.Data)
        fixed (void* pTop = top.Data)
        fixed (void* pBottom = bottom.Data)
        fixed (void* pFront = front.Data)
        fixed (void* pBack = back.Data)
        fixed (Vertex* pVertices = cube.Vertices)
        fixed (uint* pIndices = cube.Indices)
        {
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 0)), pRight, imageDataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 1)), pLeft, imageDataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 2)), pTop, imageDataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 3)), pBottom, imageDataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 4)), pFront, imageDataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 5)), pBack, imageDataSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 6)), pVertices, vertexSize);
            Unsafe.CopyBlock((byte*) (mapped + offset + (imageDataSize * 6) + vertexSize), pIndices, indexSize);
        }
        
        SDL.UnmapGPUTransferBuffer(_context.Device, transBuffer);

        Logger.Trace($"Uploading {totalDataSize / 1024}KiB data to cubemap.");
        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(_context.Device).Check("Acquire command buffer");

        SDL.GPUCopyPass pass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        // copy images
        {
            SDL.GPUTextureTransferInfo src = new()
            {
                TransferBuffer = transBuffer,
                Offset = offset,
                PixelsPerRow = size.Width,
                RowsPerLayer = size.Height
            };

            SDL.GPUTextureRegion dest = new()
            {
                Texture = _cubemap,
                X = 0,
                Y = 0,
                Z = 0,
                W = size.Width,
                H = size.Height,
                D = 1,
                Layer = 0,
                MipLevel = 0
            };

            // copy over the image data to each layer.
            for (uint i = 0; i < 6; i++)
            {
                src.Offset = offset + (imageDataSize * i);
                dest.Layer = i;
                SDL.UploadToGPUTexture(pass, &src, &dest, false);
            }
        }

        // copy buffers
        {
            SDL.GPUTransferBufferLocation src = new()
            {
                TransferBuffer = transBuffer,
                Offset = offset + (imageDataSize * 6) // large offset as the vertex data is at the very end of the buffer
            };

            SDL.GPUBufferRegion dest = new()
            {
                Buffer = _vertexBuffer,
                Offset = 0,
                Size = vertexSize
            };
            
            // copy vertices
            SDL.UploadToGPUBuffer(pass, &src, &dest, false);
            
            // copy indices
            src.Offset += vertexSize;
            dest.Buffer = _indexBuffer;
            dest.Size = indexSize;
            SDL.UploadToGPUBuffer(pass, &src, &dest, false);
        }

        SDL.EndGPUCopyPass(pass);
        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");

        SDL.GPUShader vertexShader = _context.CreateShader(SDL.GPUShaderStage.Vertex, "Environment/Skybox");
        SDL.GPUShader pixelShader = _context.CreateShader(SDL.GPUShaderStage.Fragment, "Environment/Skybox");

        SDL.GPUColorTargetDescription targetDesc = new()
        {
            Format = Renderer.MainRendererTargetFormat,
            BlendState = SDLUtils.NoBlend
        };

        SDL.GPUVertexBufferDescription vertexBuffer =
            new SDL.GPUVertexBufferDescription(0, (uint) sizeof(Vertex), SDL.GPUVertexInputRate.Vertex, 0);

        // we're only using the position attribute in the shader, so we only need to pass it in.
        SDL.GPUVertexAttribute inputLayout = new SDL.GPUVertexAttribute(0, 0, SDL.GPUVertexElementFormat.Float3, 0);

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vertexShader,
            FragmentShader = pixelShader,
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist,
            TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo
            {
                NumColorTargets = 1,
                ColorTargetDescriptions = &targetDesc,
                HasDepthStencilTarget = true,
                DepthStencilFormat = Renderer.MainRendererDepthFormat
            },
            VertexInputState = new SDL.GPUVertexInputState
            {
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBuffer,
                NumVertexAttributes = 1,
                VertexAttributes = &inputLayout
            },
            DepthStencilState = new SDL.GPUDepthStencilState
            {
                EnableDepthTest = true,
                EnableDepthWrite = false, // we don't want to write to the depth buffer!!
                CompareOp = SDL.GPUCompareOp.LessOrEqual
            },
            RasterizerState = new SDL.GPURasterizerState
            {
                CullMode = SDL.GPUCullMode.Front,
                FrontFace = SDL.GPUFrontFace.CounterClockwise,
                FillMode = SDL.GPUFillMode.Fill
            },
            MultisampleState = new SDL.GPUMultisampleState
            {
                SampleCount = SDL.GPUSampleCount.Count1
            }
        };
        
        Logger.Trace("Creating skybox pipeline.");
        _pipeline = SDL.CreateGPUGraphicsPipeline(_context.Device, &pipelineInfo).Check("Create skybox pipeline");

        SDL.ReleaseGPUShader(_context.Device, pixelShader);
        SDL.ReleaseGPUShader(_context.Device, vertexShader);
        
        SDL.GPUSamplerCreateInfo samplerInfo = new()
        {
            MinFilter = SDL.GPUFilter.Linear,
            MagFilter = SDL.GPUFilter.Linear,
            MipmapMode = SDL.GPUSamplerMipmapMode.Linear,
            // use clamp to ensure skybox looks correct at the cube edge, in case of sampling artifacts
            // using repeat mode can look strange sometimes
            AddressModeU = SDL.GPUSamplerAddressMode.ClampToEdge,
            AddressModeV = SDL.GPUSamplerAddressMode.ClampToEdge,
            AddressModeW = SDL.GPUSamplerAddressMode.ClampToEdge,
            MinLod = 0,
            MaxLod = float.MaxValue
        };

        Logger.Trace("Creating sampler.");
        _sampler = SDL.CreateGPUSampler(_context.Device, &samplerInfo).Check("Create sampler");
    }

    public Skybox(string right, string left, string top, string bottom, string front, string back) : this(
        new Bitmap(right), new Bitmap(left), new Bitmap(top), new Bitmap(bottom), new Bitmap(front),
        new Bitmap(back)) { }

    internal unsafe void Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, SDL.GPUTexture depthTarget,
        Size<uint> viewportSize, ref readonly Camera camera, ref ClearInfo clear)
    {
        // todo reuse the scene data from the main renderer, without just ASSUMING it set the scene data
        SceneInfo scene = new SceneInfo(new ShaderCamera(in camera));
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
            ClearDepth = 1.0f,
            LoadOp = clear.HasCleared ? SDL.GPULoadOp.Load : SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass pass = SDL.BeginGPURenderPass(cb, &colorTargetInfo, 1, &depthTargetInfo)
            .Check("Begin render pass");

        SDL.GPUTextureSamplerBinding textureBinding = new SDL.GPUTextureSamplerBinding(_cubemap, _sampler);
        
        SDL.BindGPUGraphicsPipeline(pass, _pipeline);
        SDL.BindGPUFragmentSamplers(pass, 0, &textureBinding, 1);
        SDL.BindGPUVertexBuffer(pass, 0, _vertexBuffer);
        SDL.BindGPUIndexBuffer(pass, _indexBuffer, SDL.GPUIndexElementSize.Size32bit);
        SDL.DrawGPUIndexedPrimitives(pass, 36, 1, 0, 0, 0); // 36 indices, 6 indices per cube side
        
        SDL.EndGPURenderPass(pass);

        // will always clear so just set this value to true
        clear.HasCleared = true;
    }

    public void Dispose()
    {
        SDL.ReleaseGPUSampler(_context.Device, _sampler);
        SDL.ReleaseGPUGraphicsPipeline(_context.Device, _pipeline);
        SDL.ReleaseGPUBuffer(_context.Device, _indexBuffer);
        SDL.ReleaseGPUBuffer(_context.Device, _vertexBuffer);
        SDL.ReleaseGPUTexture(_context.Device, _cubemap);
    }
}