#!/usr/bin/env dotnet
#:project ../../src/Crimson.Engine/Crimson.Engine.csproj

using System.Numerics;
using Crimson.Engine;
using Crimson.Graphics;
using Crimson.Graphics.Materials;

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

        ReadOnlySpan<Vertex> vertices =
        [
            new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0, 0), new Vector3(0, 0, -1), new Color(1.0f, 0.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector2(0, 1), new Vector3(0, 0, -1), new Color(0.0f, 1.0f, 0.0f)),
            new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector2(1, 1), new Vector3(0, 0, -1), new Color(0.0f, 0.0f, 1.0f)),
            new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector2(0, 0), new Vector3(0, 0, -1), new Color(0.0f, 0.0f, 0.0f)),
        ];

        ReadOnlySpan<uint> indices =
        [
            0, 1, 3,
            1, 2, 3
        ];

        _renderable = new Renderable(vertices, indices, _material);
        
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