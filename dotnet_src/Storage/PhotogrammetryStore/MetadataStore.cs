using System.Collections.Concurrent;
using ImageProcessing.Abstractions;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using MatrixStore.Abstractions;
using MatrixStore.Memory;
using PhotogrammetryStore.Abstractions;

namespace PhotogrammetryStore;

public class MetadataStore
{
	private readonly TimeProvider _timeProvider;
	private readonly ConcurrentDictionary<Guid, ParentMetadata> _metadataMapping;
	private readonly ConcurrentDictionary<Guid, List<Keypoint>> _keypointMappings;
	private readonly IMatrixStore<Rgba64> _rgba64Storage;
	private readonly IMatrixStore<Grayscale> _grayscaleStorage;


	public MetadataStore(TimeProvider timeProvider)
	{
		_timeProvider = timeProvider;
		// TODO not really a metadata store, as it is a full store
		// TODO is there a better way we can build storage backends for each variant? Possibly DI?
		_rgba64Storage = new InMemoryMatrixStore<Rgba64>(timeProvider);
		_grayscaleStorage = new InMemoryMatrixStore<Grayscale>(timeProvider);

		// TODO this is really a 1D array. Could just convert to that.
		_keypointMappings = new ConcurrentDictionary<Guid, List<Keypoint>>();
		_metadataMapping = new ConcurrentDictionary<Guid, ParentMetadata>();
	}

	public Guid CreateRecord(DateTimeOffset? createdAt = null)
	{
		var guid = Guid.NewGuid();
		_metadataMapping.TryAdd(guid,
			new ParentMetadata(createdAt ?? _timeProvider.GetUtcNow(),
				new ConcurrentDictionary<MetadataVariant, Guid>()));
		return guid;
	}

	public void SaveRgba64(Guid record, Matrix<Rgba64> matrix) =>
		StoreAndUpdateMetadata(record, MetadataVariant.Rgba64, () => _rgba64Storage.StoreMatrix(matrix));

	public Matrix<Rgba64> FetchRgba64(Guid parent)
	{
		var guid = GetGuidIfExists(parent, MetadataVariant.Rgba64);
		return _rgba64Storage.FetchMatrix(guid).Matrix;
	}

	public void SaveGrayscale(Guid parent, Matrix<Grayscale> matrix) => StoreAndUpdateMetadata(parent,
		MetadataVariant.Greyscale, () => _grayscaleStorage.StoreMatrix(matrix));

	public Matrix<Grayscale> FetchGrayscale(Guid parent)
	{
		var guid = GetGuidIfExists(parent, MetadataVariant.Rgba64);
		return _grayscaleStorage.FetchMatrix(guid).Matrix;
	}

	public void SaveKeypoints(Guid parent, List<Keypoint> keypoints) => StoreAndUpdateMetadata(parent,
		MetadataVariant.Keypoints, () =>
		{
			var guid = Guid.NewGuid();
			_keypointMappings.TryAdd(guid, keypoints);
			return guid;
		});

	public List<Keypoint> FetchKeypoint(Guid parent)
	{
		var guid = GetGuidIfExists(parent, MetadataVariant.Keypoints);

		if (!_keypointMappings.TryGetValue(guid, out var keypoints))
		{
			throw new Exception($"Failed to fetch keypoints for record {parent}, {guid}");
		}

		return keypoints;
	}

	private void StoreAndUpdateMetadata(Guid record, MetadataVariant variant, Func<Guid> storeFunc)
	{
		if (!_metadataMapping.TryGetValue(record, out var parentMetadata))
		{
			throw new Exception($"Record does not exist {record}");
		}

		if (parentMetadata.Children.ContainsKey(variant))
		{
			throw new Exception($"Record {record} already has a {variant} variant");
		}

		var guid = storeFunc();
		parentMetadata.Children.TryAdd(variant, guid);
		_metadataMapping[record] = parentMetadata;
	}

	private Guid GetGuidIfExists(Guid record, MetadataVariant variant)
	{
		if (!_metadataMapping.TryGetValue(record, out var mappings))
		{
			throw new Exception($"Record does not exist: {record}");
		}

		if (!mappings.Children.TryGetValue(variant, out var variantGuid))
		{
			throw new Exception($"Variant {variant} for record {record} does not exist.");
		}

		return variantGuid;
	}
}
