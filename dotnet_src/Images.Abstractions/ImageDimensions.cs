namespace Images.Abstractions;

public class ImageDimensions(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;

    protected bool Equals(ImageDimensions other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ImageDimensions)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }
}