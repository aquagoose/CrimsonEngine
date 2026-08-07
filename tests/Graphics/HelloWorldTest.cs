#!/usr/bin/env dotnet
#:project Crimson.Graphics.Tests.Common

using System.Numerics;
using Crimson.Graphics;
using Crimson.Graphics.Tests.Common;

using HelloWorldTest test = new();
test.Run();

internal class HelloWorldTest() : TestBase("Hello World Test")
{
    private Texture _texture;
    private float _value;

    protected override void Init()
    {
        _texture = new Texture("Content/bagel.png");
    }

    protected override void Loop(float dt)
    {
        _value += dt;
        if (_value >= float.Pi * 2)
            _value -= float.Pi * 2;

        for (int i = 0; i < 10; i++)
        {
            Renderer.DrawImage(_texture, new Vector2(i * 20 + float.Sin(_value + i) * 200, i * 50));
        }
    }

    public override void Dispose()
    {
        _texture.Dispose();
        base.Dispose();
    }
}