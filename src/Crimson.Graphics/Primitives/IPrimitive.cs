namespace Crimson.Graphics.Primitives;

/// <summary>
/// Implements a simple primitive mesh.
/// </summary>
public interface IPrimitive
{
    /// <summary>
    /// The vertices.
    /// </summary>
    public Vertex[] Vertices { get; }
    
    /// <summary>
    /// The indices.
    /// </summary>
    public uint[] Indices { get; }
}