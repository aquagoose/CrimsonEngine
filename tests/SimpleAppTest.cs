#!/usr/bin/env dotnet
#:project ../src/Crimson.Engine/Crimson.Engine.csproj

using Crimson.Engine;
using Crimson.Engine.Actors;
using Crimson.Graphics;

AppInfo appInfo = new("Simple App Test", "1.0.0");
App.Run(in appInfo, new TestApp());

class TestApp : Scene
{
    public override void Init()
    {
        base.Init();
        Renderer.BackgroundColor = Color.Crimson;
    }
}