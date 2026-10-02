using Crimson.Math;

namespace Crimson.Platform;

/// <summary>
/// Describes how a <see cref="Window"/> is initialized.
/// </summary>
public struct WindowInfo
{
    /// <summary>
    /// The window title.
    /// </summary>
    public string Title;

    /// <summary>
    /// The window size.
    /// </summary>
    public Size<uint> Size;

    /// <summary>
    /// Whether the window is resizable by the user.
    /// </summary>
    public bool Resizable;

    /// <summary>
    /// If the window should open in borderless fullscreen mode.
    /// </summary>
    public bool Fullscreen;

    public WindowInfo(string title, Size<uint> size, bool resizable, bool fullscreen)
    {
        Title = title;
        Size = size;
        Resizable = resizable;
        Fullscreen = fullscreen;
    }
}