#!/usr/bin/env dotnet
#:project ../src/Crimson.Core/Crimson.Core.csproj

using Crimson.Core;

Logger.Trace("Trace message");
Logger.Debug("Debug message");
Logger.Info("Info message");
Logger.Warn("Warning message");
Logger.Error("Error message");
Logger.Fatal("Fatal message");