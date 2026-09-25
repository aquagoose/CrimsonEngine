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

#define CGE_TRACE(...) CGE_LOG(Trace, __VA_ARGS__)
#define CGE_DEBUG(...) CGE_LOG(Debug, __VA_ARGS__)
#define CGE_INFO(...) CGE_LOG(Info, __VA_ARGS__)
#define CGE_WARN(...) CGE_LOG(Warning, __VA_ARGS__)
#define CGE_ERROR(...) CGE_LOG(Error, __VA_ARGS__)
#define CGE_FATAL(...) CGE_LOG(Fatal, __VA_ARGS__)

namespace cge::Logger
{
    /**
     * Defines the severity of a log message.
     */
    enum class Severity
    {
        /**
         * Verbose messages, logging exact code paths.
         */
        Trace,

        /**
         * Debug information.
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
         * Something VERY bad happened, and program execution cannot continue.
         */
        Fatal
    };

    /**
     * Log a message.
     * @param severity The message severity.
     * @param message The message to log.
     * @param location The location where this function was called.
     */
    void Log(Severity severity, const std::string_view& message, std::source_location location = std::source_location::current());
}