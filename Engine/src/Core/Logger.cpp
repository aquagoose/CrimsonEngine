#include "Crimson/Core/Logger.h"

#include <sstream>
#include <chrono>
#include <format>
#include <filesystem>
#include <iostream>

namespace cge
{
    std::stringstream _ss;

    void Logger::Log(Severity severity, const std::string_view& message, std::source_location location)
    {
        _ss.str("");
        auto now = std::chrono::system_clock::now();
        auto fileName = std::filesystem::path(location.file_name());

        _ss << std::format("{:%F %T} ", std::chrono::round<std::chrono::milliseconds>(now));

        std::string ansi;
        switch (severity)
        {
            case Severity::Trace:
                _ss << "[Trace] ";
                ansi = "90"; // gray
                break;
            case Severity::Debug:
                _ss << "[Debug] ";
                ansi = "0"; // default terminal color
                break;
            case Severity::Info:
                _ss << "[Info]  ";
                ansi = "96"; // cyan
                break;
            case Severity::Warning:
                _ss << "[Warn]  ";
                ansi = "93"; // yellow
                break;
            case Severity::Error:
                _ss << "[Error] ";
                ansi = "91"; // red
                break;
            case Severity::Fatal:
                _ss << "[FATAL] ";
                ansi = "31"; // dark red
                break;
        }

        _ss << '(' << fileName.filename().c_str() << ':' << location.line() << ") ";
        _ss << message;

        std::cout << "\e[" << ansi << "m" << _ss.str() << "\e[0m" << std::endl;
    }
}
