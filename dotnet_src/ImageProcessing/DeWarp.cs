using ImageProcessing.Abstractions;
using Images.Abstractions;

namespace ImageProcessing;

public class DeWarp
{
    private readonly ImageDimensions _dimensions;

    public DeWarp(ImageDimensions dimensions)
    {
        _dimensions = dimensions;
    }

    public DistortionMatrix GetDistortionMatrix(int[] distortionCoefficients )
    {
        // TODO we may just want to make this static
        // TODO implement caching for the matrix.
        if (distortionCoefficients.Length != 5)
             // TODO is there a way we can force this through params, without requiring 5 params? Perhaps it's own class...
            throw new ArgumentException("You must pass exactly 5 distortion coefficients");

        var distortionMatrix = new DistortionMatrix(_dimensions);

        
        
        return distortionMatrix;
    }
}