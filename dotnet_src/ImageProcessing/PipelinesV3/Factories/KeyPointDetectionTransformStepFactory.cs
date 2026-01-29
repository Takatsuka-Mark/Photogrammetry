using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageProcessing.PipelinesV3.DTOs;
using Microsoft.Extensions.Logging;

namespace ImageProcessing.PipelinesV3.Factories;

public class KeyPointDetectionTransformStepFactory : ITransformStepFactory<GrayscaleImagePair, DetectedKeypoints>
{
    private readonly KeypointDetection _keypointDetection;
    private readonly ILogger<KeyPointDetectionTransformStepFactory>? _logger;

    public KeyPointDetectionTransformStepFactory(KeypointDetection keypointDetection, ILogger<KeyPointDetectionTransformStepFactory>? logger = null)
    {
        _keypointDetection = keypointDetection;
        _logger = logger;
    }
    
    public void Initialize()
    {
    }

    public TransformBlock<GrayscaleImagePair, DetectedKeypoints> GetTransformBlock()
    {
        return new TransformBlock<GrayscaleImagePair, DetectedKeypoints>(imagePair =>
        {
            // TODO log category. also downgrade to debug
            _logger?.LogInformation("Detecting keypoints");
            return new DetectedKeypoints(imagePair, _keypointDetection.Detect(imagePair.GrayscaleImage));
        });
    }

    public TransformBlock<GrayscaleImagePair, DetectedKeypoints> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}