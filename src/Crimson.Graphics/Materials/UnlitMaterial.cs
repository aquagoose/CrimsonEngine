namespace Crimson.Graphics.Materials;

/// <summary>
/// A standard material with no lighting or shadows applied.
/// </summary>
public sealed class UnlitMaterial(Texture texture, MaterialInfo info = new()) : Material("Materials/Unlit", in info)
{
    /// <summary>
    /// The <see cref="Crimson.Graphics.Texture"/> to apply to the material. 
    /// </summary>
    public Texture Texture = texture;

    protected internal override ReadOnlySpan<Texture> Textures => new ReadOnlySpan<Texture>(ref Texture);
}