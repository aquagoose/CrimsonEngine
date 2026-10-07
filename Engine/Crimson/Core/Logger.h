#pragma once

#include <string>
#include <source_location>
#include <format>
#include <stdexcept>

#define CGE_LOG(severity, ...) {\
    auto message = std::format(__VA_ARGS__);\
    cge::Logger::Log(cge::LogSeverity::severity, message);\
    if (cge::LogSeverity::severity == cge::LogSeverity::Fatal)\
        throw std::runtime_error(message);\
}

#define CGE_TRACE(...) CGE_LOG(Trace, __VA_ARGS__)
#define CGE_DEBUG(...) CGE_LOG(Debug, __VA_ARGS__)
#define CGE_INFO(...) CGE_LOG(Info, __VA_ARGS__)
#define CGE_WARN(...) CGE_LOG(Warning, __VA_ARGS__)
#define CGE_ERROR(...) CGE_LOG(Error, __VA_ARGS__)
#define CGE_FATAL(...) CGE_LOG(Fatal, __VA_ARGS__)

namespace cge
{
    /**
     * Defines various severities of log messages.
     */
    enum class LogSeverity
    {
        /**
         * Verbose messages tracing exact code paths.
         */
        Trace,

        /**
         * Messages containing debug information.
         */
        Debug,

        /**
         * Useful information, often system information, to aid with debugging.
         */
        Info,

        /**
         * Something isn't right, but it was handled.
         */
        Warning,

        /**
         * Something went wrong, but it was handled and program execution can continue.
         */
        Error,

        /**
         * Something went very wrong, and program execution cannot continue.
         */
        Fatal
    };

    /**
     * Log useful messages to stdout and a file.
     */
    class Logger final
    {
    public:
        /**
         * Log a message to stdout and a file (if set up).
         * @param severity The severity of the message.
         * @param message The message to log.
         * @param location The location where this function was called.
         */
        static void Log(LogSeverity severity, const std::string& message, std::source_location location = std::source_location::current());
    };
}
