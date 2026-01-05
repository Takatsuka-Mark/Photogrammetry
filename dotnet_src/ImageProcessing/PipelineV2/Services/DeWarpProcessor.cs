using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelineV2.Services;

public class DeWarpProcessor : IProcessor<Matrix<Rgba64>, Matrix<Rgba64>>
{
    private readonly DeWarp _deWarp;
    private readonly Lazy<Matrix<Uv>> _distortionMatrix;

    public DeWarpProcessor(DeWarp deWarp)
    {
        // TODO unsure if passing this makes full sense.
        _deWarp = deWarp;

        _distortionMatrix = new Lazy<Matrix<Uv>>(deWarp.GetDistortionMatrix);
    }
    
    // TODO this can be made generic?
    public void Initialize()
    {
        if (!_distortionMatrix.IsValueCreated)
        {
            _ = _distortionMatrix.Value;
        }

        // TODO do logger.
        Console.WriteLine("WARN: It appears the DeWarp service has been doubly initialed");
    }

    public Matrix<Rgba64> Process(Matrix<Rgba64> matrix)
    {
        return DeWarp.ApplyDistortionMat(matrix, _distortionMatrix.Value);
    }
}