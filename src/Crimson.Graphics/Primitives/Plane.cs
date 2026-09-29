using System.Numerics;

namespace Crimson.Graphics.Primitives;

/// <summary>
/// A 2D plane with no depth.
/// </summary>
public class Plane : IPrimitive
{
    public Vertex[] Vertices { get; } =
    [
        new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector2(1, 1), Vector3.UnitZ, Color.White),
        new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector2(1, 0), Vector3.UnitZ, Color.White),
        new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector2(0, 0), Vector3.UnitZ, Color.White),
        new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0, 1), Vector3.UnitZ, Color.White),
    ];

    public uint[] Indices { get; } =
    [
        0, 1, 3,
        1, 2, 3
    ];
}