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

    # region Accessors

    public TDataType this[ushort x, ushort y]
    {
        // TODO rename row col?
        get => Get(x, y);
        set => Set(x ,y, value);
    }

    public void Set(ushort x, ushort y, TDataType data)
    {
        AssertInBounds(x, y);
        _data[x, y] = data;
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
            .Select(x => _data[y, x])
            .ToArray();
    }

    public TDataType[] GetYs(ushort x)
    {
        AssertXInBounds(x);
        return Enumerable.Range(0, Dimensions.Height)
            .Select(y => _data[y, x])
            .ToArray();
    }

    public IEnumerable<TDataType> IterateAll()
    {
        for (ushort y = 0; y < Dimensions.Height; y += 1)
        {
            for (ushort x = 0; x < Dimensions.Width; x += 1)
            {
                yield return _data[y, x];
            }
        }
    }

    public void MapAll(Func<(ushort x, ushort y), TDataType, TDataType> mappingFunction)
    {
        for (ushort y = 0; y < Dimensions.Height; y += 1)
        {
            for (ushort x = 0; x < Dimensions.Width; x += 1)
            {
                _data[y, x] = mappingFunction((y, x), _data[y, x]);
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
                newMatrix.Set(x, y, mappingFunction((x, y), _data[y, x]));
            }
        }

        return newMatrix;
    }

    public IMatrix<TDataType> Transpose()
    {
        // TODO need a better way to load data into new matrix.
        var newMatrix = new Matrix<TDataType>(new MatrixDimensions { Height = Dimensions.Width, Width = Dimensions.Height });
        
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