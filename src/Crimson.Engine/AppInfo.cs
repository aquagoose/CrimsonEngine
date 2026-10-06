using Crimson.Math;
using Crimson.Platform;

namespace Crimson.Engine;

/// <summary>
/// Contains properties describing app creation.
/// </summary>
public struct AppInfo(string appName, string appVersion)
{
    /// <summary>
    /// The name of the application.
    /// </summary>
    public string AppName = appName;

    /// <summary>
    /// The application version.
    /// </summary>
    public string AppVersion = appVersion;

    /// <summary>
    /// The <see cref="WindowInfo"/> to use when creating the window.
    /// </summary>
    public WindowInfo Window = new(appName, Size<uint>.Zero, false, false);
}