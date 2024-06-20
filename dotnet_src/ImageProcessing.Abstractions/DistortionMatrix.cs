using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Abstractions;

public class DistortionMatrix : Matrix<Uv>
{
    public DistortionMatrix(MatrixDimensions dimensions) : base(dimensions)
    {
    }
}