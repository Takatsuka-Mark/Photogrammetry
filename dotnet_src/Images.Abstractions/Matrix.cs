namespace Images.Abstractions;

public class Matrix<TData> where TData : class
{
    // TODO maybe eventually allow for creation of N dimensional matrix?
    public MatrixDimensions Dimensions { get; }

    private MatrixStorage<TData> storage;

    public Matrix(MatrixDimensions dimensions)
    {
        Dimensions = dimensions;
        storage = new MatrixStorage<TData>(dimensions);
    }

    internal Matrix(MatrixStorage<TData> storage)
    {
        Dimensions = storage.Dimensions;
        this.storage = storage;
    }

    // TODO maybe some of this should go into a Images project.
    
    public static Matrix<TData> FromRowMajorArray(TData[,] rowMajorArray)
    {
        return new Matrix<TData>(MatrixStorage<TData>.FromColMajorArray(rowMajorArray));
    }

    public TData this[int x, int y]
    {
        get => storage[x, y];
        set => storage[x, y] = value;
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
        // TODO should probably build a way to do this without checks.
        var transposedMatrix = new Matrix<TData>(new MatrixDimensions(Dimensions.Height, Dimensions.Width));

        for (var y = 0; y < Dimensions.Height; y++)
        {
            for (var x = 0; x < Dimensions.Width; x++)
            {
                transposedMatrix[y, x] = this[x, y];
            }
        }

        return transposedMatrix;
    }

    public TData[] Row(int y)
    {
        return storage.Row(y);
    }

    public TData[] GetColumn(int x)
    {
        return storage.Column(x);
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