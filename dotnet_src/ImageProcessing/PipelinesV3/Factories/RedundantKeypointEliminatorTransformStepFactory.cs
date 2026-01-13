using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageProcessing.PipelinesV3.DTOs;

namespace ImageProcessing.PipelinesV3.Factories;

public class
    RedundantKeypointEliminatorTransformStepFactory : ITransformStepFactory<DetectedKeypoints, DetectedKeypoints>
{
    private readonly RedundantKeypointEliminator _redundantKeypointEliminator;

    public RedundantKeypointEliminatorTransformStepFactory(RedundantKeypointEliminator redundantKeypointEliminator)
    {
        _redundantKeypointEliminator = redundantKeypointEliminator;
    }

    public void Initialize()
    {
    }

    public TransformBlock<DetectedKeypoints, DetectedKeypoints> GetTransformBlock()
    {
        return new TransformBlock<DetectedKeypoints, DetectedKeypoints>(keypoints => keypoints with
        {
            Keypoints = _redundantKeypointEliminator.EliminateRedundantKeypoints(keypoints.Keypoints)
        });
    }

    public TransformBlock<DetectedKeypoints, DetectedKeypoints> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}