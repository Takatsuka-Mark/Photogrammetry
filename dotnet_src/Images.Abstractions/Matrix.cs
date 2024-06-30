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

    // TODO maybe some of this should go into a Images project.
    
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
        // TODO maybe just rename these all to dim 0 and dim 1
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

    public bool CoordsAreValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Dimensions.Width && y < Dimensions.Height;
    }
    
    public void ValidateCoords(int x, int y)
    {
        if (!CoordsAreValid(x, y))
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

    public TData[] Row(int y)
    {
        // TODO maybe smartly combine this into [int x, int y] some how?
        return Enumerable.Range(0, Dimensions.Width).Select(x => this[x, y]).ToArray();
    }

    public TData[] GetColumn(int x)
    {
        return Enumerable.Range(0, Dimensions.Height).Select(y => this[x, y]).ToArray();
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

    public void DrawSquare(int x, int y, int radius)
    {
        ValidateCoords(x, y);

        for (var u = x - radius; u < x + radius; u += 1)
        {
            for (var v = y - radius; v < y + radius; v += 1)
            {
                if (!CoordsAreValid(u, v))
                    continue;
                
            }
        }
    }
}