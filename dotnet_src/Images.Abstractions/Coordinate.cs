namespace Images.Abstractions;

public class Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinate Add(Coordinate addend)
    {
        return new Coordinate { X = X + addend.X, Y = Y + addend.Y };
    }
    
    public bool IsInPositiveBounds(MatrixDimensions dimensions)
    {
        return X >= 0 && X < dimensions.Width && Y >= 0 && Y < dimensions.Height;
    }
}