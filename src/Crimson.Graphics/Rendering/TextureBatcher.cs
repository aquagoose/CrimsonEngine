using piko.SDL3;

namespace Crimson.Graphics.Rendering;

internal sealed class TextureBatcher : IDisposable
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
    }

    public void Dispose()
    {

    }
}