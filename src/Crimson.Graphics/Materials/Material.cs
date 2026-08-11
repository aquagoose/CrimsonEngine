using piko.SDL3;

namespace Crimson.Graphics.Materials;

/// <summary>
/// A material describes the appearance of a <see cref="Mesh"/>, containing a shader, textures, and various parameters.
/// </summary>
public abstract class Material : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Material"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    internal readonly SDL.GPUGraphicsPipeline Pipeline;

    protected Material(ref readonly MaterialInfo info, string shader)
    {
        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new()
        {

        }
    }

    /// <summary>
    /// Unload any resources such as textures that have been created in this material.
    /// </summary>
    protected virtual void Unload() { }

    /// <summary>
    /// Dispose of this <see cref="Material"/>.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        Unload();
    }
}