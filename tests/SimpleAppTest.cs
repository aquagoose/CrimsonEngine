#!/usr/bin/env dotnet
#:project ../src/Crimson.Engine/Crimson.Engine.csproj

using Crimson.Engine;

AppInfo appInfo = new("Simple App Test", "1.0.0");
App.Run(in appInfo);