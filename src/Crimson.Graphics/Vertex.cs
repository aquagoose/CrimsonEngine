using System.Numerics;

namespace Crimson.Graphics;

/// <summary>
/// Defines the parameters for each vertex of a <see cref="Mesh"/>.
/// </summary>
public struct Vertex
{
    /// <summary>
    /// The position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The texture coordinate.
    /// </summary>
    public Vector2 TexCoord;

    /// <summary>
    /// The normal vector.
    /// </summary>
    public Vector3 Normal;

    /// <summary>
    /// The vertex color.
    /// </summary>
    public Color Color;

    public Vertex(Vector3 position, Vector2 texCoord, Vector3 normal, Color color)
    {
        Position = position;
        TexCoord = texCoord;
        Normal = normal;
        Color = color;
    }
}