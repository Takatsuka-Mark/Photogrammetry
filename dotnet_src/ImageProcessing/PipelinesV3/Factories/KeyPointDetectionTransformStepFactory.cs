using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageProcessing.PipelinesV3.DTOs;
using Microsoft.Extensions.Logging;
using PhotogrammetryStore;

namespace ImageProcessing.PipelinesV3.Factories;

public class KeyPointDetectionTransformStepFactory : ITransformStepFactory<MetadataStoreRecord, MetadataStoreRecord>
{
    private readonly MetadataStore _metadataStore;
    private readonly KeypointDetection _keypointDetection;
    private readonly ILogger<KeyPointDetectionTransformStepFactory>? _logger;

    public KeyPointDetectionTransformStepFactory(MetadataStore metadataStore, KeypointDetection keypointDetection, ILogger<KeyPointDetectionTransformStepFactory>? logger = null)
    {
        _metadataStore = metadataStore;
        _keypointDetection = keypointDetection;
        _logger = logger;
    }
    
    public void Initialize()
    {
    }

    public TransformBlock<MetadataStoreRecord, MetadataStoreRecord> GetTransformBlock()
    {
        return new TransformBlock<MetadataStoreRecord, MetadataStoreRecord>(record =>
        {
            // TODO log category. also downgrade to debug
            // TODO variant selection. See DeWarp for full comment.
            _logger?.LogInformation("Detecting keypoints");
            var grayscaleImage = _metadataStore.FetchDeWarpedGrayscale(record.RecordGuid);
            var keypoints = _keypointDetection.Detect(grayscaleImage);
            _metadataStore.StoreKeypoints(record.RecordGuid, keypoints);
            _logger?.LogInformation("Found {N} keypoints", keypoints.Count);
            return record;
        });
    }

    public TransformBlock<MetadataStoreRecord, MetadataStoreRecord> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}