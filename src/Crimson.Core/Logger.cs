using System.Runtime.CompilerServices;
using System.Text;

namespace Crimson.Core;

/// <summary>
/// Log events and messages to the console and/or a file.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Invoked whenever a new message is logged.
    /// </summary>
    public static event OnMessageLogged MessageLogged;

    private static StringBuilder _sb;

    /// <summary>
    /// Enable/disable logging to the console.
    /// </summary>
    /// <remarks>When compiling with DEBUG, this will be enabled by default.</remarks>
    public static bool LogToConsole
    {
        get => field;
        set
        {
            if (field == value)
                return;

            field = value;

            if (field)
                MessageLogged += WriteMessageToConsole;
            else
                MessageLogged -= WriteMessageToConsole;
        }
    }

    static Logger()
    {
        MessageLogged = delegate { };
        _sb = new StringBuilder();
#if DEBUG
        LogToConsole = true;
#endif
    }

    /// <summary>
    /// Log a message.
    /// </summary>
    /// <param name="severity">The <see cref="Severity"/> of the message.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The line number that this method was called from.</param>
    /// <param name="file">The file path that this method was called from.</param>
    public static void Log(Severity severity, string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
    {
        _sb.Clear();

        DateTime now = DateTime.Now;
        _sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss.fff "));

        _sb.Append(severity switch
        {
            Severity.Trace =>   "[Trace] ",
            Severity.Debug =>   "[Debug] ",
            Severity.Info =>    "[Info]  ",
            Severity.Warning => "[Warn]  ",
            Severity.Error =>   "[Error] ",
            Severity.Fatal =>   "[FATAL] ",
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        });

        string fileName = Path.GetFileName(file);
        _sb.Append('(');
        _sb.Append(fileName);
        _sb.Append(':');
        _sb.Append(line);
        _sb.Append(')');
        _sb.Append(' ');

        _sb.Append(message);

        MessageLogged(_sb.ToString(), severity, line, file);
    }

    /// <summary>
    /// Log a <see cref="Severity.Trace"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The line number that this method was called from.</param>
    /// <param name="file">The file path that this method was called from.</param>
    public static void Trace(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        => Log(Severity.Trace, message, line, file);

    /// <summary>
    /// Log a <see cref="Severity.Debug"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The line number that this method was called from.</param>
    /// <param name="file">The file path that this method was called from.</param>
    public static void Debug(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        => Log(Severity.Debug, message, line, file);

    /// <summary>
    /// Log a <see cref="Severity.Info"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The line number that this method was called from.</param>
    /// <param name="file">The file path that this method was called from.</param>
    public static void Info(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        => Log(Severity.Info, message, line, file);

    /// <summary>
    /// Log a <see cref="Severity.Warning"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The line number that this method was called from.</param>
    /// <param name="file">The file path that this method was called from.</param>
    public static void Warn(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        => Log(Severity.Warning, message, line, file);

    /// <summary>
    /// Log a <see cref="Severity.Error"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The line number that this method was called from.</param>
    /// <param name="file">The file path that this method was called from.</param>
    public static void Error(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        => Log(Severity.Error, message, line, file);

    /// <summary>
    /// Log a <see cref="Severity.Fatal"/> message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="line">The line number that this method was called from.</param>
    /// <param name="file">The file path that this method was called from.</param>
    public static void Fatal(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        => Log(Severity.Fatal, message, line, file);

    private static void WriteMessageToConsole(string message, Severity severity, int line, string file)
    {
        string colorCode = severity switch
        {
            Severity.Trace =>   "90", // gray
            Severity.Debug =>   "0",  // default terminal color
            Severity.Info =>    "96", // cyan
            Severity.Warning => "93", // yellow
            Severity.Error =>   "91", // red
            Severity.Fatal =>   "31", // dark red
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };

        Console.WriteLine($"\e[{colorCode}m{message}\e[0m");
    }

    /// <summary>
    /// Represents how severe a log message can be.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Verbose messages logging exact code paths.
        /// </summary>
        Trace,

        /// <summary>
        /// Messages containing useful debug information.
        /// </summary>
        Debug,

        /// <summary>
        /// Useful information.
        /// </summary>
        Info,

        /// <summary>
        /// Something isn't right, but it was handled.
        /// </summary>
        Warning,

        /// <summary>
        /// Something went wrong, but it was handled and program execution can continue.
        /// </summary>
        Error,

        /// <summary>
        /// Something went very wrong, and program execution cannot continue.
        /// </summary>
        Fatal
    }

    public delegate void OnMessageLogged(string message, Severity severity, int line, string file);
}