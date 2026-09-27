using System.Diagnostics;
using Crimson.Graphics.Materials;
using piko.SDL3;

namespace Crimson.Graphics;

public sealed class Renderable : IDisposable
{
    private readonly RenderContext _context;
    
    internal readonly SDL.GPUBuffer VertexBuffer;
    internal readonly SDL.GPUBuffer IndexBuffer;

    public Material Material;
    
    public Renderable(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices, Material material)
    {
        _context = Renderer.Context;
        Material = material;
        
        Debug.Assert(vertices.Length > 0);
        Debug.Assert(indices.Length > 0,
            "indices.Length > 0: Renderables must have an index buffer. Vertex-only renderables are not yet supported!");

        VertexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Vertex, vertices);
        IndexBuffer = _context.CreateBuffer(SDL.GPUBufferUsageFlags.Index, indices);
    }
    
    public void Dispose()
    {
        SDL.ReleaseGPUBuffer(_context.Device, IndexBuffer);
        SDL.ReleaseGPUBuffer(_context.Device, VertexBuffer);
    }
}