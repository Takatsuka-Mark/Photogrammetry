namespace LinearAlgebra;

public class Matrix<TDataType> : IMatrix<TDataType>
{
    private readonly MatrixDimensions _matrixDimensions;
    private readonly TDataType[,] _data;

    public Matrix(uint rows, uint cols, TDataType emptyFill) : this(new MatrixDimensions { Cols = cols, Rows = rows },
        emptyFill)
    {
    }

    public Matrix(MatrixDimensions matrixDimensions, TDataType emptyFill)
    {
        _matrixDimensions = matrixDimensions;
        _data = new TDataType[matrixDimensions.Rows, matrixDimensions.Cols];

        Fill(emptyFill);
    }

    # region Accessors

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
        return Enumerable.Range(0, (int)_matrixDimensions.Cols)
            .Select(col => _data[rowIdx, col])
            .ToArray();
    }

    public TDataType[] GetColumn(uint columnIdx)
    {
        AssertColInBounds(columnIdx);
        return Enumerable.Range(0, (int)_matrixDimensions.Rows)
            .Select(row => _data[row, columnIdx])
            .ToArray();
    }

    public IEnumerable<TDataType> IterateAll()
    {
        for (var row = 0; row < _matrixDimensions.Rows; row += 1)
        {
            for (var col = 0; col < _matrixDimensions.Cols; col += 1)
            {
                yield return _data[row, col];
            }
        }
    }

    public void MapAll(Func<(int row, int col), TDataType, TDataType> mappingFunction)
    {
        for (var row = 0; row < _matrixDimensions.Rows; row += 1)
        {
            for (var col = 0; col < _matrixDimensions.Cols; col += 1)
            {
                _data[row, col] = mappingFunction((row, col), _data[row, col]);
            }
        }
    }

    # endregion

    # region Helpers

    public void Fill(TDataType fillData) => MapAll((_, _) => fillData);

    public IMatrix<TNewDataType> Convert<TNewDataType>(
        Func<(int row, int col), TDataType, TNewDataType> mappingFunction)
    {
        // TODO think about how this should be done. Should each be set or can I have a ctor that takes a 2d array as input?
        throw new NotImplementedException();
    }

    #endregion

    # region Validators

    public bool InBounds(uint rowIdx, uint colIdx) => RowInBounds(rowIdx) && ColInBounds(colIdx);

    public bool RowInBounds(uint rowIdx) => rowIdx < _matrixDimensions.Rows;

    public bool ColInBounds(uint colIdx) => colIdx < _matrixDimensions.Cols;

    private void AssertInBounds(uint rowIdx, uint colIdx)
    {
        if (!InBounds(rowIdx, colIdx))
            throw new IndexOutOfRangeException(
                $"({rowIdx} row, {colIdx} col) for matrix of size {_matrixDimensions}");
    }

    private void AssertRowInBounds(uint rowIdx)
    {
        if (!RowInBounds(rowIdx))
            throw new IndexOutOfRangeException(
                $"({rowIdx} row) for matrix of size {_matrixDimensions}");
    }

    private void AssertColInBounds(uint colIdx)
    {
        if (!ColInBounds(colIdx))
            throw new IndexOutOfRangeException(
                $"({colIdx} row) for matrix of size {_matrixDimensions}");
    }

    #endregion
}