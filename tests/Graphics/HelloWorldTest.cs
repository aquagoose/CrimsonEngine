#!/usr/bin/env dotnet
#:project Crimson.Graphics.Tests.Common

using Crimson.Graphics.Tests.Common;

using HelloWorldTest test = new();
test.Run();

internal class HelloWorldTest() : TestBase("Hello World Test")
{
    
}