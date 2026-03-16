using System.Collections.Concurrent;
using LinearAlgebra;
using MatrixStore.Abstractions;

namespace MatrixStore.Memory;

public class InMemoryMatrixStore<T> : IMatrixStore<T>
{
	private readonly TimeProvider _timeProvider;
	private readonly ConcurrentDictionary<Guid, MatrixRecord<T>> _storage;

	public InMemoryMatrixStore(TimeProvider timeProvider)
	{
		_timeProvider = timeProvider;
		// TODO configuration around retention (?)
		_storage = new ConcurrentDictionary<Guid, MatrixRecord<T>>();
	}

	public Guid StoreMatrix(Matrix<T> matrix, DateTimeOffset? createdDate = null, int? sequenceId = null)
	{
		// TODO add backpressure.
		var matrixGuid = Guid.NewGuid();
		return !_storage.TryAdd(matrixGuid, new MatrixRecord<T>(matrix, createdDate ?? _timeProvider.GetUtcNow(), sequenceId))
			? throw new Exception("Failed to add to matrix storage")
			: matrixGuid;
	}

	public MatrixRecord<T> FetchMatrix(Guid matrixGuid)
	{
		return !_storage.TryGetValue(matrixGuid, out var matrixRecord) ? throw
			new Exception($"Failed to fetch matrix with GUID {matrixGuid}") :
			matrixRecord;
	}

	public void RemoveMatrix(Guid matrixGuid)
	{
		throw new NotImplementedException();
	}
}
