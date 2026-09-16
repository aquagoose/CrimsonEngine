using Crimson.Core;
using Crimson.Graphics.Utils;
using piko.SDL3;

namespace Crimson.Graphics;

internal sealed class RenderContext : IDisposable
{
    public readonly SDL.Window Window;

    public readonly SDL.GPUDevice Device;

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
    }

    public void Dispose()
    {
        SDL.ReleaseWindowFromGPUDevice(Device, Window);
        SDL.DestroyGPUDevice(Device);
    }
}