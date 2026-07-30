#!/usr/bin/env dotnet
#:project Crimson.Graphics.Tests.Common

using Crimson.Graphics;
using Crimson.Graphics.Tests.Common;

using HelloWorldTest test = new();
test.Run();

internal class HelloWorldTest() : TestBase("Hello World Test")
{
    private Texture _texture;

    protected override void Init()
    {
        _texture = new Texture("Content/bagel.png");
    }

    public override void Dispose()
    {
        _texture.Dispose();
        base.Dispose();
    }
}