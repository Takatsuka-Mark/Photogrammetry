namespace LinearAlgebra;

public interface IMatrix<TDataType>
{
    # region Accessors
    public TDataType this[ushort xIds, ushort yIdx] { get; set; }
    public void Set(ushort xIdx, ushort yIdx, TDataType data);
    public TDataType Get(ushort xIdx, ushort yIdx);

    // TODO change list return types?
    public TDataType[] GetYs(ushort yIdx);
    public TDataType[] GetXs(ushort xIdx);
    public IEnumerable<TDataType> IterateAll();
    public void MapAll(Func<(ushort x, ushort y), TDataType, TDataType> mappingFunction);

    #endregion

    # region Helpers

    // TODO could make these extensions like .fill
    public void Fill(TDataType fillData);

    public IMatrix<TNewDataType> Convert<TNewDataType>(
        Func<(ushort x, ushort y), TDataType, TNewDataType> mappingFunction);

    public IMatrix<TDataType> Transpose();

    #endregion

    # region Validators

    public bool InBounds(ushort xIdx, ushort yIdx);
    public bool YInBounds(ushort yIdx);
    public bool XInBounds(ushort xIdx);

    #endregion
}