using System.Numerics;

namespace Crimson.Math;

/// <summary>
/// A 2-dimensional Size with a width and height.
/// </summary>
public struct Size<T> where T : INumber<T>
{
    /// <summary>
    /// The width.
    /// </summary>
    public readonly T Width;

    /// <summary>
    /// The height.
    /// </summary>
    public readonly T Height;

    /// <summary>
    /// Construct a <see cref="Size{T}"/> from a width and height.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Size(T width, T height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Construct a <see cref="Size{T}"/> from a scalar value.
    /// </summary>
    /// <param name="wh">The scalar value to apply to the width and height.</param>
    public Size(T wh)
    {
        Width = wh;
        Height = wh;
    }
}