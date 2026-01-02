namespace LinearAlgebra;

public class Matrix<TDataType> : IMatrix<TDataType>
{
    public readonly MatrixDimensions Dimensions;
    private readonly TDataType[,] _data;

    public Matrix(uint rows, uint cols, TDataType emptyFill) : this(new MatrixDimensions { Cols = cols, Rows = rows },
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
        _data = new TDataType[dimensions.Rows, dimensions.Cols];
    }

    # region Accessors

    public TDataType this[uint rowIdx, uint colIdx]
    {
        get => Get(rowIdx, colIdx);
        set => Set(rowIdx, colIdx, value);
    }

    public void Set(uint rowIdx, uint colIdx, TDataType data)
    {
        AssertInBounds(rowIdx, colIdx);
        _data[rowIdx, colIdx] = data;
    }

    public TDataType Get(uint rowIdx, uint colIdx)
    {
        AssertInBounds(rowIdx, colIdx);
        return _data[rowIdx, colIdx];
    }

    public TDataType[] GetRow(uint rowIdx)
    {
        AssertRowInBounds(rowIdx);
        return Enumerable.Range(0, (int)Dimensions.Cols)
            .Select(col => _data[rowIdx, col])
            .ToArray();
    }

    public TDataType[] GetColumn(uint columnIdx)
    {
        AssertColInBounds(columnIdx);
        return Enumerable.Range(0, (int)Dimensions.Rows)
            .Select(row => _data[row, columnIdx])
            .ToArray();
    }

    public IEnumerable<TDataType> IterateAll()
    {
        for (var row = 0; row < Dimensions.Rows; row += 1)
        {
            for (var col = 0; col < Dimensions.Cols; col += 1)
            {
                yield return _data[row, col];
            }
        }
    }

    public void MapAll(Func<(uint row, uint col), TDataType, TDataType> mappingFunction)
    {
        for (var row = 0; row < Dimensions.Rows; row += 1)
        {
            for (var col = 0; col < Dimensions.Cols; col += 1)
            {
                _data[row, col] = mappingFunction(((uint)row, (uint)col), _data[row, col]);
            }
        }
    }

    # endregion

    # region Helpers

    public void Fill(TDataType fillData) => MapAll((_, _) => fillData);

    public IMatrix<TNewDataType> Convert<TNewDataType>(
        Func<(uint row, uint col), TDataType, TNewDataType> mappingFunction)
    {
        // TODO think about how this should be done. Should each be set or can I have a ctor that takes a 2d array as input?
        var newMatrix = new Matrix<TNewDataType>(Dimensions);

        for (var row = 0; row < Dimensions.Rows; row += 1)
        {
            for (var col = 0; col < Dimensions.Cols; col += 1)
            {
                newMatrix.Set((uint)row, (uint)col, mappingFunction(((uint)row, (uint)col), _data[row, col]));
            }
        }

        return newMatrix;
    }

    public IMatrix<TDataType> Transpose()
    {
        throw new NotImplementedException();
    }

    #endregion

    # region Validators

    public bool InBounds(uint rowIdx, uint colIdx) => RowInBounds(rowIdx) && ColInBounds(colIdx);

    public bool RowInBounds(uint rowIdx) => rowIdx < Dimensions.Rows;

    public bool ColInBounds(uint colIdx) => colIdx < Dimensions.Cols;

    private void AssertInBounds(uint rowIdx, uint colIdx)
    {
        if (!InBounds(rowIdx, colIdx))
            throw new IndexOutOfRangeException(
                $"({rowIdx} row, {colIdx} col) for matrix of size {Dimensions}");
    }

    private void AssertRowInBounds(uint rowIdx)
    {
        if (!RowInBounds(rowIdx))
            throw new IndexOutOfRangeException(
                $"({rowIdx} row) for matrix of size {Dimensions}");
    }

    private void AssertColInBounds(uint colIdx)
    {
        if (!ColInBounds(colIdx))
            throw new IndexOutOfRangeException(
                $"({colIdx} row) for matrix of size {Dimensions}");
    }

    #endregion
}