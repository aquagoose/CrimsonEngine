using System.Runtime.CompilerServices;
using System.Text;

namespace Crimson.Core;

/// <summary>
/// Log useful messages. Helpful for debugging and tracing code paths.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Invoked whenever a new message is logged.
    /// </summary>
    public static event OnMessageLogged MessageLogged;

    private static readonly StringBuilder _sb;

    /// <summary>
    /// Enable/disable logging to the console.
    /// </summary>
    /// <remarks>This is enabled by default in Debug config, but not in any other configuration.</remarks>
    public static bool LogToConsole
    {
        get => field;
        set
        {
            // subscribing/unsubscribing to the event multiple times can cause weird issues to this prevents that
            if (value == field)
                return;

            field = value;
            if (field)
                MessageLogged += LogMessageToConsole;
            else
                MessageLogged -= LogMessageToConsole;
        }
    }

    static Logger()
    {
        MessageLogged = delegate { };
        _sb = new StringBuilder();
#if DEBUG // enable logging to console by default in debug mode. it's probably useful.
        LogToConsole = true;
#endif
    }

    /// <summary>
    /// Log a message.
    /// </summary>
    /// <param name="severity">The message's <see cref="Severity"/>.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The calling line number.</param>
    /// <param name="path">The calling file path.</param>
    /// <remarks>
    /// You should generally avoid calling this directly, instead use the <see cref="Trace"/>, <see cref="Debug"/>,
    /// <see cref="Info"/>, <see cref="Warn"/>, <see cref="Error"/>, and <see cref="Fatal"/> methods.
    /// </remarks>
    public static void Log(Severity severity, string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
    {
        _sb.Clear();

        DateTime now = DateTime.Now;
        string fileName = Path.GetFileName(path);

        _sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss.fff "));
        _sb.Append('(');
        _sb.Append(fileName);
        _sb.Append(':');
        _sb.Append(line);
        _sb.Append(')');
        _sb.Append(' ');

        // to help with my ocd this aligns all messages as much as possible (as long as the file name and line number aren't stupidly long)
        const int minLength = 50;
        if (_sb.Length < minLength)
            _sb.Append(' ', minLength - _sb.Length);

        _sb.Append(severity switch
        {
            Severity.Trace   => "[Trace] ",
            Severity.Debug   => "[Debug] ",
            Severity.Info    => "[Info]  ",
            Severity.Warning => "[Warn]  ",
            Severity.Error   => "[Error] ",
            Severity.Fatal   => "[Fatal] ",
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        });

        _sb.Append(message);

        MessageLogged(_sb.ToString(), severity);
    }

    /// <summary>
    /// Log a <see cref="Severity.Trace"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The calling line number.</param>
    /// <param name="path">The calling file path.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Trace(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => Log(Severity.Trace, message, line, path);

    /// <summary>
    /// Log a <see cref="Severity.Debug"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The calling line number.</param>
    /// <param name="path">The calling file path.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => Log(Severity.Debug, message, line, path);

    /// <summary>
    /// Log a <see cref="Severity.Info"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The calling line number.</param>
    /// <param name="path">The calling file path.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => Log(Severity.Info, message, line, path);

    /// <summary>
    /// Log a <see cref="Severity.Warning"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The calling line number.</param>
    /// <param name="path">The calling file path.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => Log(Severity.Warning, message, line, path);

    /// <summary>
    /// Log a <see cref="Severity.Error"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The calling line number.</param>
    /// <param name="path">The calling file path.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => Log(Severity.Error, message, line, path);

    /// <summary>
    /// Log a <see cref="Severity.Fatal"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The calling line number.</param>
    /// <param name="path">The calling file path.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fatal(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => Log(Severity.Fatal, message, line, path);

    private static void LogMessageToConsole(string message, Severity severity)
    {
        string color = severity switch
        {
            Severity.Trace   => "37", // gray/white
            Severity.Debug   => "0",  // default terminal color
            Severity.Info    => "96", // cyan
            Severity.Warning => "93", // yellow
            Severity.Error   => "91", // red
            Severity.Fatal   => "31", // dark red
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };

        Console.WriteLine($"\e[{color}m{message}\e[0m");
    }

    /// <summary>
    /// Defines the allowed severity of a log message.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Verbose messages used to trace code paths.
        /// </summary>
        Trace,

        /// <summary>
        /// Messages to aid with debugging.
        /// </summary>
        Debug,

        /// <summary>
        /// Useful information.
        /// </summary>
        Info,

        /// <summary>
        /// Something isn't right, but not enough to affect execution.
        /// </summary>
        Warning,

        /// <summary>
        /// Something bad happened, but execution can continue.
        /// </summary>
        Error,

        /// <summary>
        /// Something REALLY bad happened, and execution cannot continue.
        /// </summary>
        Fatal
    }

    public delegate void OnMessageLogged(string message, Severity severity);
}