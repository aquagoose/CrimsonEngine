using System.Numerics;

namespace Crimson.Math;

/// <summary>
/// Represents a 2-dimensional Size with a width and height component.
/// </summary>
/// <typeparam name="T">A numeric type.</typeparam>
public struct Size<T> : IEquatable<Size<T>> where T : INumber<T>
{
    /// <summary>
    /// Gets a <see cref="Size{T}"/> where all components are zero.
    /// </summary>
    public static Size<T> Zero => new Size<T>(T.Zero);

    /// <summary>
    /// Gets a <see cref="Size{T}"/> where all components are one.
    /// </summary>
    public static Size<T> One => new Size<T>(T.One);

    /// <summary>
    /// The width.
    /// </summary>
    public T Width;

    /// <summary>
    /// The height.
    /// </summary>
    public T Height;

    /// <summary>
    /// Create a <see cref="Size{T}"/> from a width and height.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Size(T width, T height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Create a <see cref="Size{T}"/> from a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar value to apply to both the width and height.</param>
    public Size(T scalar)
    {
        Width = scalar;
        Height = scalar;
    }

    /// <summary>
    /// Convert this <see cref="Size{T}"/> to a string.
    /// </summary>
    /// <returns>{width}x{height}</returns>
    public override string ToString()
    {
        return $"{Width}x{Height}";
    }

    public bool Equals(Size<T> other)
    {
        return EqualityComparer<T>.Default.Equals(Width, other.Width) && EqualityComparer<T>.Default.Equals(Height, other.Height);
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