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

    public override void Init()
    {
        Renderer.BackgroundColor = Color.CornflowerBlue;
        
        _texture = new Texture("Content/DEBUG.png");
        _material = new UnlitMaterial(_texture);

        ReadOnlySpan<Vertex> vertices =
        [
            new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0, 0), new Vector3(0, 0, -1), Color.White),
            new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector2(0, 1), new Vector3(0, 0, -1), Color.White),
            new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector2(1, 1), new Vector3(0, 0, -1), Color.White),
            new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector2(0, 0), new Vector3(0, 0, -1), Color.White),
        ];

        ReadOnlySpan<uint> indices =
        [
            0, 1, 3,
            1, 2, 3
        ];

        _renderable = new Renderable(vertices, indices, _material);
        
        base.Init();
    }

    public override void Dispose()
    {
        _renderable.Dispose();
        _material.Dispose();
        _texture.Dispose();
        
        base.Dispose();
    }
}