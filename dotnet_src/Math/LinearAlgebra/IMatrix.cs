namespace LinearAlgebra;

public interface IMatrix<TDataType>
{
    # region Accessors
    public TDataType this[ushort rowIds, ushort colIdx] { get; set; }
    public void Set(ushort rowIdx, ushort colIdx, TDataType data);
    public TDataType Get(ushort rowIdx, ushort colIdx);

    // TODO change list return types?
    public TDataType[] GetRow(ushort rowIdx);
    public TDataType[] GetColumn(ushort columnIdx);
    public IEnumerable<TDataType> IterateAll();
    public void MapAll(Func<(ushort row, ushort col), TDataType, TDataType> mappingFunction);

    #endregion

    # region Helpers

    // TODO could make these extensions like .fill
    public void Fill(TDataType fillData);

    public IMatrix<TNewDataType> Convert<TNewDataType>(
        Func<(ushort row, ushort col), TDataType, TNewDataType> mappingFunction);

    public IMatrix<TDataType> Transpose();

    #endregion

    # region Validators

    public bool InBounds(ushort rowIdx, ushort colIdx);
    public bool RowInBounds(ushort rowIdx);
    public bool ColInBounds(ushort colIdx);

    #endregion
}