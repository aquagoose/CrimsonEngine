namespace Crimson.Graphics.Materials;

/// <summary>
/// A material using the Unlit shader, where no lighting is applied to the object.
/// </summary>
public sealed class UnlitMaterial : Material
{
    public UnlitMaterial(in MaterialInfo info) : base(in info, "Materials/Unlit") { }
}