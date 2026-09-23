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
    /// <summary>
    /// The application name.
    /// </summary>
    public static string Name;

    /// <summary>
    /// The application version.
    /// </summary>
    public static string Version;

    /// <summary>
    /// Gets if the application is running.
    /// </summary>
    public static bool IsRunning;

    /// <summary>
    /// Run the application.
    /// </summary>
    public static void Run(in AppInfo info)
    {
        Name = info.Name;
        Version = info.Version;
        
        Logger.Info($"App Name: {Name}");
        Logger.Info($"App Version: {Version}");
        Logger.Info($"App Assembly Version: {Assembly.GetEntryAssembly()?.GetVersionAttribute() ?? "unknown"}");
        Logger.Info($"Crimson Version: {Assembly.GetExecutingAssembly().GetVersionAttribute() ?? "unknown"}");
        
        Logger.Info($"Argv: {Environment.CommandLine}");
        Logger.Info($"CWD: {Environment.CurrentDirectory}");
        Logger.Info($"CLR version: {Environment.Version}");
        Logger.Info($"OS: {Environment.OSVersion}");
        Logger.Info("Processor: TODO");
        Logger.Info($"Logical threads: {Environment.ProcessorCount}");
        
        Logger.Debug("Initializing window.");
        Window.Init(new WindowInfo("Test", new Size<uint>(1280, 720), true)); // todo window options in appinfo
        
        Logger.Debug("Initializing events.");
        Events.Init();
        Events.Quit += () => IsRunning = false;
        
        Logger.Debug("Initializing renderer.");
        Renderer.Init(new SDL.Window(Window.Handle));

        IsRunning = true;
        while (IsRunning)
        {
            Events.Poll();
            
            Renderer.Render();
        }
        
        Renderer.Free();
        Events.Free();
        Window.Free();
    }
}