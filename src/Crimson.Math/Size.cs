using System.Numerics;

namespace Crimson.Math;

/// <summary>
/// A 2-dimensional Size with a width and height.
/// </summary>
public struct Size<T> : IEquatable<Size<T>> where T : INumber<T>
{
    public static Size<T> Zero => new Size<T>(T.Zero);
    
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

    /// <summary>
    /// Gets this size as a string, in the format {Width}x{Height}
    /// </summary>
    public override string ToString()
        => $"{Width}x{Height}";

    public bool Equals(Size<T> other)
    {
        return EqualityComparer<T>.Default.Equals(Width, other.Width) &&
               EqualityComparer<T>.Default.Equals(Height, other.Height);
    }

    public override bool Equals(object? obj)
    {
        return obj is Size<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public static bool operator ==(Size<T> left, Size<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Size<T> left, Size<T> right)
    {
        return !left.Equals(right);
    }
}