namespace Crimson.Graphics.Rendering.Structs;

internal readonly ref struct SceneInfo
{
    public readonly ShaderCamera Camera;

    public SceneInfo(ShaderCamera camera)
    {
        Camera = camera;
    }
}