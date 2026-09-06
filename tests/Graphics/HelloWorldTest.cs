#!/usr/bin/env dotnet
#:project Crimson.Graphics.Tests.Common/Crimson.Graphics.Tests.Common.csproj

using Crimson.Graphics;
using Crimson.Graphics.Tests.Common;

using HelloWorldTest test = new HelloWorldTest();
test.Run();

class HelloWorldTest() : TestBase("Hello World")
{
    private Texture _texture = null!;

    protected override void Init()
    {
        _texture = new Texture(Path.Combine("Content", "DEBUG.png"));
    }

    public override void Dispose()
    {
        _texture.Dispose();
        
        base.Dispose();
    }
}