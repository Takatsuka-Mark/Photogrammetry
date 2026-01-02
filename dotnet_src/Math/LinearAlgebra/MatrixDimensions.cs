namespace LinearAlgebra;

public record MatrixDimensions
{
    public required ushort Rows { get; init; }
    public required ushort Cols { get; init; }

    public override string ToString()
    {
        return $"({Rows} rows, {Cols} cols)";
    }
}