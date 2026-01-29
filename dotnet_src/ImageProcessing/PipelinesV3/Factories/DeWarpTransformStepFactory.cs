using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using Microsoft.Extensions.Logging;

namespace ImageProcessing.PipelinesV3.Factories;

public class DeWarpTransformStepFactory : ITransformStepFactory<Matrix<Rgba64>, Matrix<Rgba64>>
{
    private readonly ILogger<DeWarpTransformStepFactory>? _logger;
    private readonly Lazy<Matrix<Uv>> _distortionMatrix;

    public DeWarpTransformStepFactory(DeWarp deWarp, ILogger<DeWarpTransformStepFactory>? logger = null)
    {
        _logger = logger;
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
        {
            // TODO log category. also downgrade to debug
            _logger?.LogInformation("Dewarping image");
            return DeWarp.ApplyDistortionMat(inputMatrix, _distortionMatrix.Value);
        });
    }

    public TransformBlock<Matrix<Rgba64>, Matrix<Rgba64>> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}