namespace Crimson.Graphics.Rendering.Structs;

internal struct ClearInfo
{
    public Color Color;

    public bool HasCleared;

    public ClearInfo(Color color, bool hasCleared)
    {
        Color = color;
        HasCleared = hasCleared;
    }
}