#!/usr/bin/env dotnet
#:project ../../src/Crimson.Engine/Crimson.Engine.csproj

using System.Numerics;
using Crimson.Engine;
using Crimson.Engine.Entities;
using Crimson.Graphics;
using Crimson.Graphics.Materials;
using Crimson.Graphics.Primitives;
using Crimson.Input;
using Crimson.Platform;

AppInfo info = new AppInfo("Skybox Test", "1.0.0");
App.Run(in info, new SkyboxTest());

class SkyboxTest : Scene
{
    private Skybox _skybox = null!;
    private Texture _texture = null!;
    private Material _material = null!;
    private Renderable _renderable = null!;
    private float _value;

    public override void Init()
    {
        _skybox = new Skybox("Content/Skybox/right.jpg", "Content/Skybox/left.jpg", "Content/Skybox/top.jpg",
            "Content/Skybox/bottom.jpg", "Content/Skybox/front.jpg", "Content/Skybox/back.jpg");

        _texture = new Texture("Content/DEBUG.png");
        _texture.Sampler = Sampler.NearestRepeat;
        _material = new UnlitMaterial(_texture);
        _renderable = new Renderable(new Cube(), _material);
        
        base.Init();
    }

    public override void Loop(float dt)
    {
        if (Input.IsKeyPressed(Key.Escape))
            App.Close();
        
        _value += dt * 0.5f;
        if (_value >= float.Pi * 5) 
            _value -= float.Pi * 2 * 5;

        float pitch = float.Sin(_value * 0.4f) * 0.75f;
        float yaw = _value;

        const float distance = 5;
        Quaternion euler = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);
        Vector3 position = new Vector3(float.Cos(pitch) * float.Sin(yaw), float.Sin(-pitch), float.Cos(pitch) * float.Cos(yaw)) * distance;
        Vector3 forward = Vector3.Transform(-Vector3.UnitZ, euler);
        Vector3 up = Vector3.Transform(Vector3.UnitY, euler);

        Renderer.AddCamera(Camera.Perspective(position, forward, up, float.DegreesToRadians(45), Renderer.Size, 0.1f,
            100f, _skybox));
        
        Renderer.DrawRenderable(_renderable, Matrix4x4.Identity);
        
        base.Loop(dt);
    }

    public override void Dispose()
    {
        _renderable.Dispose();
        _material.Dispose();
        _texture.Dispose();
        _skybox.Dispose();
        
        base.Dispose();
    }
}