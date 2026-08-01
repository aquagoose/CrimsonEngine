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

    /// <summary>
    /// Get this Color as an 8-bit RGBA string.
    /// </summary>
    public override string ToString()
    {
        return $"{Rb}, {Gb}, {Bb}, {Ab}";
    }

    /// <summary>
    /// AliceBlue has an RGB value of 240, 248, 255 (0xF0F8FF)
    /// </summary>
    public static Color AliceBlue => new Color(0.9411764705882353f, 0.9725490196078431f, 1.0f);

    /// <summary>
    /// AntiqueWhite has an RGB value of 250, 235, 215 (0xFAEBD7)
    /// </summary>
    public static Color AntiqueWhite => new Color(0.9803921568627451f, 0.9215686274509803f, 0.8431372549019608f);

    /// <summary>
    /// Aqua has an RGB value of 0, 255, 255 (0x00FFFF)
    /// </summary>
    public static Color Aqua => new Color(0.0f, 1.0f, 1.0f);

    /// <summary>
    /// Aquamarine has an RGB value of 127, 255, 212 (0x7FFFD4)
    /// </summary>
    public static Color Aquamarine => new Color(0.4980392156862745f, 1.0f, 0.8313725490196079f);

    /// <summary>
    /// Azure has an RGB value of 240, 255, 255 (0xF0FFFF)
    /// </summary>
    public static Color Azure => new Color(0.9411764705882353f, 1.0f, 1.0f);

    /// <summary>
    /// Beige has an RGB value of 245, 245, 220 (0xF5F5DC)
    /// </summary>
    public static Color Beige => new Color(0.9607843137254902f, 0.9607843137254902f, 0.8627450980392157f);

    /// <summary>
    /// Bisque has an RGB value of 255, 228, 196 (0xFFE4C4)
    /// </summary>
    public static Color Bisque => new Color(1.0f, 0.8941176470588236f, 0.7686274509803922f);

    /// <summary>
    /// Black has an RGB value of 0, 0, 0 (0x000000)
    /// </summary>
    public static Color Black => new Color(0.0f, 0.0f, 0.0f);

    /// <summary>
    /// BlanchedAlmond has an RGB value of 255, 235, 205 (0xFFEBCD)
    /// </summary>
    public static Color BlanchedAlmond => new Color(1.0f, 0.9215686274509803f, 0.803921568627451f);

    /// <summary>
    /// Blue has an RGB value of 0, 0, 255 (0x0000FF)
    /// </summary>
    public static Color Blue => new Color(0.0f, 0.0f, 1.0f);

    /// <summary>
    /// BlueViolet has an RGB value of 138, 43, 226 (0x8A2BE2)
    /// </summary>
    public static Color BlueViolet => new Color(0.5411764705882353f, 0.16862745098039217f, 0.8862745098039215f);

    /// <summary>
    /// Brown has an RGB value of 165, 42, 42 (0xA52A2A)
    /// </summary>
    public static Color Brown => new Color(0.6470588235294118f, 0.16470588235294117f, 0.16470588235294117f);

    /// <summary>
    /// BurlyWood has an RGB value of 222, 184, 135 (0xDEB887)
    /// </summary>
    public static Color BurlyWood => new Color(0.8705882352941177f, 0.7215686274509804f, 0.5294117647058824f);

    /// <summary>
    /// CadetBlue has an RGB value of 95, 158, 160 (0x5F9EA0)
    /// </summary>
    public static Color CadetBlue => new Color(0.37254901960784315f, 0.6196078431372549f, 0.6274509803921569f);

    /// <summary>
    /// Chartreuse has an RGB value of 127, 255, 0 (0x7FFF00)
    /// </summary>
    public static Color Chartreuse => new Color(0.4980392156862745f, 1.0f, 0.0f);

    /// <summary>
    /// Chocolate has an RGB value of 210, 105, 30 (0xD2691E)
    /// </summary>
    public static Color Chocolate => new Color(0.8235294117647058f, 0.4117647058823529f, 0.11764705882352941f);

    /// <summary>
    /// Coral has an RGB value of 255, 127, 80 (0xFF7F50)
    /// </summary>
    public static Color Coral => new Color(1.0f, 0.4980392156862745f, 0.3137254901960784f);

    /// <summary>
    /// CornflowerBlue has an RGB value of 100, 149, 237 (0x6495ED)
    /// </summary>
    public static Color CornflowerBlue => new Color(0.39215686274509803f, 0.5843137254901961f, 0.9294117647058824f);

    /// <summary>
    /// Cornsilk has an RGB value of 255, 248, 220 (0xFFF8DC)
    /// </summary>
    public static Color Cornsilk => new Color(1.0f, 0.9725490196078431f, 0.8627450980392157f);

    /// <summary>
    /// Crimson has an RGB value of 220, 20, 60 (0xDC143C)
    /// </summary>
    public static Color Crimson => new Color(0.8627450980392157f, 0.0784313725490196f, 0.23529411764705882f);

    /// <summary>
    /// Cyan has an RGB value of 0, 255, 255 (0x00FFFF)
    /// </summary>
    public static Color Cyan => new Color(0.0f, 1.0f, 1.0f);

    /// <summary>
    /// DarkBlue has an RGB value of 0, 0, 139 (0x00008B)
    /// </summary>
    public static Color DarkBlue => new Color(0.0f, 0.0f, 0.5450980392156862f);

    /// <summary>
    /// DarkCyan has an RGB value of 0, 139, 139 (0x008B8B)
    /// </summary>
    public static Color DarkCyan => new Color(0.0f, 0.5450980392156862f, 0.5450980392156862f);

    /// <summary>
    /// DarkGoldenRod has an RGB value of 184, 134, 11 (0xB8860B)
    /// </summary>
    public static Color DarkGoldenRod => new Color(0.7215686274509804f, 0.5254901960784314f, 0.043137254901960784f);

    /// <summary>
    /// DarkGray has an RGB value of 169, 169, 169 (0xA9A9A9)
    /// </summary>
    public static Color DarkGray => new Color(0.6627450980392157f, 0.6627450980392157f, 0.6627450980392157f);

    /// <summary>
    /// DarkGrey has an RGB value of 169, 169, 169 (0xA9A9A9)
    /// </summary>
    public static Color DarkGrey => new Color(0.6627450980392157f, 0.6627450980392157f, 0.6627450980392157f);

    /// <summary>
    /// DarkGreen has an RGB value of 0, 100, 0 (0x006400)
    /// </summary>
    public static Color DarkGreen => new Color(0.0f, 0.39215686274509803f, 0.0f);

    /// <summary>
    /// DarkKhaki has an RGB value of 189, 183, 107 (0xBDB76B)
    /// </summary>
    public static Color DarkKhaki => new Color(0.7411764705882353f, 0.7176470588235294f, 0.4196078431372549f);

    /// <summary>
    /// DarkMagenta has an RGB value of 139, 0, 139 (0x8B008B)
    /// </summary>
    public static Color DarkMagenta => new Color(0.5450980392156862f, 0.0f, 0.5450980392156862f);

    /// <summary>
    /// DarkOliveGreen has an RGB value of 85, 107, 47 (0x556B2F)
    /// </summary>
    public static Color DarkOliveGreen => new Color(0.3333333333333333f, 0.4196078431372549f, 0.1843137254901961f);

    /// <summary>
    /// DarkOrange has an RGB value of 255, 140, 0 (0xFF8C00)
    /// </summary>
    public static Color DarkOrange => new Color(1.0f, 0.5490196078431373f, 0.0f);

    /// <summary>
    /// DarkOrchid has an RGB value of 153, 50, 204 (0x9932CC)
    /// </summary>
    public static Color DarkOrchid => new Color(0.6f, 0.19607843137254902f, 0.8f);

    /// <summary>
    /// DarkRed has an RGB value of 139, 0, 0 (0x8B0000)
    /// </summary>
    public static Color DarkRed => new Color(0.5450980392156862f, 0.0f, 0.0f);

    /// <summary>
    /// DarkSalmon has an RGB value of 233, 150, 122 (0xE9967A)
    /// </summary>
    public static Color DarkSalmon => new Color(0.9137254901960784f, 0.5882352941176471f, 0.47843137254901963f);

    /// <summary>
    /// DarkSeaGreen has an RGB value of 143, 188, 143 (0x8FBC8F)
    /// </summary>
    public static Color DarkSeaGreen => new Color(0.5607843137254902f, 0.7372549019607844f, 0.5607843137254902f);

    /// <summary>
    /// DarkSlateBlue has an RGB value of 72, 61, 139 (0x483D8B)
    /// </summary>
    public static Color DarkSlateBlue => new Color(0.2823529411764706f, 0.23921568627450981f, 0.5450980392156862f);

    /// <summary>
    /// DarkSlateGray has an RGB value of 47, 79, 79 (0x2F4F4F)
    /// </summary>
    public static Color DarkSlateGray => new Color(0.1843137254901961f, 0.30980392156862746f, 0.30980392156862746f);

    /// <summary>
    /// DarkSlateGrey has an RGB value of 47, 79, 79 (0x2F4F4F)
    /// </summary>
    public static Color DarkSlateGrey => new Color(0.1843137254901961f, 0.30980392156862746f, 0.30980392156862746f);

    /// <summary>
    /// DarkTurquoise has an RGB value of 0, 206, 209 (0x00CED1)
    /// </summary>
    public static Color DarkTurquoise => new Color(0.0f, 0.807843137254902f, 0.8196078431372549f);

    /// <summary>
    /// DarkViolet has an RGB value of 148, 0, 211 (0x9400D3)
    /// </summary>
    public static Color DarkViolet => new Color(0.5803921568627451f, 0.0f, 0.8274509803921568f);

    /// <summary>
    /// DeepPink has an RGB value of 255, 20, 147 (0xFF1493)
    /// </summary>
    public static Color DeepPink => new Color(1.0f, 0.0784313725490196f, 0.5764705882352941f);

    /// <summary>
    /// DeepSkyBlue has an RGB value of 0, 191, 255 (0x00BFFF)
    /// </summary>
    public static Color DeepSkyBlue => new Color(0.0f, 0.7490196078431373f, 1.0f);

    /// <summary>
    /// DimGray has an RGB value of 105, 105, 105 (0x696969)
    /// </summary>
    public static Color DimGray => new Color(0.4117647058823529f, 0.4117647058823529f, 0.4117647058823529f);

    /// <summary>
    /// DimGrey has an RGB value of 105, 105, 105 (0x696969)
    /// </summary>
    public static Color DimGrey => new Color(0.4117647058823529f, 0.4117647058823529f, 0.4117647058823529f);

    /// <summary>
    /// DodgerBlue has an RGB value of 30, 144, 255 (0x1E90FF)
    /// </summary>
    public static Color DodgerBlue => new Color(0.11764705882352941f, 0.5647058823529412f, 1.0f);

    /// <summary>
    /// FireBrick has an RGB value of 178, 34, 34 (0xB22222)
    /// </summary>
    public static Color FireBrick => new Color(0.6980392156862745f, 0.13333333333333333f, 0.13333333333333333f);

    /// <summary>
    /// FloralWhite has an RGB value of 255, 250, 240 (0xFFFAF0)
    /// </summary>
    public static Color FloralWhite => new Color(1.0f, 0.9803921568627451f, 0.9411764705882353f);

    /// <summary>
    /// ForestGreen has an RGB value of 34, 139, 34 (0x228B22)
    /// </summary>
    public static Color ForestGreen => new Color(0.13333333333333333f, 0.5450980392156862f, 0.13333333333333333f);

    /// <summary>
    /// Fuchsia has an RGB value of 255, 0, 255 (0xFF00FF)
    /// </summary>
    public static Color Fuchsia => new Color(1.0f, 0.0f, 1.0f);

    /// <summary>
    /// Gainsboro has an RGB value of 220, 220, 220 (0xDCDCDC)
    /// </summary>
    public static Color Gainsboro => new Color(0.8627450980392157f, 0.8627450980392157f, 0.8627450980392157f);

    /// <summary>
    /// GhostWhite has an RGB value of 248, 248, 255 (0xF8F8FF)
    /// </summary>
    public static Color GhostWhite => new Color(0.9725490196078431f, 0.9725490196078431f, 1.0f);

    /// <summary>
    /// Gold has an RGB value of 255, 215, 0 (0xFFD700)
    /// </summary>
    public static Color Gold => new Color(1.0f, 0.8431372549019608f, 0.0f);

    /// <summary>
    /// GoldenRod has an RGB value of 218, 165, 32 (0xDAA520)
    /// </summary>
    public static Color GoldenRod => new Color(0.8549019607843137f, 0.6470588235294118f, 0.12549019607843137f);

    /// <summary>
    /// Gray has an RGB value of 128, 128, 128 (0x808080)
    /// </summary>
    public static Color Gray => new Color(0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f);

    /// <summary>
    /// Grey has an RGB value of 128, 128, 128 (0x808080)
    /// </summary>
    public static Color Grey => new Color(0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f);

    /// <summary>
    /// Green has an RGB value of 0, 128, 0 (0x008000)
    /// </summary>
    public static Color Green => new Color(0.0f, 0.5019607843137255f, 0.0f);

    /// <summary>
    /// GreenYellow has an RGB value of 173, 255, 47 (0xADFF2F)
    /// </summary>
    public static Color GreenYellow => new Color(0.6784313725490196f, 1.0f, 0.1843137254901961f);

    /// <summary>
    /// HoneyDew has an RGB value of 240, 255, 240 (0xF0FFF0)
    /// </summary>
    public static Color HoneyDew => new Color(0.9411764705882353f, 1.0f, 0.9411764705882353f);

    /// <summary>
    /// HotPink has an RGB value of 255, 105, 180 (0xFF69B4)
    /// </summary>
    public static Color HotPink => new Color(1.0f, 0.4117647058823529f, 0.7058823529411765f);

    /// <summary>
    /// IndianRed has an RGB value of 205, 92, 92 (0xCD5C5C)
    /// </summary>
    public static Color IndianRed => new Color(0.803921568627451f, 0.3607843137254902f, 0.3607843137254902f);

    /// <summary>
    /// Indigo has an RGB value of 75, 0, 130 (0x4B0082)
    /// </summary>
    public static Color Indigo => new Color(0.29411764705882354f, 0.0f, 0.5098039215686274f);

    /// <summary>
    /// Ivory has an RGB value of 255, 255, 240 (0xFFFFF0)
    /// </summary>
    public static Color Ivory => new Color(1.0f, 1.0f, 0.9411764705882353f);

    /// <summary>
    /// Khaki has an RGB value of 240, 230, 140 (0xF0E68C)
    /// </summary>
    public static Color Khaki => new Color(0.9411764705882353f, 0.9019607843137255f, 0.5490196078431373f);

    /// <summary>
    /// Lavender has an RGB value of 230, 230, 250 (0xE6E6FA)
    /// </summary>
    public static Color Lavender => new Color(0.9019607843137255f, 0.9019607843137255f, 0.9803921568627451f);

    /// <summary>
    /// LavenderBlush has an RGB value of 255, 240, 245 (0xFFF0F5)
    /// </summary>
    public static Color LavenderBlush => new Color(1.0f, 0.9411764705882353f, 0.9607843137254902f);

    /// <summary>
    /// LawnGreen has an RGB value of 124, 252, 0 (0x7CFC00)
    /// </summary>
    public static Color LawnGreen => new Color(0.48627450980392156f, 0.9882352941176471f, 0.0f);

    /// <summary>
    /// LemonChiffon has an RGB value of 255, 250, 205 (0xFFFACD)
    /// </summary>
    public static Color LemonChiffon => new Color(1.0f, 0.9803921568627451f, 0.803921568627451f);

    /// <summary>
    /// LightBlue has an RGB value of 173, 216, 230 (0xADD8E6)
    /// </summary>
    public static Color LightBlue => new Color(0.6784313725490196f, 0.8470588235294118f, 0.9019607843137255f);

    /// <summary>
    /// LightCoral has an RGB value of 240, 128, 128 (0xF08080)
    /// </summary>
    public static Color LightCoral => new Color(0.9411764705882353f, 0.5019607843137255f, 0.5019607843137255f);

    /// <summary>
    /// LightCyan has an RGB value of 224, 255, 255 (0xE0FFFF)
    /// </summary>
    public static Color LightCyan => new Color(0.8784313725490196f, 1.0f, 1.0f);

    /// <summary>
    /// LightGoldenRodYellow has an RGB value of 250, 250, 210 (0xFAFAD2)
    /// </summary>
    public static Color LightGoldenRodYellow => new Color(0.9803921568627451f, 0.9803921568627451f, 0.8235294117647058f);

    /// <summary>
    /// LightGray has an RGB value of 211, 211, 211 (0xD3D3D3)
    /// </summary>
    public static Color LightGray => new Color(0.8274509803921568f, 0.8274509803921568f, 0.8274509803921568f);

    /// <summary>
    /// LightGrey has an RGB value of 211, 211, 211 (0xD3D3D3)
    /// </summary>
    public static Color LightGrey => new Color(0.8274509803921568f, 0.8274509803921568f, 0.8274509803921568f);

    /// <summary>
    /// LightGreen has an RGB value of 144, 238, 144 (0x90EE90)
    /// </summary>
    public static Color LightGreen => new Color(0.5647058823529412f, 0.9333333333333333f, 0.5647058823529412f);

    /// <summary>
    /// LightPink has an RGB value of 255, 182, 193 (0xFFB6C1)
    /// </summary>
    public static Color LightPink => new Color(1.0f, 0.7137254901960784f, 0.7568627450980392f);

    /// <summary>
    /// LightSalmon has an RGB value of 255, 160, 122 (0xFFA07A)
    /// </summary>
    public static Color LightSalmon => new Color(1.0f, 0.6274509803921569f, 0.47843137254901963f);

    /// <summary>
    /// LightSeaGreen has an RGB value of 32, 178, 170 (0x20B2AA)
    /// </summary>
    public static Color LightSeaGreen => new Color(0.12549019607843137f, 0.6980392156862745f, 0.6666666666666666f);

    /// <summary>
    /// LightSkyBlue has an RGB value of 135, 206, 250 (0x87CEFA)
    /// </summary>
    public static Color LightSkyBlue => new Color(0.5294117647058824f, 0.807843137254902f, 0.9803921568627451f);

    /// <summary>
    /// LightSlateGray has an RGB value of 119, 136, 153 (0x778899)
    /// </summary>
    public static Color LightSlateGray => new Color(0.4666666666666667f, 0.5333333333333333f, 0.6f);

    /// <summary>
    /// LightSlateGrey has an RGB value of 119, 136, 153 (0x778899)
    /// </summary>
    public static Color LightSlateGrey => new Color(0.4666666666666667f, 0.5333333333333333f, 0.6f);

    /// <summary>
    /// LightSteelBlue has an RGB value of 176, 196, 222 (0xB0C4DE)
    /// </summary>
    public static Color LightSteelBlue => new Color(0.6901960784313725f, 0.7686274509803922f, 0.8705882352941177f);

    /// <summary>
    /// LightYellow has an RGB value of 255, 255, 224 (0xFFFFE0)
    /// </summary>
    public static Color LightYellow => new Color(1.0f, 1.0f, 0.8784313725490196f);

    /// <summary>
    /// Lime has an RGB value of 0, 255, 0 (0x00FF00)
    /// </summary>
    public static Color Lime => new Color(0.0f, 1.0f, 0.0f);

    /// <summary>
    /// LimeGreen has an RGB value of 50, 205, 50 (0x32CD32)
    /// </summary>
    public static Color LimeGreen => new Color(0.19607843137254902f, 0.803921568627451f, 0.19607843137254902f);

    /// <summary>
    /// Linen has an RGB value of 250, 240, 230 (0xFAF0E6)
    /// </summary>
    public static Color Linen => new Color(0.9803921568627451f, 0.9411764705882353f, 0.9019607843137255f);

    /// <summary>
    /// Magenta has an RGB value of 255, 0, 255 (0xFF00FF)
    /// </summary>
    public static Color Magenta => new Color(1.0f, 0.0f, 1.0f);

    /// <summary>
    /// Maroon has an RGB value of 128, 0, 0 (0x800000)
    /// </summary>
    public static Color Maroon => new Color(0.5019607843137255f, 0.0f, 0.0f);

    /// <summary>
    /// MediumAquaMarine has an RGB value of 102, 205, 170 (0x66CDAA)
    /// </summary>
    public static Color MediumAquaMarine => new Color(0.4f, 0.803921568627451f, 0.6666666666666666f);

    /// <summary>
    /// MediumBlue has an RGB value of 0, 0, 205 (0x0000CD)
    /// </summary>
    public static Color MediumBlue => new Color(0.0f, 0.0f, 0.803921568627451f);

    /// <summary>
    /// MediumOrchid has an RGB value of 186, 85, 211 (0xBA55D3)
    /// </summary>
    public static Color MediumOrchid => new Color(0.7294117647058823f, 0.3333333333333333f, 0.8274509803921568f);

    /// <summary>
    /// MediumPurple has an RGB value of 147, 112, 219 (0x9370DB)
    /// </summary>
    public static Color MediumPurple => new Color(0.5764705882352941f, 0.4392156862745098f, 0.8588235294117647f);

    /// <summary>
    /// MediumSeaGreen has an RGB value of 60, 179, 113 (0x3CB371)
    /// </summary>
    public static Color MediumSeaGreen => new Color(0.23529411764705882f, 0.7019607843137254f, 0.44313725490196076f);

    /// <summary>
    /// MediumSlateBlue has an RGB value of 123, 104, 238 (0x7B68EE)
    /// </summary>
    public static Color MediumSlateBlue => new Color(0.4823529411764706f, 0.40784313725490196f, 0.9333333333333333f);

    /// <summary>
    /// MediumSpringGreen has an RGB value of 0, 250, 154 (0x00FA9A)
    /// </summary>
    public static Color MediumSpringGreen => new Color(0.0f, 0.9803921568627451f, 0.6039215686274509f);

    /// <summary>
    /// MediumTurquoise has an RGB value of 72, 209, 204 (0x48D1CC)
    /// </summary>
    public static Color MediumTurquoise => new Color(0.2823529411764706f, 0.8196078431372549f, 0.8f);

    /// <summary>
    /// MediumVioletRed has an RGB value of 199, 21, 133 (0xC71585)
    /// </summary>
    public static Color MediumVioletRed => new Color(0.7803921568627451f, 0.08235294117647059f, 0.5215686274509804f);

    /// <summary>
    /// MidnightBlue has an RGB value of 25, 25, 112 (0x191970)
    /// </summary>
    public static Color MidnightBlue => new Color(0.09803921568627451f, 0.09803921568627451f, 0.4392156862745098f);

    /// <summary>
    /// MintCream has an RGB value of 245, 255, 250 (0xF5FFFA)
    /// </summary>
    public static Color MintCream => new Color(0.9607843137254902f, 1.0f, 0.9803921568627451f);

    /// <summary>
    /// MistyRose has an RGB value of 255, 228, 225 (0xFFE4E1)
    /// </summary>
    public static Color MistyRose => new Color(1.0f, 0.8941176470588236f, 0.8823529411764706f);

    /// <summary>
    /// Moccasin has an RGB value of 255, 228, 181 (0xFFE4B5)
    /// </summary>
    public static Color Moccasin => new Color(1.0f, 0.8941176470588236f, 0.7098039215686275f);

    /// <summary>
    /// NavajoWhite has an RGB value of 255, 222, 173 (0xFFDEAD)
    /// </summary>
    public static Color NavajoWhite => new Color(1.0f, 0.8705882352941177f, 0.6784313725490196f);

    /// <summary>
    /// Navy has an RGB value of 0, 0, 128 (0x000080)
    /// </summary>
    public static Color Navy => new Color(0.0f, 0.0f, 0.5019607843137255f);

    /// <summary>
    /// OldLace has an RGB value of 253, 245, 230 (0xFDF5E6)
    /// </summary>
    public static Color OldLace => new Color(0.9921568627450981f, 0.9607843137254902f, 0.9019607843137255f);

    /// <summary>
    /// Olive has an RGB value of 128, 128, 0 (0x808000)
    /// </summary>
    public static Color Olive => new Color(0.5019607843137255f, 0.5019607843137255f, 0.0f);

    /// <summary>
    /// OliveDrab has an RGB value of 107, 142, 35 (0x6B8E23)
    /// </summary>
    public static Color OliveDrab => new Color(0.4196078431372549f, 0.5568627450980392f, 0.13725490196078433f);

    /// <summary>
    /// Orange has an RGB value of 255, 165, 0 (0xFFA500)
    /// </summary>
    public static Color Orange => new Color(1.0f, 0.6470588235294118f, 0.0f);

    /// <summary>
    /// OrangeRed has an RGB value of 255, 69, 0 (0xFF4500)
    /// </summary>
    public static Color OrangeRed => new Color(1.0f, 0.27058823529411763f, 0.0f);

    /// <summary>
    /// Orchid has an RGB value of 218, 112, 214 (0xDA70D6)
    /// </summary>
    public static Color Orchid => new Color(0.8549019607843137f, 0.4392156862745098f, 0.8392156862745098f);

    /// <summary>
    /// PaleGoldenRod has an RGB value of 238, 232, 170 (0xEEE8AA)
    /// </summary>
    public static Color PaleGoldenRod => new Color(0.9333333333333333f, 0.9098039215686274f, 0.6666666666666666f);

    /// <summary>
    /// PaleGreen has an RGB value of 152, 251, 152 (0x98FB98)
    /// </summary>
    public static Color PaleGreen => new Color(0.596078431372549f, 0.984313725490196f, 0.596078431372549f);

    /// <summary>
    /// PaleTurquoise has an RGB value of 175, 238, 238 (0xAFEEEE)
    /// </summary>
    public static Color PaleTurquoise => new Color(0.6862745098039216f, 0.9333333333333333f, 0.9333333333333333f);

    /// <summary>
    /// PaleVioletRed has an RGB value of 219, 112, 147 (0xDB7093)
    /// </summary>
    public static Color PaleVioletRed => new Color(0.8588235294117647f, 0.4392156862745098f, 0.5764705882352941f);

    /// <summary>
    /// PapayaWhip has an RGB value of 255, 239, 213 (0xFFEFD5)
    /// </summary>
    public static Color PapayaWhip => new Color(1.0f, 0.9372549019607843f, 0.8352941176470589f);

    /// <summary>
    /// PeachPuff has an RGB value of 255, 218, 185 (0xFFDAB9)
    /// </summary>
    public static Color PeachPuff => new Color(1.0f, 0.8549019607843137f, 0.7254901960784313f);

    /// <summary>
    /// Peru has an RGB value of 205, 133, 63 (0xCD853F)
    /// </summary>
    public static Color Peru => new Color(0.803921568627451f, 0.5215686274509804f, 0.24705882352941178f);

    /// <summary>
    /// Pink has an RGB value of 255, 192, 203 (0xFFC0CB)
    /// </summary>
    public static Color Pink => new Color(1.0f, 0.7529411764705882f, 0.796078431372549f);

    /// <summary>
    /// Plum has an RGB value of 221, 160, 221 (0xDDA0DD)
    /// </summary>
    public static Color Plum => new Color(0.8666666666666667f, 0.6274509803921569f, 0.8666666666666667f);

    /// <summary>
    /// PowderBlue has an RGB value of 176, 224, 230 (0xB0E0E6)
    /// </summary>
    public static Color PowderBlue => new Color(0.6901960784313725f, 0.8784313725490196f, 0.9019607843137255f);

    /// <summary>
    /// Purple has an RGB value of 128, 0, 128 (0x800080)
    /// </summary>
    public static Color Purple => new Color(0.5019607843137255f, 0.0f, 0.5019607843137255f);

    /// <summary>
    /// RebeccaPurple has an RGB value of 102, 51, 153 (0x663399)
    /// </summary>
    public static Color RebeccaPurple => new Color(0.4f, 0.2f, 0.6f);

    /// <summary>
    /// Red has an RGB value of 255, 0, 0 (0xFF0000)
    /// </summary>
    public static Color Red => new Color(1.0f, 0.0f, 0.0f);

    /// <summary>
    /// RosyBrown has an RGB value of 188, 143, 143 (0xBC8F8F)
    /// </summary>
    public static Color RosyBrown => new Color(0.7372549019607844f, 0.5607843137254902f, 0.5607843137254902f);

    /// <summary>
    /// RoyalBlue has an RGB value of 65, 105, 225 (0x4169E1)
    /// </summary>
    public static Color RoyalBlue => new Color(0.2549019607843137f, 0.4117647058823529f, 0.8823529411764706f);

    /// <summary>
    /// SaddleBrown has an RGB value of 139, 69, 19 (0x8B4513)
    /// </summary>
    public static Color SaddleBrown => new Color(0.5450980392156862f, 0.27058823529411763f, 0.07450980392156863f);

    /// <summary>
    /// Salmon has an RGB value of 250, 128, 114 (0xFA8072)
    /// </summary>
    public static Color Salmon => new Color(0.9803921568627451f, 0.5019607843137255f, 0.4470588235294118f);

    /// <summary>
    /// SandyBrown has an RGB value of 244, 164, 96 (0xF4A460)
    /// </summary>
    public static Color SandyBrown => new Color(0.9568627450980393f, 0.6431372549019608f, 0.3764705882352941f);

    /// <summary>
    /// SeaGreen has an RGB value of 46, 139, 87 (0x2E8B57)
    /// </summary>
    public static Color SeaGreen => new Color(0.1803921568627451f, 0.5450980392156862f, 0.3411764705882353f);

    /// <summary>
    /// SeaShell has an RGB value of 255, 245, 238 (0xFFF5EE)
    /// </summary>
    public static Color SeaShell => new Color(1.0f, 0.9607843137254902f, 0.9333333333333333f);

    /// <summary>
    /// Sienna has an RGB value of 160, 82, 45 (0xA0522D)
    /// </summary>
    public static Color Sienna => new Color(0.6274509803921569f, 0.3215686274509804f, 0.17647058823529413f);

    /// <summary>
    /// Silver has an RGB value of 192, 192, 192 (0xC0C0C0)
    /// </summary>
    public static Color Silver => new Color(0.7529411764705882f, 0.7529411764705882f, 0.7529411764705882f);

    /// <summary>
    /// SkyBlue has an RGB value of 135, 206, 235 (0x87CEEB)
    /// </summary>
    public static Color SkyBlue => new Color(0.5294117647058824f, 0.807843137254902f, 0.9215686274509803f);

    /// <summary>
    /// SlateBlue has an RGB value of 106, 90, 205 (0x6A5ACD)
    /// </summary>
    public static Color SlateBlue => new Color(0.41568627450980394f, 0.35294117647058826f, 0.803921568627451f);

    /// <summary>
    /// SlateGray has an RGB value of 112, 128, 144 (0x708090)
    /// </summary>
    public static Color SlateGray => new Color(0.4392156862745098f, 0.5019607843137255f, 0.5647058823529412f);

    /// <summary>
    /// SlateGrey has an RGB value of 112, 128, 144 (0x708090)
    /// </summary>
    public static Color SlateGrey => new Color(0.4392156862745098f, 0.5019607843137255f, 0.5647058823529412f);

    /// <summary>
    /// Snow has an RGB value of 255, 250, 250 (0xFFFAFA)
    /// </summary>
    public static Color Snow => new Color(1.0f, 0.9803921568627451f, 0.9803921568627451f);

    /// <summary>
    /// SpringGreen has an RGB value of 0, 255, 127 (0x00FF7F)
    /// </summary>
    public static Color SpringGreen => new Color(0.0f, 1.0f, 0.4980392156862745f);

    /// <summary>
    /// SteelBlue has an RGB value of 70, 130, 180 (0x4682B4)
    /// </summary>
    public static Color SteelBlue => new Color(0.27450980392156865f, 0.5098039215686274f, 0.7058823529411765f);

    /// <summary>
    /// Tan has an RGB value of 210, 180, 140 (0xD2B48C)
    /// </summary>
    public static Color Tan => new Color(0.8235294117647058f, 0.7058823529411765f, 0.5490196078431373f);

    /// <summary>
    /// Teal has an RGB value of 0, 128, 128 (0x008080)
    /// </summary>
    public static Color Teal => new Color(0.0f, 0.5019607843137255f, 0.5019607843137255f);

    /// <summary>
    /// Thistle has an RGB value of 216, 191, 216 (0xD8BFD8)
    /// </summary>
    public static Color Thistle => new Color(0.8470588235294118f, 0.7490196078431373f, 0.8470588235294118f);

    /// <summary>
    /// Tomato has an RGB value of 255, 99, 71 (0xFF6347)
    /// </summary>
    public static Color Tomato => new Color(1.0f, 0.38823529411764707f, 0.2784313725490196f);

    /// <summary>
    /// Turquoise has an RGB value of 64, 224, 208 (0x40E0D0)
    /// </summary>
    public static Color Turquoise => new Color(0.25098039215686274f, 0.8784313725490196f, 0.8156862745098039f);

    /// <summary>
    /// Violet has an RGB value of 238, 130, 238 (0xEE82EE)
    /// </summary>
    public static Color Violet => new Color(0.9333333333333333f, 0.5098039215686274f, 0.9333333333333333f);

    /// <summary>
    /// Wheat has an RGB value of 245, 222, 179 (0xF5DEB3)
    /// </summary>
    public static Color Wheat => new Color(0.9607843137254902f, 0.8705882352941177f, 0.7019607843137254f);

    /// <summary>
    /// White has an RGB value of 255, 255, 255 (0xFFFFFF)
    /// </summary>
    public static Color White => new Color(1.0f, 1.0f, 1.0f);

    /// <summary>
    /// WhiteSmoke has an RGB value of 245, 245, 245 (0xF5F5F5)
    /// </summary>
    public static Color WhiteSmoke => new Color(0.9607843137254902f, 0.9607843137254902f, 0.9607843137254902f);

    /// <summary>
    /// Yellow has an RGB value of 255, 255, 0 (0xFFFF00)
    /// </summary>
    public static Color Yellow => new Color(1.0f, 1.0f, 0.0f);

    /// <summary>
    /// YellowGreen has an RGB value of 154, 205, 50 (0x9ACD32)
    /// </summary>
    public static Color YellowGreen => new Color(0.6039215686274509f, 0.803921568627451f, 0.19607843137254902f);

    /// <summary>
    /// Transparent has an RGBA value of 0, 0, 0, 0 (0x00000000)
    /// </summary>
    public static Color Transparent => new Color(0.0f, 0.0f, 0.0f, 0.0f);
}