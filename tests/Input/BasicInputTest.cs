#!/usr/bin/env dotnet
#:project ../../src/Crimson.Engine/Crimson.Engine.csproj

using Crimson.Engine;
using Crimson.Engine.Actors;
using Crimson.Graphics;
using Crimson.Input;
using Crimson.Platform;

AppInfo info = new AppInfo("Basic Input Test", "1.0.0");
App.Run(in info, new BasicInputTest());

class BasicInputTest : Scene
{
    public override void Init()
    {
        Renderer.BackgroundColor = Color.CornflowerBlue;
        
        base.Init();
    }

    public override void Loop(float dt)
    {
        if (Input.IsKeyPressed(Key.Escape))
            App.Close();
        
        if (Input.IsKeyPressed(Key.Space))
            Console.WriteLine("Space key pressed!");
        if (Input.IsKeyDown(Key.Enter))
            Console.WriteLine("Enter key down!");
        
        if (Input.IsMouseButtonDown(MouseButton.Left))
            Console.WriteLine($"Mouse pos: {Input.MousePosition}, delta: {Input.MouseDelta}");
        if (Input.IsMouseButtonPressed(MouseButton.Right))
            Console.WriteLine("Right mouse button pressed!");
        
        base.Loop(dt);
    }
}