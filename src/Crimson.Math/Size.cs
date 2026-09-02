using System.Numerics;

namespace Crimson.Math;

/// <summary>
/// A 2-dimensional size with a width and height.
/// <typeparam name="T">A numeric type.</typeparam>
/// </summary>
public readonly struct Size<T> : IEquatable<Size<T>> where T : INumber<T>
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
    /// Construct a <see cref="Size{T}"/> with the given width and height.
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
    /// <param name="wh">The value to apply to both the width and height.</param>
    public Size(T wh)
    {
        Width = wh;
        Height = wh;
    }

    /// <summary>
    /// Cast the elements of this size into another type.
    /// </summary>
    /// <typeparam name="TOther">A numeric type.</typeparam>
    /// <returns>A new size where the elements are of type <see cref="TOther"/>.</returns>
    public Size<TOther> As<TOther>() where TOther : INumber<TOther>
        => new Size<TOther>(TOther.CreateChecked(Width), TOther.CreateChecked(Height));

    public static bool operator ==(Size<T> left, Size<T> right)
        => left.Width == right.Width && left.Height == right.Height;

    public static bool operator !=(Size<T> left, Size<T> right)
        => left.Width != right.Width || left.Height != right.Height;

    /// <inheritdoc />
    public bool Equals(Size<T> other)
        => this == other;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Size<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Width, Height);

    /// <inheritdoc />
    public override string ToString()
        => $"{Width}x{Height}";
}