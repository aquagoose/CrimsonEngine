using Crimson.Core;
using piko.SDL3;

namespace Crimson.Graphics.SDLGPU;

/// <summary>
/// A small wrapper around an SDL GPU device, providing useful utilities, and can be passed around as an instance.
/// </summary>
internal class GPUContext : IDisposable
{
    private readonly SDL.Window _window;

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
    }

    public void Dispose()
    {
        SDL.ReleaseWindowFromGPUDevice(Device, _window);
        SDL.DestroyGPUDevice(Device);
    }
}