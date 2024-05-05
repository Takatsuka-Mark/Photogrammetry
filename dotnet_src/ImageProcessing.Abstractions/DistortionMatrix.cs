using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Abstractions;

public class DistortionMatrix : Image<Uv>
{
    public DistortionMatrix(ImageDimensions dimensions) : base(dimensions)
    {
    }
}