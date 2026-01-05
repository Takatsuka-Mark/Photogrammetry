namespace LinearAlgebra;

public record MatrixDimensions
{
    public required ushort Rows { get; init; }
    public required ushort Cols { get; init; }

    public override string ToString()
    {
        return $"({Rows} rows, {Cols} cols)";
    }

    public virtual bool Equals(MatrixDimensions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Rows == other.Rows && Cols == other.Cols;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Rows, Cols);
    }
}