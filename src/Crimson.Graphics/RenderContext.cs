using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Graphics.Utils;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics;

internal sealed class RenderContext : IDisposable
{
    /// <summary>
    /// 32MiB initial transfer buffer size.
    /// </summary>
    private const uint InitialTranferBufferSize = 32 * 1024 * 1024;

    private SDL.GPUTransferBuffer _transferBuffer;
    private uint _transferBufferSize;
    private uint _transferBufferOffset;

    public readonly SDL.Window Window;
    public readonly SDL.GPUDevice Device;

    public readonly HashSet<SDL.GPUTexture> MipmapQueue;

    public RenderContext(SDL.Window window)
    {
        Window = window;

        uint createProps = SDL.CreateProperties();

        if (OperatingSystem.IsWindows()) // enable dx12 on windows
            SDL.SetBooleanProperty(createProps, SDL.Prop.GpuDeviceCreateShadersDxilBoolean, true);
        else if (OperatingSystem.IsMacOS()) // enable metal on macos
            SDL.SetBooleanProperty(createProps, SDL.Prop.GpuDeviceCreateShadersMslBoolean, true);
        else // enable vulkan on everything else
            SDL.SetBooleanProperty(createProps, SDL.Prop.GpuDeviceCreateShadersSpirvBoolean, true);

#if DEBUG
        SDL.SetBooleanProperty(createProps, SDL.Prop.GpuDeviceCreateDebugmodeBoolean, true);
        SDL.SetBooleanProperty(createProps, SDL.Prop.GpuDeviceCreateVerboseBoolean, true);
#endif

        Logger.Trace("Creating device.");
        Device = SDL.CreateGPUDeviceWithProperties(createProps).Check("Create device");
        SDL.DestroyProperties(createProps);

        uint deviceProps = SDL.GetGPUDeviceProperties(Device);
        Logger.Info($"Backend: {SDL.GetGPUDeviceDriver(Device)}");
        Logger.Info($"Device: {SDL.GetStringProperty(deviceProps, SDL.Prop.GpuDeviceNameString, "unknown")}");
        Logger.Info($"Driver: {SDL.GetStringProperty(deviceProps, SDL.Prop.GpuDeviceDriverInfoString, "unknown")}");
        SDL.DestroyProperties(deviceProps);

        Logger.Trace("Claiming window for device.");
        SDL.ClaimWindowForGPUDevice(Device, Window).Check("Claim window for device");

        MipmapQueue = [];

        _transferBufferSize = InitialTranferBufferSize;
        _transferBufferOffset = 0;
        _transferBuffer = CreateTransferBuffer(SDL.GPUTransferBufferUsage.Upload, _transferBufferSize);
    }

    public unsafe SDL.GPUBuffer CreateBuffer(SDL.GPUBufferUsageFlags usage, uint size)
    {
        SDL.GPUBufferCreateInfo bufferInfo = new()
        {
            Usage = usage,
            Size = size
        };

        Logger.Trace($"Creating {size / 1024}KiB {usage} buffer.");
        return SDL.CreateGPUBuffer(Device, &bufferInfo).Check("Create buffer");
    }

    public unsafe SDL.GPUTransferBuffer CreateTransferBuffer(SDL.GPUTransferBufferUsage usage, uint size)
    {
        SDL.GPUTransferBufferCreateInfo bufferInfo = new()
        {
            Usage = usage,
            Size = size
        };

        Logger.Trace($"Creating {size / 1024}KiB transfer buffer.");
        return SDL.CreateGPUTransferBuffer(Device, &bufferInfo).Check("Create transfer buffer");
    }

    public SDL.GPUTransferBuffer GetUploadBuffer(uint size, out uint offset, out bool shouldCycle)
    {
        if (size >= _transferBufferSize)
        {
            Logger.Trace(
                $"Requested upload size ({size / 1024}KiB) is larger than the transfer buffer size ({_transferBufferSize / 1024}KiB). It will be resized.");
            // ensure the buffer can hold the data, then round to the next power of 2 so there's some wiggle room.
            _transferBufferSize = BitUtils.RoundToNextPowerOf2(size);
            _transferBufferOffset = 0;
            SDL.ReleaseGPUTransferBuffer(Device, _transferBuffer);
            _transferBuffer = CreateTransferBuffer(SDL.GPUTransferBufferUsage.Upload, _transferBufferSize);
        }

        shouldCycle = false;
        if (_transferBufferOffset + size >= _transferBufferSize)
        {
            shouldCycle = true;
            _transferBufferOffset = 0;
        }

        offset = _transferBufferOffset;
        _transferBufferOffset += size;

        return _transferBuffer;
    }

    // todo Vec2<uint> for X and Y
    public unsafe void CopyDataToTexture(SDL.GPUTexture texture, void* data, uint x, uint y, Size<uint> size,
        PixelFormat format)
    {
        uint dataSize = size.Width * size.Height * format.BytesPerPixel;

        SDL.GPUTransferBuffer transBuffer = GetUploadBuffer(dataSize, out uint offset, out bool cycle);
        Logger.Trace($"Uploading {dataSize / 1024}KiB data to texture {texture.Handle} (Offset: {offset}, Cycle: {cycle})");

        nint mapped = SDL.MapGPUTransferBuffer(Device, transBuffer, cycle).Check("Map transfer buffer");
        Unsafe.CopyBlock((byte*) mapped + offset, data, dataSize);
        SDL.UnmapGPUTransferBuffer(Device, transBuffer);

        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(Device).Check("Acquire command buffer");
        SDL.GPUCopyPass pass = SDL.BeginGPUCopyPass(cb).Check("Begin copy pass");

        SDL.GPUTextureTransferInfo src = new()
        {
            TransferBuffer = transBuffer,
            Offset = offset,
            PixelsPerRow = size.Width,
            RowsPerLayer = size.Height
        };

        SDL.GPUTextureRegion dest = new()
        {
            Texture = texture,
            X = x,
            Y = y,
            Z = 0,
            W = size.Width,
            H = size.Height,
            D = 1,
            MipLevel = 0,
            Layer = 0
        };

        SDL.UploadToGPUTexture(pass, &src, &dest, false);

        SDL.EndGPUCopyPass(pass);
        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }

    public void Dispose()
    {
        SDL.ReleaseGPUTransferBuffer(Device, _transferBuffer);
        SDL.ReleaseWindowFromGPUDevice(Device, Window);
        SDL.DestroyGPUDevice(Device);
    }
}