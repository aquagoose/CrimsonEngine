using System.Numerics;
using System.Runtime.CompilerServices;

namespace Crimson.Math;

/// <summary>
/// A 4-component vector with an X, Y, Z, and W component.
/// </summary>
/// <typeparam name="T">A numeric type.</typeparam>
public readonly struct Vec4<T> : IEquatable<Vec4<T>> where T : INumber<T>
{
    #region Fields & Constructors

    /// <summary>
    /// The X-component.
    /// </summary>
    public readonly T X;

    /// <summary>
    /// The Y-component.
    /// </summary>
    public readonly T Y;

    /// <summary>
    /// The Z-component.
    /// </summary>
    public readonly T Z;

    /// <summary>
    /// The W-component.
    /// </summary>
    public readonly T W;

    /// <summary>
    /// Create a <see cref="Vec4{T}"/> from the given X, Y, Z, and W components.
    /// </summary>
    /// <param name="x">The X-component.</param>
    /// <param name="y">The Y-component.</param>
    /// <param name="z">The Z-component.</param>
    /// <param name="w">The W-component.</param>
    public Vec4(T x, T y, T z, T w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Create a <see cref="Vec4{T}"/> from a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar value to apply to all components.</param>
    public Vec4(T scalar)
    {
        X = scalar;
        Y = scalar;
        Z = scalar;
        W = scalar;
    }

    #endregion

    #region Operators

    /// <summary>
    /// Check if two <see cref="Vec4{T}"/>'s are equal.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right <see cref="Vec4{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vec4<T> left, Vec4<T> right)
    {
        return left.X == right.X &&
               left.Y == right.Y &&
               left.Z == right.Z &&
               left.W == right.W;
    }

    /// <summary>
    /// Check if two <see cref="Vec4{T}"/>'s are not equal.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right <see cref="Vec4{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vec4<T> left, Vec4<T> right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Add two <see cref="Vec4{T}"/>'s together.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right <see cref="Vec4{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4<T> operator +(Vec4<T> left, Vec4<T> right)
        => new Vec4<T>(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

    /// <summary>
    /// Add subtract a <see cref="Vec4{T}"/> from another.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right <see cref="Vec4{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4<T> operator -(Vec4<T> left, Vec4<T> right)
        => new Vec4<T>(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

    /// <summary>
    /// Multiply two <see cref="Vec4{T}"/>'s together.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right <see cref="Vec4{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4<T> operator *(Vec4<T> left, Vec4<T> right)
        => new Vec4<T>(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);

    /// <summary>
    /// Multiply a <see cref="Vec4{T}"/> by a scalar value.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right scalar value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4<T> operator *(Vec4<T> left, T right)
        => new Vec4<T>(left.X * right, left.Y * right, left.Z * right, left.W * right);

    /// <summary>
    /// Divide one <see cref="Vec4{T}"/>'s by another.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right <see cref="Vec4{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4<T> operator /(Vec4<T> left, Vec4<T> right)
        => new Vec4<T>(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);


    /// <summary>
    /// Divide a <see cref="Vec4{T}"/> by a scalar value.
    /// </summary>
    /// <param name="left">The left <see cref="Vec4{T}"/>.</param>
    /// <param name="right">The right scalar value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4<T> operator /(Vec4<T> left, T right)
        => new Vec4<T>(left.X / right, left.Y / right, left.Z / right, left.W / right);

    #endregion

    #region Methods

    /// <summary>
    /// Checks if this <see cref="Vec4{T}"/> is equal to another <see cref="Vec4{T}"/>.
    /// </summary>
    /// <param name="other">The <see cref="Vec4{T}"/> to compare against.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vec4<T> other)
    {
        return this == other;
    }

    /// <summary>
    /// Checks if this <see cref="Vec4{T}"/> is equal to another <see cref="object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare against.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Vec4<T> other && Equals(other);
    }

    /// <summary>
    /// Gets a hash code for the X, Y, Z, and W components.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    /// <summary>
    /// Gets this <see cref="Vec4{T}"/> as a formatted string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"X: {X}, Y: {Y}, Z: {Z}, W: {W}";
    }

    #endregion

    #region Statics

    /// <summary>
    /// Gets a <see cref="Vec4{T}"/> where all components are zero.
    /// </summary>
    public static Vec4<T> Zero => new Vec4<T>(T.Zero);

    /// <summary>
    /// Gets a <see cref="Vec4{T}"/> where all components are one.
    /// </summary>
    public static Vec4<T> One => new Vec4<T>(T.One);

    /// <summary>
    /// Gets a <see cref="Vec4{T}"/> where the X-component is one, and all other components are zero.
    /// </summary>
    public static Vec4<T> UnitX => new Vec4<T>(T.One, T.Zero, T.Zero, T.Zero);

    /// <summary>
    /// Gets a <see cref="Vec4{T}"/> where the Y-component is one, and all other components are zero.
    /// </summary>
    public static Vec4<T> UnitY => new Vec4<T>(T.Zero, T.One, T.Zero, T.Zero);

    /// <summary>
    /// Gets a <see cref="Vec4{T}"/> where the Z-component is one, and all other components are zero.
    /// </summary>
    public static Vec4<T> UnitZ => new Vec4<T>(T.Zero, T.Zero, T.One, T.Zero);

    /// <summary>
    /// Gets a <see cref="Vec4{T}"/> where the W-component is one, and all other components are zero.
    /// </summary>
    public static Vec4<T> UnitW => new Vec4<T>(T.Zero, T.Zero, T.Zero, T.One);

    #endregion
}