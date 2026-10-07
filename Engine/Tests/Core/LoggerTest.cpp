#include <Core/Logger.h>

int main(int argc, char* argv[])
{
    CGE_TRACE("{} message", "Trace");
    CGE_DEBUG("{} message", "Debug");
    CGE_INFO("{} message", "Info");
    CGE_WARN("{} message", "Warning");
    CGE_ERROR("{} message", "Error");
    CGE_FATAL("{} message", "Fatal");

    return 0;
}