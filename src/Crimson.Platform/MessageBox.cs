using piko.SDL3;

namespace Crimson.Platform;

public static class MessageBox
{
    public static void Show(Severity severity, string title, string message)
    {
        uint flags = severity switch
        {
            Severity.Info => SDL.MessageboxInformation,
            Severity.Warning => SDL.MessageboxWarning,
            Severity.Error => SDL.MessageboxError,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
        
        SDL.ShowSimpleMessageBox(flags, title, message, Surface.Window);
    }

    public enum Severity
    {
        Info,
        Warning,
        Error
    }
}