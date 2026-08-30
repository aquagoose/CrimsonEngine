#!/usr/bin/env dotnet
#:project Crimson.Graphics.Tests.Common/Crimson.Graphics.Tests.Common.csproj

using Crimson.Graphics.Tests.Common;

using HelloWorldTest test = new HelloWorldTest();
test.Run();

class HelloWorldTest() : TestBase("Hello World")
{
    
}