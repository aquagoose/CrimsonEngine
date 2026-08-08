using System.Numerics;

namespace Crimson.Graphics;

/// <summary>
/// The standard vertex type used in <see cref="Mesh"/>es.
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
    /// The normal.
    /// </summary>
    public Vector3 Normal;

    /// <summary>
    /// The color.
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