using System.Numerics;
using Crimson.Math;

namespace Crimson.Graphics;

public struct Camera
{
    public Matrix4x4 Projection;

    public Matrix4x4 View;

    public Size<uint> ViewportSize;

    public Vector3 Position;

    private float _padding1;

    public Camera(Matrix4x4 projection, Matrix4x4 view, Size<uint> viewportSize, Vector3 position)
    {
        Projection = projection;
        View = view;
        ViewportSize = viewportSize;
        Position = position;
    }

    public static Camera Perspective(Vector3 position, Vector3 forward, Vector3 up, float fov, Size<uint> viewportSize, float near, float far)
    {
        return new Camera
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, viewportSize.Width / (float) viewportSize.Height, near, far),
            View = Matrix4x4.CreateLookAt(position, position + forward, up),
            ViewportSize = viewportSize,
            Position = position
        };
    }
}