using Crimson.Graphics.Rendering.Structs;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.Rendering;

internal interface IRenderer3D
{
    public void Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, SDL.GPUTexture depthTarget,
        Size<uint> viewportSize, ref readonly Camera camera, ref ClearInfo clear);
}