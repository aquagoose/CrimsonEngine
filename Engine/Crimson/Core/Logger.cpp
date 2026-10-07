#include "Logger.h"

#include <sstream>
#include <chrono>
#include <filesystem>
#include <iostream>

namespace cge
{
    static std::stringstream _ss;

    void Logger::Log(LogSeverity severity, const std::string& message, std::source_location location)
    {
        // clear the stringstream
        _ss.str("");

        auto now = std::chrono::system_clock::now();
        auto fileName = std::filesystem::path(location.file_name()).filename().string();

        _ss << std::format("{:%F %T} ", std::chrono::round<std::chrono::milliseconds>(now));

        std::string ansiCode; // the ansi color code to use in stdout
        switch (severity)
        {
            case LogSeverity::Trace:
                _ss << "[Trace] ";
                ansiCode = "\e[90m"; // gray
                break;
            case LogSeverity::Debug:
                _ss << "[Debug] ";
                ansiCode = "\e[0m"; // default terminal color
                break;
            case LogSeverity::Info:
                _ss << "[Info]  ";
                ansiCode = "\e[96m"; // cyan
                break;
            case LogSeverity::Warning:
                _ss << "[Warn]  ";
                ansiCode = "\e[93m"; // yellow
                break;
            case LogSeverity::Error:
                _ss << "[Error] ";
                ansiCode = "\e[91m"; // red
                break;
            case LogSeverity::Fatal:
                _ss << "[FATAL] ";
                ansiCode = "\e[31m"; // dark red
                break;
        }

        _ss << '(' << fileName << ':' << location.line() << ") ";
        _ss << message;

#ifndef NDEBUG
        std::cout << ansiCode << _ss.str() << "\e[0m" << std::endl;
#endif
    }
}
