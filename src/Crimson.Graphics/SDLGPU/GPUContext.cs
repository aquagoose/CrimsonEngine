using System.Runtime.CompilerServices;
using Crimson.Core;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.SDLGPU;

/// <summary>
/// A small wrapper around an SDL GPU device, providing useful utilities, and can be passed around as an instance.
/// </summary>
internal class GPUContext : IDisposable
{
    /// <summary>
    /// 32MiB initial transfer buffer size.
    /// </summary>
    private const uint InitialTransferBufferSize = 32 * 1024 * 1024;

    private readonly SDL.Window _window;

    private SDL.GPUTransferBuffer _transferBuffer;
    private uint _transferBufferSize;
    private uint _transferBufferOffset;

    public readonly SDL.GPUDevice Device;

    public GPUContext(SDL.Window window)
    {
        _window = window;

        uint props = SDL.CreateProperties();
        // always enable vulkan as all platforms should support it (even macos)
        SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersSpirvBoolean, 1);

        // enable metal on macos
        if (OperatingSystem.IsMacOS())
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersMslBoolean, 1);

        // enable d3d12 on windows
        if (OperatingSystem.IsWindows())
        {
            // todo do we still want dxbc?
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersDxbcBoolean, 1);
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersDxilBoolean, 1);
        }

#if DEBUG
        SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateDebugmodeBoolean, 1);
#endif

        Logger.Trace("Creating device.");
        Device = SDL.CreateGPUDeviceWithProperties(props).Check("Create device");
        SDL.DestroyProperties(props);

        Logger.Trace("Claiming window for device.");
        SDL.ClaimWindowForGPUDevice(Device, _window).Check("Claim window for device");

        uint deviceProps = SDL.GetGPUDeviceProperties(Device);
        Logger.Info($"Backend: {SDL.GetGPUDeviceDriver(Device)}");
        Logger.Info($"GPU Device: {SDL.GetStringProperty(deviceProps, SDL.Prop.GpuDeviceNameString, "unknown")}");
        Logger.Info($"GPU Driver: {SDL.GetStringProperty(deviceProps, SDL.Prop.GpuDeviceDriverInfoString, "unknown")}");
        SDL.DestroyProperties(deviceProps);

        _transferBufferSize = InitialTransferBufferSize;
        _transferBufferOffset = 0;
        _transferBuffer = CreateTransferBuffer(SDL.GPUTransferBufferUsage.Upload, _transferBufferSize);
    }

    /// <summary>
    /// Get a transfer buffer to upload data to.
    /// </summary>
    /// <param name="dataSize">The size of the data to be uploaded.</param>
    /// <param name="bufferOffset">The offset into the buffer that data should be uploaded to.</param>
    /// <param name="cycle">Whether the buffer should be cycled.</param>
    /// <returns>A transfer buffer that can have data uploaded to it.</returns>
    /// <remarks>
    /// The returned transfer buffer is a cyclical buffer and therefore <paramref name="bufferOffset"/> may not be 0.
    /// It is important that data is written at this offset value, to ensure that previous data,
    /// which may still be in use, is not overwritten.
    /// Likewise, you should respect the result of <paramref name="cycle"/>, cycling the buffer when mapping it,
    /// if required.
    /// </remarks>
    public SDL.GPUTransferBuffer GetTransferBuffer(uint dataSize, out uint bufferOffset, out bool cycle)
    {
        if (dataSize >= _transferBufferSize)
        {
            Logger.Trace($"Data size ({dataSize / 1024}KiB) is larger than the transfer buffer ({_transferBufferSize / 1024}KiB)! The buffer will be resized.");
            _transferBufferSize = BitUtils.RoundToNextPowerOf2(dataSize); // ensure there is plenty of space!
            _transferBufferOffset = 0; // since we're creating a brand new transfer buffer, we can reset the offset.
            SDL.ReleaseGPUTransferBuffer(Device, _transferBuffer);
            _transferBuffer = CreateTransferBuffer(SDL.GPUTransferBufferUsage.Upload, _transferBufferSize);
        }

        cycle = false;
        if (_transferBufferOffset + dataSize >= _transferBufferSize)
        {
            _transferBufferOffset = 0;
            cycle = true;
        }

        bufferOffset = _transferBufferOffset;
        _transferBufferOffset += dataSize;

        return _transferBuffer;
    }

    public unsafe void CopyDataToTexture(SDL.GPUCommandBuffer cb, SDL.GPUTexture texture, uint x, uint y, Size<uint> size,
        PixelFormat format, ReadOnlySpan<byte> data)
    {
        uint dataSize = (uint) data.Length;

        SDL.GPUTransferBuffer transferBuffer = GetTransferBuffer(dataSize, out uint offset, out bool cycle);

        Logger.Trace($"Copying {dataSize/1024}KiB of data to texture {texture.Handle} (offset: {offset}, cycle: {cycle})");
        void* mapped = SDL.MapGPUTransferBuffer(Device, transferBuffer, (byte) (cycle ? 1 : 0));
        if (mapped == null)
            throw new Exception($"Failed to map transfer buffer: {SDL.GetError()}");

        fixed (byte* pData = data)
            Unsafe.CopyBlock((byte*) mapped + offset, pData, dataSize);

        SDL.UnmapGPUTransferBuffer(Device, transferBuffer);

        SDL.GPUCopyPass pass = SDL.BeginGPUCopyPass(cb);

        SDL.GPUTextureTransferInfo src = new()
        {
            TransferBuffer = transferBuffer,
            Offset = offset,
            PixelsPerRow = size.Width,
            RowsPerLayer = size.Height
        };

        SDL.GPUTextureRegion dst = new()
        {
            Texture = texture,
            X = x,
            Y = y,
            Z = 0,
            W = size.Width,
            H = size.Height,
            D = 1,
            Layer = 0,
            MipLevel = 0
        };

        SDL.UploadToGPUTexture(pass, &src, &dst, 0);
        SDL.EndGPUCopyPass(pass);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyDataToTexture(SDL.GPUTexture texture, uint x, uint y, Size<uint> size, PixelFormat format,
        ReadOnlySpan<byte> data)
    {
        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(Device).Check("Acquire command buffer");
        CopyDataToTexture(cb, texture, x, y, size, format, data);
        SDL.SubmitGPUCommandBuffer(cb);
    }

    /// <summary>
    /// Create an empty buffer.
    /// </summary>
    public unsafe SDL.GPUBuffer CreateBuffer(SDL.GPUBufferUsageFlags usage, uint size)
    {
        SDL.GPUBufferCreateInfo bufferInfo = new()
        {
            Usage = usage,
            Size = size
        };

        Logger.Trace($"Creating {size / 1024}KiB buffer. Usage flags: {usage}");
        return SDL.CreateGPUBuffer(Device, &bufferInfo).Check("Create buffer");
    }

    /// <summary>
    /// Create a transfer buffer.
    /// </summary>
    private unsafe SDL.GPUTransferBuffer CreateTransferBuffer(SDL.GPUTransferBufferUsage usage, uint size)
    {
        SDL.GPUTransferBufferCreateInfo transferBufferInfo = new()
        {
            Usage = usage,
            Size = size
        };

        Logger.Trace($"Creating {size / 1024}KiB {usage} transfer buffer.");
        return SDL.CreateGPUTransferBuffer(Device, &transferBufferInfo).Check("Create transfer buffer");
    }

    public void Dispose()
    {
        SDL.ReleaseGPUTransferBuffer(Device, _transferBuffer);

        SDL.ReleaseWindowFromGPUDevice(Device, _window);
        SDL.DestroyGPUDevice(Device);
    }
}