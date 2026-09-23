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

    public WindowInfo(string title, Size<uint> size, bool resizable)
    {
        Title = title;
        Size = size;
        Resizable = resizable;
    }
}