#pragma once

#include <string>
#include <source_location>

/**
 * Contains utility functions for logging.
 */
namespace cge::Logger
{
    /**
     * Defines the severity of a log message.
     */
    enum class Severity
    {
        /**
         * Verbose messages tracing exact code paths.
         */
        Trace,

        /**
         * Messages containing useful debug information.
         */
        Debug,

        /**
         * General information.
         */
        Info,

        /**
         * Something isn't right, but everything can continue.
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
     * Log a message.
     * @param severity The severity of the message.
     * @param message A formatted log messaage.
     * @param location The source location that the log was created at.
     */
    void Log(Severity severity, const std::string& message, const std::source_location& location = std::source_location::current());
}
