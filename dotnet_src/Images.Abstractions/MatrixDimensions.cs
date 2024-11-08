namespace Images.Abstractions;

public class MatrixDimensions
{
    public MatrixDimensions(int width, int height)
    {
        // TODO maybe just rename these all to dim 0 and dim 1
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    protected bool Equals(MatrixDimensions other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MatrixDimensions)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }
}