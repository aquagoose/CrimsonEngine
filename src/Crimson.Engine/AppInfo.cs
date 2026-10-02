using Crimson.Math;
using Crimson.Platform;

namespace Crimson.Engine;

public struct AppInfo
{
    public string Name;

    public string Version;

    public WindowInfo Window;

    public AppInfo(string name, string version)
    {
        Name = name;
        Version = version;
        Window = new WindowInfo(name, Size<uint>.Zero, false, false);
    }
}