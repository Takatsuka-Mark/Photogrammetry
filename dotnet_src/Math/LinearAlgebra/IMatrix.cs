namespace LinearAlgebra;

public interface IMatrix<TDataType>
{
    # region Accessors

    public void Set(uint rowIdx, uint colIdx, TDataType data);
    public TDataType Get(uint rowIdx, uint colIdx);

    // TODO change list return types?
    public TDataType[] GetRow(uint rowIdx);
    public TDataType[] GetColumn(uint columnIdx);
    public IEnumerable<TDataType> IterateAll();
    public void MapAll(Func<(uint row, uint col), TDataType, TDataType> mappingFunction);

    #endregion

    # region Helpers

    // TODO could make these extensions like .fill
    public void Fill(TDataType fillData);

    public IMatrix<TNewDataType> Convert<TNewDataType>(
        Func<(uint row, uint col), TDataType, TNewDataType> mappingFunction);

    #endregion

    # region Validators

    public bool InBounds(uint rowIdx, uint colIdx);
    public bool RowInBounds(uint rowIdx);
    public bool ColInBounds(uint colIdx);

    #endregion
}