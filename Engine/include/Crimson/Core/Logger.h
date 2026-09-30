#pragma once

#include <string>
#include <source_location>
#include <format>
#include <stdexcept>

#define CGE_LOG(severity, ...) {\
    auto message = std::format(__VA_ARGS__);\
    cge::Logger::Log(cge::Logger::Severity::severity, message);\
    if (cge::Logger::Severity::severity == cge::Logger::Severity::Fatal)\
        throw std::runtime_error(message);\
}

#define CGE_TRACE(...) CGE_LOG(Trace, __VA_ARGS__);
#define CGE_DEBUG(...) CGE_LOG(Debug, __VA_ARGS__);
#define CGE_INFO(...) CGE_LOG(Info, __VA_ARGS__);
#define CGE_WARN(...) CGE_LOG(Warning, __VA_ARGS__);
#define CGE_ERROR(...) CGE_LOG(Error, __VA_ARGS__);
#define CGE_FATAL(...) CGE_LOG(Fatal, __VA_ARGS__);

/**
 * Provides logging functions.
 */
namespace cge::Logger
{
    /**
     * Represents various severities of log messages.
     */
    enum class Severity
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
         * Useful information.
         */
        Info,

        /**
         * Something isn't right, but it was handled.
         */
        Warning,

        /**
         * Something bad happened, but program execution can continue.
         */
        Error,

        /**
         * Something very bad happened, and program execution cannot continue.
         */
        Fatal
    };

    void Log(Severity severity, const std::string& message,
             const std::source_location& location = std::source_location::current());
}