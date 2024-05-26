namespace Images.Abstractions;

public class Matrix<TData>
{
    // TODO maybe eventually allow for creation of N dimensional matrix?
    public MatrixDimensions Dimensions { get; }

    // TODO possibly a better way to store this under the hoold...
    private readonly TData[,] _pixels;

    public Matrix(MatrixDimensions dimensions)
    {
        Dimensions = dimensions;
        _pixels = new TData[dimensions.Width, dimensions.Height];
    }

    public static Matrix<TData> FromArray(TData[,] data)
    {
        var numRows = data.GetLength(0);
        var numCols = data.GetLength(1);

        var resultMatrix = new Matrix<TData>(new MatrixDimensions(numCols, numRows));

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                resultMatrix[col, row] = data[row, col];
            }
        }
        
        return resultMatrix;
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
            throw new ArgumentOutOfRangeException($"Received X:{x}, Y:{y} for matrix of Width:{Dimensions.Width}, Height:{Dimensions.Height}");
    }

    public Matrix<TData> Transpose()
    {
        var transposedMatrix = new Matrix<TData>(new MatrixDimensions(Dimensions.Height, Dimensions.Width));

        for (int y = 0; y < Dimensions.Height; y++)
        {
            for (int x = 0; x < Dimensions.Width; x++)
            {
                transposedMatrix[y, x] = this[x, y];
            }
        }

        return transposedMatrix;
    }

    public void Draw()
    {
        Console.WriteLine("[");
        for (int y = 0; y < Dimensions.Height; y++)
        {
            var items = new List<TData>();
            for (int x = 0; x < Dimensions.Width; x++)
            {
                items.Add(this[x, y]);
            }

            Console.WriteLine($"[{String.Join(", ", items)}],");
        }
        Console.WriteLine("]");
    }


    public delegate TOutData PixelConverter<out TOutData>(TData dataIn); 
    public Matrix<TNewData> Convert<TNewData>(PixelConverter<TNewData> pixelConverter)
    {
        // TODO should create a generic map function that does this double for loop generation.
        var outputMatrix = new Matrix<TNewData>(Dimensions);

        for (int x = 0; x < Dimensions.Width; x++)
        {
            for (int y = 0; y < Dimensions.Height; y++)
            {
                outputMatrix[x, y] = pixelConverter(_pixels[x, y]);
            }
        }
        
        return outputMatrix;
    }
}