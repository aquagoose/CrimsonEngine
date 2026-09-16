#!/usr/bin/env dotnet
#:project ../src/Crimson.Core/Crimson.Core.csproj

using System.Diagnostics;
using Crimson.Core;

#if DEBUG
Debug.Assert(Logger.LogToConsole == true);
#else
Logger.LogToConsole = true
#endif

Logger.Trace("Trace message");
Logger.Debug("Debug message");
Logger.Info("Info message");
Logger.Warn("Warning message");
Logger.Error("Error message");
Logger.Fatal("Fatal message");