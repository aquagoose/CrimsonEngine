namespace Crimson.Graphics;

/// <summary>
/// Defines how <see cref="Texture"/>s are sampled by the renderer.
/// </summary>
public record struct Sampler
{
    /// <summary>
    /// The minification <see cref="Filter"/>.
    /// </summary>
    public Filter MinFilter;

    /// <summary>
    /// The magnification <see cref="Filter"/>.
    /// </summary>
    public Filter MagFilter;

    /// <summary>
    /// The mipmap <see cref="Filter"/>.
    /// </summary>
    public Filter MipFilter;

    /// <summary>
    /// The <see cref="AddressMode"/> for the U texture coordinate.
    /// </summary>
    public AddressMode AddressU;

    /// <summary>
    /// The <see cref="AddressMode"/> for the V texture coordinate.
    /// </summary>
    public AddressMode AddressV;

    /// <summary>
    /// Allow anisotropic filtering. If <see langword="true"/>, the anisotropy will be set according to the
    /// <see cref="Renderer"/> settings, however setting this to <see langword="false"/> will disable anistropy
    /// regardless of the setting.
    /// </summary>
    public bool AllowAnisotropy;

    public Sampler(Filter minFilter, Filter magFilter, Filter mipFilter, AddressMode addressU, AddressMode addressV, bool allowAnisotropy = true)
    {
        MinFilter = minFilter;
        MagFilter = magFilter;
        MipFilter = mipFilter;
        AddressU = addressU;
        AddressV = addressV;
        AllowAnisotropy = allowAnisotropy;
    }

    public Sampler(Filter filter, AddressMode addressMode) : this(filter, filter, filter, addressMode, addressMode) { }

    public static Sampler LinearRepeat => new Sampler(Filter.Linear, AddressMode.Repeat);

    public static Sampler LinearClamp => new Sampler(Filter.Linear, AddressMode.Clamp);

    public static Sampler NearestRepeat => new Sampler(Filter.Nearest, AddressMode.Repeat);

    public static Sampler NearestClamp => new Sampler(Filter.Nearest, AddressMode.Clamp);
}