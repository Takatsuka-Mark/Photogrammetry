namespace LinearAlgebra;

public record Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinate Add(Coordinate addend)
    {
        return new Coordinate { X = X + addend.X, Y = Y + addend.Y };
    }
    
    public bool IsInPositiveBounds(LinearAlgebra.MatrixDimensions dimensions)
    {
        // TODO move out of here?
        return X >= 0 && X < dimensions.Width && Y >= 0 && Y < dimensions.Height;
    }
}