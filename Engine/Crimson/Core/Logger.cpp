#include "Logger.h"

#include <sstream>
#include <filesystem>
#include <format>
#include <chrono>
#include <iostream>

namespace cge
{
    static std::stringstream _ss;

    void Logger::Log(Severity severity, const std::string& message, const std::source_location& location)
    {
        _ss.str("");

        auto now = std::chrono::system_clock::now();
        auto time = std::format("{:%F %T} ", std::chrono::floor<std::chrono::milliseconds>(now));
        auto filePath = std::filesystem::path(location.file_name()).filename().string();

        _ss << time;

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

        _ss << '(' << filePath << ':' << location.line() << ')' << ' ' << message;

        // always output to stdout when debug is enabled.
#ifndef NDEBUG
        std::cout << _ss.str() << std::endl;
#endif
    }
}
