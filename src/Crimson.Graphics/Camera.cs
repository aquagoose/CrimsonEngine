using System.Numerics;

namespace Crimson.Graphics;

public struct Camera
{
    public Matrix4x4 Projection;

    public Matrix4x4 View;

    public Vector3 Position;

    private float _padding;

    public Camera(Matrix4x4 projection, Matrix4x4 view, Vector3 position)
    {
        Projection = projection;
        View = view;
        Position = position;
    }
}