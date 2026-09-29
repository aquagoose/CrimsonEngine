#!/usr/bin/env dotnet
#:project ../../src/Crimson.Engine/Crimson.Engine.csproj

using System.Numerics;
using Crimson.Core;
using Crimson.Engine;
using Crimson.Graphics;
using Crimson.Graphics.Materials;
using Crimson.Graphics.Primitives;

Logger.LogToConsole = true;

AppInfo info = new AppInfo("Simple Mesh Test", "1.0.0");
App.Run(in info, new SimpleMeshTest());

class SimpleMeshTest : Application
{
    private Texture _texture = null!;
    private UnlitMaterial _material = null!;
    private Renderable _renderable = null!;
    private float _rotation;

    public override void Init()
    {
        Renderer.BackgroundColor = Color.CornflowerBlue;
        
        _texture = new Texture("Content/DEBUG.png");
        _material = new UnlitMaterial(_texture);

        Cube cube = new Cube();
        _renderable = new Renderable(cube, _material);
        
        base.Init();
    }

    public override void Loop(float dt)
    {
        _rotation += dt;
        if (_rotation >= float.Pi * 2)
            _rotation -= float.Pi * 2;
        
        Renderer.AddCamera(Camera.Perspective(new Vector3(0, 0, 3), -Vector3.UnitZ, Vector3.UnitY, float.DegreesToRadians(45), Renderer.Size, 0.1f, 100f));
        Renderer.DrawRenderable(_renderable, Matrix4x4.CreateFromYawPitchRoll(_rotation, _rotation * 2, _rotation));
        
        base.Loop(dt);
    }

    public override void Dispose()
    {
        _renderable.Dispose();
        _material.Dispose();
        _texture.Dispose();
        
        base.Dispose();
    }
}