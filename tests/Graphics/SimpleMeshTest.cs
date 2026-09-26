#!/usr/bin/env dotnet
#:project ../../src/Crimson.Engine/Crimson.Engine.csproj

using Crimson.Engine;
using Crimson.Graphics;
using Crimson.Graphics.Materials;

AppInfo info = new AppInfo("Simple Mesh Test", "1.0.0");
App.Run(in info, new SimpleMeshTest());

class SimpleMeshTest : Application
{
    private Texture _texture = null!;
    private UnlitMaterial _material = null!;

    public override void Init()
    {
        _texture = new Texture("Content/DEBUG.png");
        _material = new UnlitMaterial(_texture);
        
        base.Init();
    }

    public override void Dispose()
    {
        _material.Dispose();
        _texture.Dispose();
        
        base.Dispose();
    }
}