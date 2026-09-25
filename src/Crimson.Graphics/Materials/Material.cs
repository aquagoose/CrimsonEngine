using piko.SDL3;

namespace Crimson.Graphics.Materials;

/// <summary>
/// A material can be applied to a <see cref="Mesh"/>, and determines the mesh's appearance.
/// </summary>
public class Material : IDisposable
{
    private readonly RenderContext _context;
    
    internal readonly SDL.GPUGraphicsPipeline Pipeline;

    protected Material(string shader)
    {
        _context = Renderer.Context;

        SDL.GPUShader vertexShader = _context.CreateShader(SDL.GPUShaderStage.Vertex, shader);
        SDL.GPUShader pixelShader = _context.CreateShader(SDL.GPUShaderStage.Fragment, shader);
        
        
    }
    
    public virtual void Dispose()
    {
        
    }
}