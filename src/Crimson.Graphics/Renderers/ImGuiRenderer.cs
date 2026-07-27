using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Graphics.Utils;
using Crimson.Math;
using Hexa.NET.ImGui;
using piko.SDL3;

namespace Crimson.Graphics.Renderers;

internal sealed class ImGuiRenderer : IDisposable
{
    private readonly SDL.GPUDevice _device;
    
    private readonly ImGuiContextPtr _imguiContext;

    private uint _vBufferSize;
    private uint _iBufferSize;

    private SDL.GPUBuffer _vertexBuffer;
    private SDL.GPUBuffer _indexBuffer;
    private SDL.GPUTransferBuffer _transferBuffer;

    private SDL.GPUGraphicsPipeline _pipeline;

    private SDL.GPUTexture? _texture;
    private SDL.GPUSampler _sampler;

    public ImGuiContextPtr Context => _imguiContext;
    
    public unsafe ImGuiRenderer(SDL.GPUDevice device, Size<int> size, SDL.GPUTextureFormat outFormat, RendererOptions.ImGuiInfo info)
    {
        _device = device;

        _imguiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(_imguiContext);

        _vBufferSize = 5000;
        _iBufferSize = 10000;

        uint vBufferSizeBytes = (uint) (_vBufferSize * sizeof(ImDrawVert));
        uint iBufferSizeBytes = _iBufferSize * sizeof(uint);
        
        _vertexBuffer = SdlUtils.CreateBuffer(_device, SDL.GPUBufferUsageFlags.Vertex, vBufferSizeBytes);
        _indexBuffer = SdlUtils.CreateBuffer(_device, SDL.GPUBufferUsageFlags.Index, iBufferSizeBytes);

        _transferBuffer = SdlUtils.CreateTransferBuffer(_device, SDL.GPUTransferBufferUsage.Upload,
            vBufferSizeBytes + iBufferSizeBytes);

        ShaderUtils.LoadGraphicsShader(_device, "Debug/ImGui", out SDL.GPUShader? vertexShader, out SDL.GPUShader? pixelShader);

        SDL.GPUColorTargetDescription targetDesc = new()
        {
            Format = outFormat,
            BlendState = SdlUtils.NonPremultipliedBlend
        };

        SDL.GPUVertexBufferDescription vertexBufferDesc = new()
        {
            InputRate = SDL.GPUVertexInputRate.Vertex,
            Slot = 0,
            InstanceStepRate = 0,
            Pitch = (uint) sizeof(ImDrawVert)
        };

        SDL.GPUVertexAttribute* vertexAttributes = stackalloc SDL.GPUVertexAttribute[]
        {
            new SDL.GPUVertexAttribute // Position
                { Format = SDL.GPUVertexElementFormat.Float2, Offset = 0, BufferSlot = 0, Location = 0 },
            new SDL.GPUVertexAttribute // TexCoord
                { Format = SDL.GPUVertexElementFormat.Float2, Offset = 8, BufferSlot = 0, Location = 1 },
            new SDL.GPUVertexAttribute // Color
                { Format = SDL.GPUVertexElementFormat.Ubyte4Norm, Offset = 16, BufferSlot = 0, Location = 2 }
        };

        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            VertexShader = vertexShader.Value,
            FragmentShader = pixelShader.Value,
            TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo()
            {
                NumColorTargets = 1,
                ColorTargetDescriptions = &targetDesc
            },
            VertexInputState = new SDL.GPUVertexInputState()
            {
                NumVertexBuffers = 1,
                VertexBufferDescriptions = &vertexBufferDesc,
                NumVertexAttributes = 3,
                VertexAttributes = vertexAttributes
            },
            PrimitiveType = SDL.GPUPrimitiveType.Trianglelist
        };

        _pipeline = SDL.CreateGPUGraphicsPipeline(_device, &pipelineInfo).Check("Create pipeline");
        
        SDL.ReleaseGPUShader(_device, pixelShader.Value);
        SDL.ReleaseGPUShader(_device, vertexShader.Value);

        SDL.GPUSamplerCreateInfo samplerInfo = new()
        {
            MinFilter = SDL.GPUFilter.Linear,
            MagFilter = SDL.GPUFilter.Linear,
            MipmapMode = SDL.GPUSamplerMipmapMode.Linear,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            MaxLod = 1000
        };

        _sampler = SDL.CreateGPUSampler(_device, &samplerInfo).Check("Create sampler");

        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(size.Width, size.Height);
        io.BackendFlags = ImGuiBackendFlags.RendererHasVtxOffset;
        io.IniFilename = null;
        io.LogFilename = null;
        
        if (info.Font != null)
        {
            Debug.Assert(info.FontSize != null);
            string path = Content.Content.GetFullyQualifiedName(info.Font);
            io.Fonts.AddFontFromFileTTF(path, info.FontSize.Value);
        }
        else
            io.Fonts.AddFontDefault();

        RecreateFontTexture();
        
        ImGui.NewFrame();
    }

    public unsafe bool Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, bool shouldClear)
    {
        ImGui.SetCurrentContext(_imguiContext);
        
        ImGui.Render();
        ImDrawDataPtr drawData = ImGui.GetDrawData();

        // Don't bother rendering if there is nothing to draw.
        if (drawData.CmdListsCount == 0)
        {
            ImGui.NewFrame();
            return false;
        }
        
        SdlUtils.PushDebugGroup(cb, "ImGUI Buffer Copy");

        bool hasResizedBuffer = false;
        
        if (drawData.TotalVtxCount >= _vBufferSize)
        {
            Logger.Trace("Recreate vertex buffer.");
            SDL.ReleaseGPUBuffer(_device, _vertexBuffer);
            _vBufferSize = (uint) drawData.TotalVtxCount + 5000;
            _vertexBuffer = SdlUtils.CreateBuffer(_device, SDL.GPUBufferUsageFlags.Vertex,
                (uint) (_vBufferSize * sizeof(ImDrawVert)));
            hasResizedBuffer = true;
        }
        
        if (drawData.TotalIdxCount >= _iBufferSize)
        {
            Logger.Trace("Recreate index buffer.");
            SDL.ReleaseGPUBuffer(_device, _indexBuffer);
            _iBufferSize = (uint) drawData.TotalIdxCount + 10000;
            _indexBuffer = SdlUtils.CreateBuffer(_device, SDL.GPUBufferUsageFlags.Index, _iBufferSize * sizeof(uint));
            hasResizedBuffer = true;
        }

        if (hasResizedBuffer)
        {
            uint vBufferSizeBytes = (uint) (_vBufferSize * sizeof(ImDrawVert));
            uint iBufferSizeBytes = _iBufferSize * sizeof(uint);
            
            Logger.Trace("Recreate transfer buffer.");
            SDL.ReleaseGPUTransferBuffer(_device, _transferBuffer);
            _transferBuffer = SdlUtils.CreateTransferBuffer(_device, SDL.GPUTransferBufferUsage.Upload,
                vBufferSizeBytes + iBufferSizeBytes);
        }

        uint vertexOffset = 0;
        uint indexOffset = 0;

        // todo reintroduce check
        void* mappedPtr = SDL.MapGPUTransferBuffer(_device, _transferBuffer, 1);
        
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[i];

            uint vertexSize = (uint) (cmdList.VtxBuffer.Size * sizeof(ImDrawVert));
            uint indexSize = (uint) (cmdList.IdxBuffer.Size * sizeof(ushort));

            Unsafe.CopyBlock((byte*) mappedPtr + vertexOffset, cmdList.VtxBuffer.Data, vertexSize);
            Unsafe.CopyBlock((byte*) mappedPtr + (_vBufferSize * sizeof(ImDrawVert)) + indexOffset,
                cmdList.IdxBuffer.Data, indexSize);

            vertexOffset += vertexSize;
            indexOffset += indexSize;
        }
        
        SDL.UnmapGPUTransferBuffer(_device, _transferBuffer);

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
            Size = vertexOffset
        };
        
        SDL.UploadToGPUBuffer(copyPass, &vertexSource, &vertexDest, 0);

        SDL.GPUTransferBufferLocation indexSource = new()
        {
            TransferBuffer = _transferBuffer,
            Offset = _vBufferSize * (uint) sizeof(ImDrawVert)
        };

        SDL.GPUBufferRegion indexDest = new()
        {
            Buffer = _indexBuffer,
            Offset = 0,
            Size = indexOffset
        };
        
        SDL.UploadToGPUBuffer(copyPass, &indexSource, &indexDest, 0);
        
        SDL.EndGPUCopyPass(copyPass);
        
        SdlUtils.PopDebugGroup(cb);

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(drawData.DisplayPos.X,
            drawData.DisplayPos.X + drawData.DisplaySize.X, drawData.DisplayPos.Y + drawData.DisplaySize.Y,
            drawData.DisplayPos.Y, -1, 1);
        
        SDL.PushGPUVertexUniformData(cb, 0, &projection, 64);
        
        SdlUtils.PushDebugGroup(cb, "ImGUI Pass");

        SDL.GPUColorTargetInfo targetInfo = new()
        {
            Texture = colorTarget,
            ClearColor = new SDL.FColor(0.0f, 0.0f, 0.0f, 1.0f),
            LoadOp = shouldClear ? SDL.GPULoadOp.Clear : SDL.GPULoadOp.Load,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass renderPass = SDL.BeginGPURenderPass(cb, &targetInfo, 1, null)
            .Check("Begin render pass");
        
        SDL.BindGPUGraphicsPipeline(renderPass, _pipeline);

        SDL.GPUViewport viewport = new()
        {
            X = drawData.DisplayPos.X,
            Y = drawData.DisplayPos.Y,
            W = drawData.DisplaySize.X,
            H = drawData.DisplaySize.Y,
            MinDepth = 0,
            MaxDepth = 1
        };
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

        SDL.BindGPUIndexBuffer(renderPass, &indexBinding, SDL.GPUIndexElementSize.Size16bit);

        vertexOffset = 0;
        indexOffset = 0;
        Vector2 clipOff = drawData.DisplayPos;

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[i];

            for (int j = 0; j < cmdList.CmdBuffer.Size; j++)
            {
                ImDrawCmd drawCmd = cmdList.CmdBuffer[j];
                
                if (drawCmd.UserCallback != null)
                    continue;

                SDL.GPUTexture texture = _texture!.Value;

                if (drawCmd.TextureId != ImTextureID.Null)
                    texture = new SDL.GPUTexture((nint) drawCmd.TextureId.Handle);
                
                Vector2 clipMin = new Vector2(drawCmd.ClipRect.X - clipOff.X, drawCmd.ClipRect.Y - clipOff.Y);
                Vector2 clipMax = new Vector2(drawCmd.ClipRect.Z - clipOff.X, drawCmd.ClipRect.W - clipOff.Y);
                
                if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                    continue;

                SDL.Rect scissorRect = new()
                {
                    X = (int) clipMin.X,
                    Y = (int) clipMin.Y,
                    W = (int) clipMax.X - (int) clipMin.X,
                    H = (int) clipMax.Y - (int) clipMin.Y
                };

                SDL.SetGPUScissor(renderPass, &scissorRect);

                SDL.GPUTextureSamplerBinding samplerBinding = new()
                {
                    Texture = texture,
                    Sampler = _sampler
                };

                SDL.BindGPUFragmentSamplers(renderPass, 0, &samplerBinding, 1);

                SDL.DrawGPUIndexedPrimitives(renderPass, drawCmd.ElemCount, 1, drawCmd.IdxOffset + indexOffset,
                    (short) (drawCmd.VtxOffset + vertexOffset), 0);
            }
            
            vertexOffset += (uint) cmdList.VtxBuffer.Size;
            indexOffset += (uint) cmdList.IdxBuffer.Size;
        }
        
        SDL.EndGPURenderPass(renderPass);
        
        SdlUtils.PopDebugGroup(cb);
        
        ImGui.NewFrame();

        return true;
    }

    public void Resize(Size<int> size)
    {
        ImGui.GetIO().DisplaySize = new Vector2(size.Width, size.Height);
    }

    private unsafe void RecreateFontTexture()
    {
        if (_texture != null)
            SDL.ReleaseGPUTexture(_device, _texture.Value);

        ImGuiIOPtr io = ImGui.GetIO();
        byte* imagePixels;
        int width, height;
        io.Fonts.GetTexDataAsRGBA32(&imagePixels, &width, &height);

        _texture = SdlUtils.CreateTexture2D(_device, (nint) imagePixels, (uint) width, (uint) height,
            SDL.GPUTextureFormat.R8g8b8a8Unorm, 1);
    }

    public void Dispose()
    {
        SDL.ReleaseGPUTexture(_device, _texture!.Value);
        SDL.ReleaseGPUSampler(_device, _sampler);
        SDL.ReleaseGPUGraphicsPipeline(_device, _pipeline);
        SDL.ReleaseGPUTransferBuffer(_device, _transferBuffer);
        SDL.ReleaseGPUBuffer(_device, _indexBuffer);
        SDL.ReleaseGPUBuffer(_device, _vertexBuffer);
        
        ImGui.DestroyContext(_imguiContext);
    }
}