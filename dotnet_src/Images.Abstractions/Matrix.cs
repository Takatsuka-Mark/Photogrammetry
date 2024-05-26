namespace Images.Abstractions;

public class Matrix<TData>
{
    // TODO maybe eventually allow for creation of N dimensional matrix?
    public MatrixDimensions Dimensions { get; }

    private readonly TData[,] _pixels;

    public Matrix(MatrixDimensions dimensions)
    {
        Dimensions = dimensions;
        _pixels = new TData[dimensions.Width, dimensions.Height];
    }

    public TData this[int x, int y]
    {
        get
        {
            ValidateCoords(x, y);
            return _pixels[x, y];
        }
        set
        {
            ValidateCoords(x, y);
            _pixels[x, y] = value;
        }
    }

    public void ValidateCoords(int x, int y)
    {
        if (!(x >= 0 && y >= 0 && x < Dimensions.Width && y < Dimensions.Height))
            throw new ArgumentOutOfRangeException($"Received X:{x}, Y:{y} for matrix of Height:{Dimensions.Width}, Width:{Dimensions.Height}");
    }
}