namespace Crimson.Graphics;

/// <summary>
/// Represents various texture address modes when the texture coordinates are out of the 0-1 range.
/// </summary>
public enum AddressMode
{
    /// <summary>
    /// Repeat the texture.
    /// </summary>
    Repeat,
    
    /// <summary>
    /// Clamp to edge.
    /// </summary>
    Clamp
}