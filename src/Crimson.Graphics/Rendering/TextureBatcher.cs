using System.Numerics;
using piko.SDL3;
using Index = uint;

namespace Crimson.Graphics.Rendering;

internal sealed unsafe class TextureBatcher : IDisposable
{
    /// <summary>
    /// The initial maximum number of draws, before expansion.
    /// </summary>
    private const uint InitialMaxDrawCount = 4096;

    /// <summary>
    /// Number of vertices per sprite.
    /// </summary>
    private const uint NumVertices = 4;

    /// <summary>
    /// Number of indices per sprite.
    /// </summary>
    private const uint NumIndices = 6;

    private readonly RenderContext _context;

    private SDL.GPUBuffer _vertexBuffer;
    private SDL.GPUBuffer _indexBuffer;
    private uint _maxDraws;

    public TextureBatcher(RenderContext context, SDL.GPUTextureFormat targetFormat)
    {
        _context = context;
        _maxDraws = InitialMaxDrawCount;

        _vertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, InitialMaxDrawCount * NumVertices * (uint) sizeof(Vertex));
        _indexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, InitialMaxDrawCount * NumIndices * sizeof(Index));


    }

    public void Dispose()
    {

    }

    private readonly struct Vertex(Vector2 position, Vector2 texCoord, Vector4 tint)
    {
        public readonly Vector2 Position = position;
        public readonly Vector2 TexCoord = texCoord;
        public readonly Vector4 Tint = tint; // todo color struct
    }
}