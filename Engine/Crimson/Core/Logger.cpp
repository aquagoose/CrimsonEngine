#include "Logger.h"

#include <sstream>
#include <chrono>
#include <filesystem>
#include <iostream>

namespace cge
{
    std::stringstream _ss;

    void Logger::Log(LogSeverity severity, const std::string& message, std::source_location location)
    {
        // clear the stringstream
        _ss.str("");

        auto now = std::chrono::system_clock::now();
        auto fileName = std::filesystem::path(location.file_name()).filename();

        _ss << std::format("{:%F %T} ", std::chrono::round<std::chrono::milliseconds>(now));

        switch (severity)
        {
            case LogSeverity::Trace:
                _ss << "[Trace] ";
                break;
            case LogSeverity::Debug:
                _ss << "[Debug] ";
                break;
            case LogSeverity::Info:
                _ss << "[Info]  ";
                break;
            case LogSeverity::Warning:
                _ss << "[Warn]  ";
                break;
            case LogSeverity::Error:
                _ss << "[Error] ";
                break;
            case LogSeverity::Fatal:
                _ss << "[FATAL] ";
                break;
        }

        _ss << '(' << fileName << ':' << location.line() << ") ";
        _ss << message;

#ifndef NDEBUG
        std::cout << _ss.str() << std::endl;
#endif
    }
}
