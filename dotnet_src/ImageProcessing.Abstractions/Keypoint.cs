using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Abstractions;

public class Keypoint
{
    private readonly int _imageId;
    public readonly int X;
    public readonly int Y;
    private int? _briefDescriptor;

    public Keypoint(int imageId, int x, int y)
    {
        _imageId = imageId;
        X = x;
        Y = y;
        // TODO gaussian pairs?
    }

    public int BriefDescriptor(Matrix<Grayscale> image)
    {
        if (_briefDescriptor.HasValue)
            return _briefDescriptor.Value;
        throw new NotImplementedException();
    }
}