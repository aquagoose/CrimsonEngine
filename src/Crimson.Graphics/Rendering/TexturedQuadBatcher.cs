using System.Numerics;
using Crimson.Graphics.SDLGPU;
using piko.SDL3;

namespace Crimson.Graphics.Rendering;

/// <summary>
/// Batches 2D textured quads together.
/// </summary>
internal unsafe class TexturedQuadBatcher : IDisposable
{
    /// <summary>
    /// The initial number of quads the batch can support, before expansion.
    /// </summary>
    private const uint InitialBatchSize = 4096;

    /// <summary>
    /// The number of vertices per quad.
    /// </summary>
    private const uint NumVertices = 4;

    /// <summary>
    /// The number of indices per quad.
    /// </summary>
    private const uint NumIndices = 6;

    private readonly GPUContext _context;

    // dynamically resizing vertex and index lists
    private Vertex[] _vertices;
    private uint[] _indices;

    // dynamic resizing vertex and index buffers
    private SDL.GPUBuffer _vertexBuffer;
    private SDL.GPUBuffer _indexBuffer;

    public TexturedQuadBatcher(GPUContext context)
    {
        _context = context;

        _vertices = new Vertex[InitialBatchSize * NumVertices];
        _indices = new uint[InitialBatchSize * NumIndices];

        _vertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, (uint) (_vertices.Length * sizeof(Vertex)));
        _indexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, (uint) (_indices.Length * sizeof(uint)));
    }

    public void Dispose()
    {
        SDL.ReleaseGPUBuffer(_context.Device, _indexBuffer);
        SDL.ReleaseGPUBuffer(_context.Device, _vertexBuffer);
    }

    public readonly struct Vertex
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;
        public readonly Color Color;

        public Vertex(Vector2 position, Vector2 texCoord, Color color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
    }
}