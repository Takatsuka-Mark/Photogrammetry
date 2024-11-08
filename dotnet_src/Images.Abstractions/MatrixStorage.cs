namespace Images.Abstractions;

public class MatrixStorage<TData>
{
    public readonly MatrixDimensions Dimensions;

    private readonly TData[] _data;
    
    public MatrixStorage(MatrixDimensions dimensions)
    {
        // Note, is row major
        Dimensions = dimensions;
        _data = new TData[(long)dimensions.Width * dimensions.Height];
    }

    private MatrixStorage(MatrixDimensions dimensions, TData[] data)
    {
        Dimensions = dimensions;
        _data = data;
    }

    public static MatrixStorage<TData> FromColMajorArray(TData[,] colMajorArray)
    {
        var numCols = colMajorArray.GetLength(0);
        var numRows = colMajorArray.GetLength(1);

        var data = new TData[(long)numRows * numCols];
        var dimensions = new MatrixDimensions(numCols, numRows);

        for (var row = 0; row < numRows; row++)
        {
            var offset = row * dimensions.Width;
            for (var col = 0; col < numCols; col++)
            {
                data[offset + col] = colMajorArray[col, row];
            }
        }

        return new MatrixStorage<TData>(dimensions, data);
    }
    
    public static MatrixStorage<TData> FromRowMajorArray(TData[,] rowMajorArray)
    {
        var numRows = rowMajorArray.GetLength(0);
        var numCols = rowMajorArray.GetLength(1);

        var data = new TData[(long)numRows * numCols];
        var dimensions = new MatrixDimensions(numCols, numRows);

        for (var row = 0; row < numRows; row++)
        {
            var offset = row * dimensions.Width;
            for (var col = 0; col < numCols; col++)
            {
                data[offset + col] = rowMajorArray[row, col];
            }
        }

        return new MatrixStorage<TData>(dimensions, data);
    }
    
    public TData this[int x, int y]
    {
        get
        {
            AssertCoordsAreValid(x, y);
            return GetWithoutValidation(x, y);
        }
        set
        {
            AssertCoordsAreValid(x, y);
            SetWithoutValidation(x, y, value);
        }
    }

    public TData? GetOrNull(int x, int y)
    {
        // TODO note that if this is reference type, the value will be the default value.
        return !CoordsAreValid(x, y) ? default : GetWithoutValidation(x, y);
    }

    public void SetOrDoNothing(int x, int y, TData data)
    {
        if (CoordsAreValid(x, y))
            SetWithoutValidation(x, y, data);
    }

    private TData GetWithoutValidation(int x, int y)
    {
        return _data[(long)y * Dimensions.Width + x];
    }

    private void SetWithoutValidation(int x, int y, TData datum)
    {
        _data[(long)y * Dimensions.Width + x] = datum;
    }

    public bool CoordsAreValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Dimensions.Width && y < Dimensions.Height;
    }

    public void AssertCoordsAreValid(int x, int y)
    {
        if (!CoordsAreValid(x, y))
            throw new ArgumentOutOfRangeException(
                $"Received X:{x}, Y:{y} for matrix of Width:{Dimensions.Width}, Height:{Dimensions.Height}");
    }

    public TData[] Row(int y)
    {
        return Enumerable.Range(0, Dimensions.Width).Select(x => GetWithoutValidation(x, y)).ToArray();
    }

    public TData[] Column(int x)
    {
        return Enumerable.Range(0, Dimensions.Height).Select(y => GetWithoutValidation(x, y)).ToArray();
    }
}