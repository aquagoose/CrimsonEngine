using System.Numerics;
using Crimson.Graphics.Rendering.Structs;
using Crimson.Math;
using piko.SDL3;

namespace Crimson.Graphics.Rendering;

internal sealed class ForwardPlusRenderer : IRenderer3D
{
    private readonly List<Draw> _opaques;
    
    public ForwardPlusRenderer()
    {
        _opaques = [];
    }

    public void AddToDrawQueue(Renderable renderable, Matrix4x4 world)
    {
        _opaques.Add(new Draw());
    }
    
    public void Render(SDL.GPUCommandBuffer cb, SDL.GPUTexture colorTarget, SDL.GPUTexture depthTarget, Size<uint> viewportSize,
        ref readonly Camera camera, ref ClearInfo clear)
    {
        
    }

    private struct Draw
    {
        public Renderable Renderable;
        public Matrix4x4 WorldMatrix;

        public Draw(Renderable renderable, Matrix4x4 worldMatrix)
        {
            Renderable = renderable;
            WorldMatrix = worldMatrix;
        }
    }
}