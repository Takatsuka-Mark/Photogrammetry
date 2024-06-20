using System.Numerics;
using Images.Abstractions;
using Images.Abstractions.Pixels;
using Microsoft.VisualBasic.CompilerServices;

namespace ImageProcessing.Abstractions;

public class Keypoint
{
    private readonly int _imageId;
    public readonly Coordinate Coordinate;
    public readonly BigInteger BriefDescriptor;

    public Keypoint(int imageId, int x, int y, List<(Coordinate, Coordinate)> guassianKeyPairs, Matrix<Grayscale> image)
    {
        _imageId = imageId;
        Coordinate = new Coordinate { X = x, Y = y };
        
        // TODO determine a less clunky way to create the brief descriptor. Like don't pass all this into the constructor
        BriefDescriptor = GetBriefDescriptor(Coordinate, guassianKeyPairs, image);
    }

    public static BigInteger GetBriefDescriptor(Coordinate coordinate, List<(Coordinate, Coordinate)> gaussianKeyPairs, Matrix<Grayscale> image)
    {
        var descriptor = BigInteger.Zero;
        var dimensions = image.Dimensions;

        foreach (var gaussianKeyPair in gaussianKeyPairs)
        {
            descriptor <<= 1;
            var testCoordinate1 = coordinate.Add(gaussianKeyPair.Item1);

            if (!testCoordinate1.IsInPositiveBounds(dimensions))
                continue;

            var testCoordinate2 = coordinate.Add(gaussianKeyPair.Item2);

            if (!testCoordinate2.IsInPositiveBounds(dimensions))
                continue;

            var testValue1 = GetValueAtCoordinate(testCoordinate1, image).K;
            var testValue2 = GetValueAtCoordinate(testCoordinate2, image).K;

            if (testValue1 < testValue2)
            {
                descriptor += 1;
            }
        }

        return descriptor;
    }

    public static Grayscale GetValueAtCoordinate(Coordinate testCoordinate, Matrix<Grayscale> image)
    {
        // TODO move this into image?
        return image[testCoordinate.X, testCoordinate.Y];
    }
}