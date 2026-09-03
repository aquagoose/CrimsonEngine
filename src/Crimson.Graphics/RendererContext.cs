using Crimson.Core;
using Crimson.Graphics.Utils;
using piko.SDL3;

namespace Crimson.Graphics;

internal sealed class RendererContext : IDisposable
{
    public readonly SDL.Window Window;
    public readonly SDL.GPUDevice Device;

    public RendererContext(SDL.Window window)
    {
        Window = window;

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
        SDL.ClaimWindowForGPUDevice(Device, Window).Check("Claim window for device");
    }

    public void Dispose()
    {
        SDL.ReleaseWindowFromGPUDevice(Device, Window);
        SDL.DestroyGPUDevice(Device);
    }
}