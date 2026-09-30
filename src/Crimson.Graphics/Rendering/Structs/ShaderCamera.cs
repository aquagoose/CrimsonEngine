using System.Numerics;
using Crimson.Math;

namespace Crimson.Graphics.Rendering.Structs;

public readonly ref struct ShaderCamera
{
    public readonly Matrix4x4 Projection;

    public readonly Matrix4x4 View;

    public readonly Size<uint> ViewportSize;

    public readonly Vector3 Position;

    private readonly float _padding;

    public ShaderCamera(ref readonly Camera camera)
    {
        Projection = camera.Projection;
        View = camera.View;
        ViewportSize = camera.ViewportSize;
        Position = camera.Position;
    }
}