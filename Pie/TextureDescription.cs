namespace Pie;

/// <summary>
/// Used to describe how a new <see cref="Texture"/> should be stored and sampled from.
/// </summary>
public struct TextureDescription
{
    /// <summary>
    /// The type of this texture.
    /// </summary>
    public TextureType TextureType;

    /// <summary>
    /// The width of this texture.
    /// </summary>
    public int Width;

    /// <summary>
    /// The height of this texture.
    /// </summary>
    public int Height;

    /// <summary>
    /// The depth of this texture.
    /// </summary>
    public int Depth;

    /// <summary>
    /// The <see cref="Pie.Format"/> of this texture.
    /// </summary>
    public Format Format;

    /// <summary>
    /// The number of mipmaps this texture will hold.
    /// </summary>
    public int MipLevels;

    /// <summary>
    /// The size of the texture array in elements.
    /// </summary>
    public int ArraySize;

    /*/// <summary>
    /// Whether or not this texture is dynamic.
    /// </summary>
    public bool Dynamic;*/

    /// <summary>
    /// The <see cref="Pie.TextureUsage"/> of this texture.
    /// </summary>
    public TextureUsage Usage;

    /// <summary>
    /// Create a new 1D <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="width">The width of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="arraySize">The size of the texture array in elements.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public TextureDescription(int width, Format format, int mipLevels, int arraySize, TextureUsage usage)
    {
        TextureType = TextureType.Texture1D;
        Width = width;
        Height = 0;
        Depth = 0;
        Format = format;
        MipLevels = mipLevels;
        ArraySize = arraySize;
        Usage = usage;
    }
    
    /// <summary>
    /// Create a new 2D <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="width">The width of this texture.</param>
    /// <param name="height">The height of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="arraySize">The size of the texture array in elements.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public TextureDescription(int width, int height, Format format, int mipLevels, int arraySize, TextureUsage usage)
    {
        TextureType = TextureType.Texture2D;
        Width = width;
        Height = height;
        Depth = 0;
        Format = format;
        MipLevels = mipLevels;
        ArraySize = arraySize;
        Usage = usage;
    }
    
    /// <summary>
    /// Create a new 3D <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="width">The width of this texture.</param>
    /// <param name="height">The height of this texture.</param>
    /// <param name="depth">The depth of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="arraySize">The size of the texture array in elements.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public TextureDescription(int width, int height, int depth, Format format, int mipLevels, int arraySize, TextureUsage usage)
    {
        TextureType = TextureType.Texture3D;
        Width = width;
        Height = height;
        Depth = depth;
        Format = format;
        MipLevels = mipLevels;
        ArraySize = arraySize;
        Usage = usage;
    }

    /// <summary>
    /// Create a new <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="textureType">The type of this texture.</param>
    /// <param name="width">The width of this texture.</param>
    /// <param name="height">The height of this texture.</param>
    /// <param name="depth">The depth of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="arraySize">The size of the texture array in elements.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public TextureDescription(TextureType textureType, int width, int height, int depth, Format format, int mipLevels, int arraySize, TextureUsage usage)
    {
        TextureType = textureType;
        Width = width;
        Height = height;
        Depth = depth;
        Format = format;
        MipLevels = mipLevels;
        ArraySize = arraySize;
        Usage = usage;
    }

    /// <summary>
    /// Check if this <see cref="TextureDescription"/> is valid.
    /// </summary>
    public Validity Validity
    {
        get
        {
            if (Width < 0 || Height < 0 || Depth < 0)
                return new Validity(false, "Texture width, height, and depth must be at least 0.");

            if (MipLevels < 0)
                return new Validity(false, "Mipmap levels must be at least 0.");
            
            if (ArraySize < 1)
                return new Validity(false, "Array size must be at least 1.");

            if (ArraySize > 1 && TextureType == TextureType.Texture3D)
                return new Validity(false, "3D textures do not support an array size of >1.");

            if (TextureType == TextureType.Cubemap && Width != Height)
                return new Validity(false, "Cubemap width must equal height.");

            return new Validity(true, null);
        }
    }
}