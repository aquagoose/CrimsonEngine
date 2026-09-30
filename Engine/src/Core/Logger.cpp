#include "Crimson/Core/Logger.h"

#include <sstream>
#include <chrono>
#include <format>
#include <filesystem>
#include <iostream>

namespace cge
{
    std::stringstream _ss;

    void Logger::Log(Severity severity, const std::string& message, const std::source_location& location)
    {
        _ss.str("");

        auto now = std::chrono::system_clock::now();
        _ss << std::format("{:%F %T} ", std::chrono::round<std::chrono::milliseconds>(now));

        switch (severity)
        {
            case Severity::Trace:
                _ss << "[Trace] ";
                break;
            case Severity::Debug:
                _ss << "[Debug] ";
                break;
            case Severity::Info:
                _ss << "[Info]  ";
                break;
            case Severity::Warning:
                _ss << "[Warn]  ";
                break;
            case Severity::Error:
                _ss << "[Error] ";
                break;
            case Severity::Fatal:
                _ss << "[FATAL] ";
                break;
        }

        auto fileName = std::filesystem::path(location.file_name()).filename().c_str();
        _ss << '(' << fileName << ':' << location.line() << ") ";
        _ss << message;

#ifndef NDEBUG
        std::cout << _ss.str() << std::endl;
#endif
    }
}
