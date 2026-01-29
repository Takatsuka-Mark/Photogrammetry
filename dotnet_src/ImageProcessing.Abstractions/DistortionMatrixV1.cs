using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Abstractions;

public class DistortionMatrixV1 : MatrixV1<Uv>
{
    public DistortionMatrixV1(MatrixDimensions dimensions) : base(dimensions)
    {
    }
}