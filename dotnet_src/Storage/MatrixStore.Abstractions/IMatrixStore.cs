using LinearAlgebra;

namespace MatrixStore.Abstractions;

public interface IMatrixStore<T>
{
	public Guid StoreMatrix(Matrix<T> matrix, DateTimeOffset? createdDate = null, int? sequenceId = null);
	public MatrixRecord<T> FetchMatrix(Guid matrixGuid);
	public void RemoveMatrix(Guid matrixGuid);
}
