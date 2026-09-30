namespace Crimson.Graphics.Rendering.Structs;

internal ref struct ClearInfo
{
    public readonly Color Color;

    public bool HasCleared;

    public ClearInfo(Color color, bool hasCleared)
    {
        Color = color;
        HasCleared = hasCleared;
    }
}