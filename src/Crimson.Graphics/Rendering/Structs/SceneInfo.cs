namespace Crimson.Graphics.Rendering.Structs;

internal readonly struct SceneInfo
{
    public readonly Camera Camera;

    public SceneInfo(Camera camera)
    {
        Camera = camera;
    }
}