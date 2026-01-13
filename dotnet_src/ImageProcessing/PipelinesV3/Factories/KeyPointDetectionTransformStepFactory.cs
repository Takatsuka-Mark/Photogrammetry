using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageProcessing.PipelinesV3.DTOs;

namespace ImageProcessing.PipelinesV3.Factories;

public class KeyPointDetectionTransformStepFactory : ITransformStepFactory<GrayscaleImagePair, DetectedKeypoints>
{
    private readonly KeypointDetection _keypointDetection;

    public KeyPointDetectionTransformStepFactory(KeypointDetection keypointDetection)
    {
        _keypointDetection = keypointDetection;
    }
    
    public void Initialize()
    {
        throw new NotImplementedException();
    }

    public TransformBlock<GrayscaleImagePair, DetectedKeypoints> GetTransformBlock()
    {
        // TODO convert to same type of matrix.
        return new TransformBlock<GrayscaleImagePair, DetectedKeypoints>(imagePair =>
            new DetectedKeypoints(imagePair, _keypointDetection.Detect(imagePair.GrayscaleImage)));
    }

    public TransformBlock<GrayscaleImagePair, DetectedKeypoints> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}