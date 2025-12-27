namespace Images.Abstractions;

public class MatrixDimensions
{
    // TODO maybe just rename these all to dim 0 and dim 1

    public required int Width { get; init; }
    public required int Height { get; init; }

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

    public override string ToString()
    {
        return $"Width: {Width}, Height: {Height}";
    }
}