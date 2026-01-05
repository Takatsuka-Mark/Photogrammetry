namespace LinearAlgebra;

public record MatrixDimensions
{
    public required ushort Height { get; init; }
    public required ushort Width { get; init; }

    public override string ToString()
    {
        return $"({Width} xs, {Height} ys)";
    }

    public virtual bool Equals(MatrixDimensions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Height == other.Height && Width == other.Width;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Height, Width);
    }
}