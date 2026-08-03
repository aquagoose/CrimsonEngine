using piko.SDL3;

namespace Crimson.Platform;

public static class MessageBox
{
    public static void Show(Severity severity, string title, string message)
    {
        SDL.MessageBoxFlags flags = severity switch
        {
            Severity.Info => SDL.MessageBoxFlags.Information,
            Severity.Warning => SDL.MessageBoxFlags.Warning,
            Severity.Error => SDL.MessageBoxFlags.Error,
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