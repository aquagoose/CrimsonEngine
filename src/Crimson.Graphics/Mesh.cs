using Crimson.Graphics.SDLGPU;
using piko.SDL3;

namespace Crimson.Graphics;

/// <summary>
/// A mesh contains vertices, indices, and a material, and can be drawn by the <see cref="Renderer"/>.
/// </summary>
public class Mesh : IDisposable
{
    private readonly GPUContext _context;

    internal readonly SDL.GPUBuffer VertexBuffer;

    internal readonly SDL.GPUBuffer IndexBuffer;

    internal readonly uint NumElements;

    public Mesh(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices)
    {
        _context = Renderer.Context;
        NumElements = (uint) indices.Length;

        VertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, vertices);
        IndexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, indices);
    }

    public void Dispose()
    {
        SDL.ReleaseGPUBuffer(_context.Device, IndexBuffer);
        SDL.ReleaseGPUBuffer(_context.Device, VertexBuffer);
    }
}