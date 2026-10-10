using System.Numerics;

namespace Crimson.Engine.Actors;

public struct Transform
{
    public Vector3 Position;

    public Quaternion Rotation;

    public Vector3 Scale;

    public Vector3 Origin;

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
        Origin = Vector3.Zero;
    }
    
    public Transform(Vector3 position) : this()
    {
        Position = position;
    }

    public Transform(Vector3 position, Quaternion rotation) : this()
    {
        Position = position;
        Rotation = rotation;
    }
}