using Crimson.Core;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// Performs 2D and 3D rendering to draw objects to the display.
/// </summary>
public class Renderer : IDisposable
{
    private readonly SDL.Window _window;

    internal readonly SDL.GPUDevice Device;

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
        Device = SDL.CreateGPUDeviceWithProperties(props);
    }

    public void Dispose()
    {
        SDL.WaitForGPUIdle(Device);

        SDL.DestroyGPUDevice(Device);
    }
}