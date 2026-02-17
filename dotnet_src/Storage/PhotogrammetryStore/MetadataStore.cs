using System.Collections.Concurrent;
using ImageProcessing.Abstractions;
using Images.Abstractions.Pixels;
using MatrixStore.Abstractions;
using MatrixStore.Memory;
using PhotogrammetryStore.Abstractions;

namespace PhotogrammetryStore;

public class MetadataStore
{
	private ConcurrentDictionary<Guid, ParentMetadata> _parentMetadataMapping;
	private ConcurrentDictionary<Guid, List<Keypoint>> _keypointMappings;
	private IMatrixStore<Rgba64> _rgba64Storage;
	private IMatrixStore<Grayscale> _grayscaleStorage;


	public MetadataStore(TimeProvider timeProvider)
	{
		// TODO not really a metadata store, as it is a full store
		// TODO base class this instead?
		// TODO is there a better way we can build storage backends for each variant? Possibly DI?
		// TODO should we handle parents, children? Or just expect caller to track pairings.
		//		I think long term this should handle it, but for now the caller can just handle it.
		_rgba64Storage = new InMemoryMatrixStore<Rgba64>(timeProvider);
		_grayscaleStorage = new InMemoryMatrixStore<Grayscale>(timeProvider);

		// TODO this is really a 1D array. Could just convert to that.
		_keypointMappings = new ConcurrentDictionary<Guid, List<Keypoint>>();
		_parentMetadataMapping = new ConcurrentDictionary<Guid, ParentMetadata>();
	}
	
	public 
}
