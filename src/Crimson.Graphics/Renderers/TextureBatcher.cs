using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Crimson.Graphics.Renderers.Structs;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.Renderers;

/// <summary>
/// Batches textures together efficiently for rendering.
/// </summary>
internal class TextureBatcher : IDisposable
{
    private const uint NumVertices = 4;
    private const uint NumIndices = 6;

    private readonly SDL.GPUDevice _device;

    private Vertex[] _vertices;
    private uint[] _indices;

    private uint _vBufferSize;
    private uint _iBufferSize;
    
    private SDL.GPUBuffer _vertexBuffer;
    private SDL.GPUBuffer _indexBuffer;

    private SDL.GPUTransferBuffer _transferBuffer;

    private readonly SDL.GPUGraphicsPipeline _blendPipeline;
    private readonly SDL.GPUGraphicsPipeline _noBlendPipeline;

    private readonly SDL.GPUSampler _sampler;

    private readonly List<Draw> _drawQueue;
    private readonly List<DrawList> _drawList;
    
    public unsafe TextureBatcher(SDL.GPUDevice device, SDL.GPUTextureFormat format)
    {
        _device = device;

        // Create with an initial size of 4096 sprites
        const uint initialSize = 4096;

        _vBufferSize = initialSize * NumVertices;
        _iBufferSize = initialSize * NumIndices;
        
        _vertices = new Vertex[_vBufferSize];
        _indices = new uint[_iBufferSize];

        _vertexBuffer = SdlUtils.CreateBuffer(device, SDL.GPUBufferUsageFlags.Vertex, _vBufferSize * Vertex.SizeInBytes);
        _indexBuffer = SdlUtils.CreateBuffer(device, SDL.GPUBufferUsageFlags.Index, _iBufferSize * sizeof(uint));

        _transferBuffer = SdlUtils.CreateTransferBuffer(device, SDL.GPUTransferBufferUsage.Upload,
            _vBufferSize * Vertex.SizeInBytes + _iBufferSize * sizeof(uint));

        ShaderUtils.LoadGraphicsShader(device, "Texture", out SDL.GPUShader? vertexShader, out SDL.GPUShader? pixelShader);

        SDL.GPUColorTargetDescription targetDesc = new()
        {
            Format = format,
            BlendState = SdlUtils.NonPremultipliedBlend
        };

        SDL.GPUVertexAttribute* vertexAttributes = stackalloc SDL.GPUVertexAttribute[]
        {
            new SDL.GPUVertexAttribute
                { Format = SDL.GPUVertexElementFormat.Float2, Offset = 0, BufferSlot = 0, Location = 0 },
            new SDL.GPUVertexAttribute
                { Format = SDL.GPUVertexElementFormat.Float2, Offset = 8, BufferSlot = 0, Location = 1 },
            new SDL.GPUVertexAttribute
                { Format = SDL.GPUVertexElementFormat.Float4, Offset = 16, BufferSlot = 0, Location = 2 }
        };

        SDL.GPUVertexBufferDescription vertexBufferDesc = new()
        {
            InputRate = SDL.GPUVertexInputRate.Vertex,
            InstanceStepRate = 0,
            Pitch = Vertex.SizeInBytes,
            Slot = 0
        };

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vertexShader.Value,
            FragmentShader = pixelShader.Value,
            TargetInfo = { NumColorTargets = 1, ColorTargetDescriptions = &targetDesc },
            VertexInputState = new SDL.GPUVertexInputState()
            {
                NumVertexAttributes = 3, 
                VertexAttributes = vertexAttributes,
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBufferDesc
            },
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist,
            
            DepthStencilState = new SDL.GPUDepthStencilState()
            {
                EnableDepthTest = false,
                EnableDepthWrite = false,
                EnableStencilTest = false
            },
            RasterizerState = new SDL.GPURasterizerState()
            {
                CullMode = SDL.GPUCullMode.Back,
                FrontFace = SDL.GPUFrontFace.Clockwise,
                FillMode = SDL.GPUFillMode.Fill
            }
        };

        _blendPipeline = SDL.CreateGPUGraphicsPipeline(device, &pipelineInfo).Check("Create GPU pipeline");
        targetDesc.BlendState = SdlUtils.NoBlend;
        _noBlendPipeline = SDL.CreateGPUGraphicsPipeline(device, &pipelineInfo).Check("Create GPU pipeline");
        
        SDL.ReleaseGPUShader(device, pixelShader.Value);
        SDL.ReleaseGPUShader(device, vertexShader.Value);

        SDL.GPUSamplerCreateInfo samplerInfo = new()
        {
            MinFilter = SDL.GPUFilter.Linear,
            MagFilter = SDL.GPUFilter.Linear,
            MipmapMode = SDL.GPUSamplerMipmapMode.Linear,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            MaxLod = 1000
        };

        _sampler = SDL.CreateGPUSampler(_device, &samplerInfo).Check("Create GPU sampler");

        _drawQueue = [];
        _drawList = [];
    }

    public void AddToDrawQueue(in Draw draw)
    {
        _drawQueue.Add(draw);
    }

    public unsafe bool Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, bool shouldClear, Size<int> size, CameraMatrices matrices)
    {
        // Don't even try to draw if the draw count is 0.
        if (_drawQueue.Count == 0)
            return false;
        
        uint numDraws = 0;
        uint bufferOffset = 0;
        uint lastBufferOffset = 0;
        Texture? texture = null;
        BlendMode blendMode = BlendMode.Blend;

        foreach (Draw draw in _drawQueue)
        {
            if ((draw.Texture != texture || draw.Blend != blendMode) && numDraws != 0)
            {
                _drawList.Add(new DrawList(texture, numDraws, lastBufferOffset, blendMode));
                lastBufferOffset = bufferOffset;
                numDraws = 0;
            }

            texture = draw.Texture;
            blendMode = draw.Blend;

            uint vOffset = bufferOffset * NumVertices;
            uint iOffset = bufferOffset * NumIndices;

            Size<float> texSize = texture.Size.As<float>();
            Rectangle<int> source = draw.Source;

            float texX = source.X / texSize.Width;
            float texY = source.Y / texSize.Height;
            float texW = source.Width / texSize.Width;
            float texH = source.Height / texSize.Height;

            _vertices[vOffset + 0] = new Vertex(draw.TopLeft, new Vector2T<float>(texX, texY), draw.Tint);
            _vertices[vOffset + 1] = new Vertex(draw.TopRight, new Vector2T<float>(texX + texW, texY), draw.Tint);
            _vertices[vOffset + 2] = new Vertex(draw.BottomRight, new Vector2T<float>(texX + texW, texY + texH), draw.Tint);
            _vertices[vOffset + 3] = new Vertex(draw.BottomLeft, new Vector2T<float>(texX, texY + texH), draw.Tint);

            _indices[iOffset + 0] = 0 + vOffset;
            _indices[iOffset + 1] = 1 + vOffset;
            _indices[iOffset + 2] = 3 + vOffset;
            _indices[iOffset + 3] = 1 + vOffset;
            _indices[iOffset + 4] = 2 + vOffset;
            _indices[iOffset + 5] = 3 + vOffset;

            numDraws++;
            bufferOffset++;
        }
        
        _drawList.Add(new DrawList(texture, numDraws, lastBufferOffset, blendMode));
        _drawQueue.Clear();
        
        uint numVertexBytes = bufferOffset * NumVertices * Vertex.SizeInBytes;
        uint numIndexBytes = bufferOffset * NumIndices * sizeof(uint);

        // todo reintroduce map check
        void* mapped = (void*) SDL.MapGPUTransferBuffer(_device, _transferBuffer, 1);
        
        fixed (Vertex* pVertices = _vertices)
        fixed (uint* pIndices = _indices)
        {
            Unsafe.CopyBlock(mapped, pVertices, numVertexBytes);
            Unsafe.CopyBlock((byte*) mapped + numVertexBytes, pIndices, numIndexBytes);
        }
        
        SDL.UnmapGPUTransferBuffer(_device, _transferBuffer);
        
        SdlUtils.PushDebugGroup(cb, "TextureBatcher Copy Pass");

        SDL.GPUCopyPass copyPass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUTransferBufferLocation vertexSource = new()
        {
            TransferBuffer = _transferBuffer,
            Offset = 0
        };

        SDL.GPUBufferRegion vertexDest = new()
        {
            Buffer = _vertexBuffer,
            Offset = 0,
            Size = numVertexBytes
        };
        
        SDL.UploadToGPUBuffer(copyPass, &vertexSource, &vertexDest, 0);

        SDL.GPUTransferBufferLocation indexSource = new()
        {
            TransferBuffer = _transferBuffer,
            Offset = numVertexBytes
        };

        SDL.GPUBufferRegion indexDest = new()
        {
            Buffer = _indexBuffer,
            Offset = 0,
            Size = numIndexBytes
        };
        
        SDL.UploadToGPUBuffer(copyPass, &indexSource, &indexDest, 0);
        
        SDL.EndGPUCopyPass(copyPass);
        
        SdlUtils.PopDebugGroup(cb);
        
        SdlUtils.PushDebugGroup(cb, "TextureBatcher Pass");

        SDL.GPUColorTargetInfo targetInfo = new()
        {
            Texture = colorTarget,
            ClearColor = new SDL.FColor(0.0f, 0.0f, 0.0f, 1.0f),
            LoadOp = shouldClear ? SDL.GPULoadOp.Clear : SDL.GPULoadOp.Load,
            StoreOp = SDL.GPUStoreOp.Store
        };
        
        SDL.GPURenderPass renderPass = SDL.BeginGPURenderPass(cb, &targetInfo, 1, null)
            .Check("Begin render pass");

        SDL.PushGPUVertexUniformData(cb, 0, &matrices, CameraMatrices.SizeInBytes);

        SDL.GPUViewport viewport = new SDL.GPUViewport(0, 0, size.Width, size.Height, 0, 1);
        SDL.SetGPUViewport(renderPass, &viewport);
        
        SDL.GPUBufferBinding vertexBinding = new()
        {
            Buffer = _vertexBuffer,
            Offset = 0
        };
        
        SDL.BindGPUVertexBuffers(renderPass, 0, &vertexBinding, 1);

        SDL.GPUBufferBinding indexBinding = new()
        {
            Buffer = _indexBuffer,
            Offset = 0
        };
        
        SDL.BindGPUIndexBuffer(renderPass, &indexBinding, SDL.GPUIndexElementSize.Size32bit);
        
        foreach (DrawList drawList in _drawList)
        {
            Flush(renderPass, in drawList);
        }
        
        SDL.EndGPURenderPass(renderPass);
        
        SdlUtils.PopDebugGroup(cb);
        
        _drawList.Clear();

        return true;
    }

    private unsafe void Flush(SDL.GPURenderPass pass, ref readonly DrawList drawList)
    {
        Debug.Assert(drawList.NumDraws != 0);
        Debug.Assert(drawList.Texture != null);

        SDL.GPUGraphicsPipeline pipeline = drawList.Blend switch
        {
            BlendMode.None => _noBlendPipeline,
            BlendMode.Blend => _blendPipeline,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        SDL.BindGPUGraphicsPipeline(pass, pipeline);
        
        SDL.GPUTextureSamplerBinding samplerBinding = new()
        {
            Sampler = _sampler,
            Texture = drawList.Texture.TextureHandle
        };
        
        SDL.BindGPUFragmentSamplers(pass, 0, &samplerBinding, 1);

        SDL.DrawGPUIndexedPrimitives(pass, drawList.NumDraws * NumIndices, 1, drawList.Offset * NumIndices, 0, 0);
    }
    
    public void Dispose()
    {
        SDL.ReleaseGPUSampler(_device, _sampler);
        SDL.ReleaseGPUGraphicsPipeline(_device, _blendPipeline);
        SDL.ReleaseGPUGraphicsPipeline(_device, _noBlendPipeline);
        SDL.ReleaseGPUTransferBuffer(_device, _transferBuffer);
        SDL.ReleaseGPUBuffer(_device, _indexBuffer);
        SDL.ReleaseGPUBuffer(_device, _vertexBuffer);
    }

    private readonly struct Vertex
    {
        public const uint SizeInBytes = 32;
        
        public readonly Vector2T<float> Position;
        public readonly Vector2T<float> TexCoord;
        public readonly Color Tint;

        public Vertex(Vector2T<float> position, Vector2T<float> texCoord, Color tint)
        {
            Position = position;
            TexCoord = texCoord;
            Tint = tint;
        }
    }

    private readonly struct DrawList(Texture? texture, uint numDraws, uint offset, BlendMode blend)
    {
        public readonly Texture? Texture = texture;
        public readonly uint NumDraws = numDraws;
        public readonly uint Offset = offset;
        public readonly BlendMode Blend = blend;
    }

    public readonly struct Draw
    {
        public readonly Texture Texture;
        public readonly Vector2T<float> TopLeft;
        public readonly Vector2T<float> TopRight;
        public readonly Vector2T<float> BottomLeft;
        public readonly Vector2T<float> BottomRight;
        public readonly Rectangle<int> Source;
        public readonly Color Tint;
        public readonly BlendMode Blend;

        public Draw(Texture texture, Vector2T<float> topLeft, Vector2T<float> topRight, Vector2T<float> bottomLeft,
            Vector2T<float> bottomRight, Rectangle<int> source, Color tint, BlendMode blend)
        {
            Texture = texture;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Source = source;
            Tint = tint;
            Blend = blend;
        }
    }
}