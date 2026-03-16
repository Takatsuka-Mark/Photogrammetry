using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageProcessing.PipelinesV3.DTOs;
using Microsoft.Extensions.Logging;
using PhotogrammetryStore;

namespace ImageProcessing.PipelinesV3.Factories;

public class
    RedundantKeypointEliminatorTransformStepFactory : ITransformStepFactory<MetadataStoreRecord, MetadataStoreRecord>
{
    private readonly MetadataStore _metadataStore;
    private readonly RedundantKeypointEliminator _redundantKeypointEliminator;
    private readonly ILogger<RedundantKeypointEliminatorTransformStepFactory>? _logger;

    public RedundantKeypointEliminatorTransformStepFactory(MetadataStore metadataStore,
        RedundantKeypointEliminator redundantKeypointEliminator,
        ILogger<RedundantKeypointEliminatorTransformStepFactory>? logger = null)
    {
        _metadataStore = metadataStore;
        _redundantKeypointEliminator = redundantKeypointEliminator;
        _logger = logger;
    }

    public void Initialize()
    {
    }

    public TransformBlock<MetadataStoreRecord, MetadataStoreRecord> GetTransformBlock()
    {
        return new TransformBlock<MetadataStoreRecord, MetadataStoreRecord>(record =>
        {
            _logger?.LogInformation("Eliminating Keypoints");
            var originalKeypoints = _metadataStore.FetchKeypoints(record.RecordGuid);
            var reducedKeypoints = _redundantKeypointEliminator.EliminateRedundantKeypoints(originalKeypoints);
            _metadataStore.StoreDenoisedKeypoints(record.RecordGuid, reducedKeypoints); // TODO move to own function?
            _logger?.LogInformation("Eliminated Redundant Keypoints to {Remain}/{Total}", reducedKeypoints.Count,
                originalKeypoints.Count);
            return record;
        });
    }

    public TransformBlock<MetadataStoreRecord, MetadataStoreRecord> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}