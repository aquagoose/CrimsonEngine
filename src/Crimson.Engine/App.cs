using System.Reflection;
using Crimson.Core;
using Crimson.Graphics;
using Crimson.Math;
using Crimson.Platform;
using piko.SDL3;

namespace Crimson.Engine;

/// <summary>
/// Crimson's entry point, responsible for running and managing your application.
/// </summary>
public static class App
{
    private static string _appName;
    private static string _appVersion;
    private static Application? _application;
    private static bool _isRunning;
    
    /// <summary>
    /// The application name.
    /// </summary>
    public static string Name => _appName;

    /// <summary>
    /// The application version.
    /// </summary>
    public static string Version => _appVersion;

    /// <summary>
    /// The global <see cref="Crimson.Engine.Application"/> associated with the app, if any.
    /// </summary>
    public static Application? Application => _application;

    /// <summary>
    /// Gets if the application is running.
    /// </summary>
    public static bool IsRunning => _isRunning;

    /// <summary>
    /// The number of ticks every second. (Default: 60)
    /// </summary>
    public static int TicksPerSecond
    {
        get => field;
        set => field = value;
    }

    /// <summary>
    /// Run the application.
    /// </summary>
    public static void Run(in AppInfo info, Application? application = null)
    {
        _appName = info.Name;
        _appVersion = info.Version;
        _application = application;
        
        Logger.Info($"App Name: {_appName}");
        Logger.Info($"App Version: {_appVersion}");
        Logger.Info($"App Assembly Version: {Assembly.GetEntryAssembly()?.GetVersionAttribute() ?? "unknown"}");
        Logger.Info($"Crimson Version: {Assembly.GetExecutingAssembly().GetVersionAttribute() ?? "unknown"}");
        
        Logger.Info($"Argv: {Environment.CommandLine}");
        Logger.Info($"CWD: {Environment.CurrentDirectory}");
        Logger.Info($"CLR version: {Environment.Version}");
        Logger.Info($"OS: {Environment.OSVersion}");
        Logger.Info("Processor: TODO");
        Logger.Info($"Logical threads: {Environment.ProcessorCount}");

        TicksPerSecond = 60; // todo add this to AppInfo
        
        Logger.Debug("Initializing window.");
        Window.Init(new WindowInfo("Test", new Size<uint>(1280, 720), true)); // todo window options in appinfo
        
        Logger.Debug("Initializing events.");
        Events.Init();
        Events.Quit += Quit;
        
        Logger.Debug("Initializing renderer.");
        Renderer.Init(new SDL.Window(Window.Handle));

        _application?.Init();
        
        _isRunning = true;
        while (IsRunning)
        {
            Events.Poll();
            
            Renderer.NewFrame();
            
            _application?.Tick(1.0f / 60.0f);
            _application?.Loop(1.0f / 60.0f);
            
            Renderer.Render();
        }
        
        Renderer.Free();
        Events.Free();
        Window.Free();
    }

    /// <summary>
    /// Gracefully close and quit the application.
    /// </summary>
    public static void Quit()
    {
        _isRunning = false;
    }
}