#!/usr/bin/env dotnet
#:project ../../src/Crimson.Engine/Crimson.Engine.csproj

using Crimson.Engine;
using Crimson.Graphics;

AppInfo info = new AppInfo("Skybox Test", "1.0.0");
App.Run(in info, new SkyboxTest());

class SkyboxTest : Application
{
    private Skybox _skybox = null!;

    public override void Init()
    {
        _skybox = new Skybox("Content/Skybox/left.jpg", "Content/Skybox/right.jpg", "Content/Skybox/top.jpg",
            "Content/Skybox/bottom.jpg", "Content/Skybox/front.jpg", "Content/Skybox/back.jpg");
        
        base.Init();
    }

    public override void Dispose()
    {
        _skybox.Dispose();
        
        base.Dispose();
    }
}