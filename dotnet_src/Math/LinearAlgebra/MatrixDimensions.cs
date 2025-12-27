namespace LinearAlgebra;

public record MatrixDimensions
{
    public required uint Rows { get; init; }
    public required uint Cols { get; init; }

    public override string ToString()
    {
        return $"({Rows} rows, {Cols} cols)";
    }
}