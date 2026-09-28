using System.Numerics;
using Crimson.Math;

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

    public static Camera Perspective(Vector3 position, Vector3 forward, Vector3 up, float fov, Size<uint> viewportSize, float near, float far)
    {
        return new Camera
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, viewportSize.Width / (float) viewportSize.Height, near, far),
            View = Matrix4x4.CreateLookAt(position, position + forward, up),
            Position = position
        };
    }
}