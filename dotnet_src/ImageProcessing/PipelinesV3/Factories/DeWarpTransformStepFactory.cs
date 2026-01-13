using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelinesV3.Factories;

public class DeWarpTransformStepFactory : ITransformStepFactory<Matrix<Rgba64>, Matrix<Rgba64>>
{
    private readonly Lazy<Matrix<Uv>> _distortionMatrix;

    public DeWarpTransformStepFactory(DeWarp deWarp)
    {
        // TODO should possibly have some way to change configuration live.

        _distortionMatrix = new Lazy<Matrix<Uv>>(deWarp.GetDistortionMatrix);
    }

    public void Initialize()
    {
        if (!_distortionMatrix.IsValueCreated)
        {
            _ = _distortionMatrix.Value;
        }
        else
        {
            // TODO do logger.
            Console.WriteLine("WARN: It appears the DeWarp service has been doubly initialed");
        }
    }

    public TransformBlock<Matrix<Rgba64>, Matrix<Rgba64>> GetTransformBlock()
    {
        // TODO OOPness of this is kinda weird.
        return new TransformBlock<Matrix<Rgba64>, Matrix<Rgba64>>(inputMatrix =>
            DeWarp.ApplyDistortionMat(inputMatrix, _distortionMatrix.Value));
    }

    public TransformBlock<Matrix<Rgba64>, Matrix<Rgba64>> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}