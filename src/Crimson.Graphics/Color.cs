namespace Crimson.Graphics;

/// <summary>
/// Represents a floating-point RGBA color.
/// </summary>
public struct Color : IEquatable<Color>
{
    /// <summary>
    /// The red component.
    /// </summary>
    public float R;

    /// <summary>
    /// The green component.
    /// </summary>
    public float G;

    /// <summary>
    /// The blue component.
    /// </summary>
    public float B;

    /// <summary>
    /// The alpha component.
    /// </summary>
    public float A;

    /// <summary>
    /// Get/set the red component as a byte.
    /// </summary>
    public byte Rb
    {
        get => (byte) (R * byte.MaxValue);
        set => R = value / (float) byte.MaxValue;
    }

    /// <summary>
    /// Get/set the green component as a byte.
    /// </summary>
    public byte Gb
    {
        get => (byte) (G * byte.MaxValue);
        set => G = value / (float) byte.MaxValue;
    }

    /// <summary>
    /// Get/set the blue component as a byte.
    /// </summary>
    public byte Bb
    {
        get => (byte) (B * byte.MaxValue);
        set => B = value / (float) byte.MaxValue;
    }

    /// <summary>
    /// Get/set the alpha component as a byte.
    /// </summary>
    public byte Ab
    {
        get => (byte) (A * byte.MaxValue);
        set => A = value / (float) byte.MaxValue;
    }

    /// <summary>
    /// Create a <see cref="Color"/> from floating-point values.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
    public Color(float r, float g, float b, float a = 1.0f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Create a <see cref="Color"/> from 8-bit values.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
    public Color(byte r, byte g, byte b, byte a = byte.MaxValue)
    {
        R = r / (float) byte.MaxValue;
        G = g / (float) byte.MaxValue;
        B = b / (float) byte.MaxValue;
        A = a / (float) byte.MaxValue;
    }

    public bool Equals(Color other)
    {
        return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
    }

    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{Rb}, {Gb}, {Bb}, {Ab}";
    }
}