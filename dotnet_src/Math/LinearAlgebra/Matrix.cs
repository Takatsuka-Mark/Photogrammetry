namespace LinearAlgebra;

public class Matrix<TDataType> : IMatrix<TDataType>
{
    public readonly MatrixDimensions Dimensions;
    private readonly TDataType[,] _data;

    public Matrix(ushort ys, ushort xs, TDataType emptyFill) : this(new MatrixDimensions { Width = xs, Height = ys },
        emptyFill)
    {
    }

    public Matrix(MatrixDimensions dimensions, TDataType emptyFill) : this(dimensions)
    {
        Fill(emptyFill);
    }

    public Matrix(MatrixDimensions dimensions)
    {
        Dimensions = dimensions;
        _data = new TDataType[dimensions.Width, dimensions.Height];
    }

    public static Matrix<TDataType> FromRowMajorArray(TDataType[,] rowMajorArray)
    {
        var numRows = (ushort)rowMajorArray.GetLength(0);
        var numCols = (ushort)rowMajorArray.GetLength(1);

        var matrix = new Matrix<TDataType>(new MatrixDimensions { Width = numCols, Height = numRows });

        for (ushort row = 0; row < numRows; row += 1)
        {
            for (ushort col = 0; col < numCols; col += 1)
            {
                matrix[col, row] = rowMajorArray[row, col];
            }
        }

        return matrix;
    }

    # region Accessors

    public TDataType this[ushort x, ushort y]
    {
        // TODO rename row col?
        get => Get(x, y);
        set => Set(x, y, value);
    }

    public TDataType this[int x, int y] => Get(x, y);

    public TDataType this[Coordinate coordinate] => Get((ushort)coordinate.X, (ushort)coordinate.Y);

    public void Set(ushort x, ushort y, TDataType data)
    {
        AssertInBounds(x, y);
        _data[x, y] = data;
    }

    public TDataType Get(int x, int y)
    {
        if (x < 0 || y < 0 || x > ushort.MaxValue || y > ushort.MaxValue)
        {
            throw new IndexOutOfRangeException($"Attempting to fetch value that would be improperly cast: {x}, {y}");
        }

        AssertInBounds((ushort)x, (ushort)y);
        return _data[x, y];
    }

    public TDataType Get(ushort x, ushort y)
    {
        AssertInBounds(x, y);
        return _data[x, y];
    }

    public TDataType[] GetXs(ushort y)
    {
        AssertYInBounds(y);
        return Enumerable.Range(0, Dimensions.Width)
            .Select(x => _data[x, y])
            .ToArray();
    }

    public TDataType[] GetYs(ushort x)
    {
        AssertXInBounds(x);
        return Enumerable.Range(0, Dimensions.Height)
            .Select(y => _data[x, y])
            .ToArray();
    }

    public IEnumerable<TDataType> IterateAll()
    {
        for (ushort y = 0; y < Dimensions.Height; y += 1)
        {
            for (ushort x = 0; x < Dimensions.Width; x += 1)
            {
                yield return _data[x, y];
            }
        }
    }

    public void MapAll(Func<(ushort x, ushort y), TDataType, TDataType> mappingFunction)
    {
        for (ushort y = 0; y < Dimensions.Height; y += 1)
        {
            for (ushort x = 0; x < Dimensions.Width; x += 1)
            {
                _data[x, y] = mappingFunction((x, y), _data[x, y]);
            }
        }
    }

    # endregion

    # region Helpers

    public void Fill(TDataType fillData) => MapAll((_, _) => fillData);

    public IMatrix<TNewDataType> Convert<TNewDataType>(
        Func<(ushort x, ushort y), TDataType, TNewDataType> mappingFunction)
    {
        // TODO think about how this should be done. Should each be set or can I have a ctor that takes a 2d array as input?
        var newMatrix = new Matrix<TNewDataType>(Dimensions);

        for (ushort y = 0; y < Dimensions.Height; y += 1)
        {
            for (ushort x = 0; x < Dimensions.Width; x += 1)
            {
                newMatrix.Set(x, y, mappingFunction((x, y), _data[x, y]));
            }
        }

        return newMatrix;
    }

    public IMatrix<TDataType> Transpose()
    {
        // TODO need a better way to load data into new matrix.
        var newMatrix = new Matrix<TDataType>(new MatrixDimensions
            { Height = Dimensions.Width, Width = Dimensions.Height });

        for (ushort y = 0; y < Dimensions.Height; y += 1)
        {
            for (ushort x = 0; x < Dimensions.Width; x += 1)
            {
                newMatrix[y, x] = _data[x, y];
            }
        }

        return newMatrix;
    }

    #endregion

    # region Validators

    public bool InBounds(int xIdx, int yIdx) => xIdx >= 0 && yIdx >= 0 && xIdx <= ushort.MaxValue &&
                                                yIdx <= ushort.MaxValue && YInBounds((ushort)yIdx) &&
                                                XInBounds((ushort)xIdx);

    public bool InBounds(ushort xIdx, ushort yIdx) => YInBounds(yIdx) && XInBounds(xIdx);

    public bool YInBounds(ushort yIdx) => yIdx < Dimensions.Height;

    public bool XInBounds(ushort xIdx) => xIdx < Dimensions.Width;

    private void AssertInBounds(ushort xIdx, ushort yIdx)
    {
        if (!InBounds(xIdx, yIdx))
            throw new IndexOutOfRangeException(
                $"({xIdx} x, {yIdx} y) for matrix of size {Dimensions}");
    }

    private void AssertYInBounds(ushort yIdx)
    {
        if (!YInBounds(yIdx))
            throw new IndexOutOfRangeException(
                $"({yIdx} y) for matrix of size {Dimensions}");
    }

    private void AssertXInBounds(ushort xIdx)
    {
        if (!XInBounds(xIdx))
            throw new IndexOutOfRangeException(
                $"({xIdx} x) for matrix of size {Dimensions}");
    }

    private (ushort xIdx, ushort yIdx) CastXAndY(int xIdx, int yIdx)
    {
        if (yIdx < 0 || yIdx > short.MaxValue || xIdx < 0 || xIdx > short.MaxValue)
        {
            throw new IndexOutOfRangeException($"Cannot cast y and x from int to ushort ({yIdx}, {xIdx})");
        }

        return ((ushort)yIdx, (ushort)xIdx);
    }

    #endregion
}