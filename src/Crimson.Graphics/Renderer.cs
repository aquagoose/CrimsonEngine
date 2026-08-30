using Crimson.Core;
using Crimson.Graphics.Utils;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// Performs 2D and 3D rendering to draw objects to the display.
/// </summary>
public sealed class Renderer : IDisposable
{
    public bool IsDisposed { get; private set; }

    private readonly SDL.Window _window;

    internal readonly SDL.GPUDevice Device;

    /// <summary>
    /// Gets the graphics backend name for this renderer.
    /// </summary>
    public string BackendName => SDL.GetGPUDeviceDriver(Device);

    /// <summary>
    /// Create a new <see cref="Renderer"/>.
    /// </summary>
    /// <param name="window">The SDL3 window to associate with this renderer.</param>
    public Renderer(SDL.Window window)
    {
        _window = window;

        uint props = SDL.CreateProperties();
        // always enable vulkan as a fallback and for linux
        SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersSpirvBoolean, true);

        // enable d3d12 on windows
        if (OperatingSystem.IsWindows())
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersDxilBoolean, true);

        // enable metal on macos
        if (OperatingSystem.IsMacOS())
            SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateShadersMslBoolean, true);

#if DEBUG
        SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateDebugmodeBoolean, true);
        SDL.SetBooleanProperty(props, SDL.Prop.GpuDeviceCreateVerboseBoolean, true);
#endif

        Logger.Trace("Creating device.");
        Device = SDL.CreateGPUDeviceWithProperties(props).Check("Create device");
        SDL.DestroyProperties(props);

        uint deviceProps = SDL.GetGPUDeviceProperties(Device);
        Logger.Info($"Backend: {SDL.GetGPUDeviceDriver(Device)}");
        Logger.Info($"Device: {SDL.GetStringProperty(deviceProps, SDL.Prop.GpuDeviceNameString, "unknown")}");
        Logger.Info($"Driver: {SDL.GetStringProperty(deviceProps, SDL.Prop.GpuDeviceDriverInfoString, "unknown")}");
        SDL.DestroyProperties(deviceProps);

        Logger.Trace("Claiming window for device.");
        SDL.ClaimWindowForGPUDevice(Device, _window).Check("Claim window for device");
    }

    /// <summary>
    /// Render everything to the display.
    /// </summary>
    public void Render()
    {
        SDL.GPUCommandBuffer cb = SDL.AcquireGPUCommandBuffer(Device).Check("Acquire command buffer");

        SDL.WaitAndAcquireGPUSwapchainTexture(cb, _window, out SDL.GPUTexture swapchainTexture, out _, out _)
            .Check("Acquire swapchain texture");

        // don't try to render if there's nothing to render to.
        if (swapchainTexture.IsNull)
        {
            SDL.CancelGPUCommandBuffer(cb).Check("Cancel command buffer");
            return;
        }

        SDL.GPUColorTargetInfo targetInfo = new()
        {
            Texture = swapchainTexture,
            ClearColor = new SDL.FColor(1.0f, 0.5f, 0.25f, 1.0f),
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store
        };

        SDL.GPURenderPass pass = SDL.BeginGPURenderPass(cb, [targetInfo], null).Check("Begin render pass");
        SDL.EndGPURenderPass(pass);

        SDL.SubmitGPUCommandBuffer(cb).Check("Submit command buffer");
    }

    /// <summary>
    /// Dispose of this <see cref="Renderer"/>.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        SDL.WaitForGPUIdle(Device);

        SDL.ReleaseWindowFromGPUDevice(Device, _window);
        SDL.DestroyGPUDevice(Device);
    }
}