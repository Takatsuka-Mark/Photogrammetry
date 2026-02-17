using System.Numerics;
using Images.Abstractions;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using Microsoft.VisualBasic.CompilerServices;

namespace ImageProcessing.Abstractions;

public class Keypoint
{
    private readonly int _imageId;
    public readonly Coordinate Coordinate;
    public readonly int FastScore;
    public readonly BigInteger BriefDescriptor;
    public Grayscale Value { get; init; }

    public Keypoint(int imageId, Coordinate coordinate, List<(Coordinate, Coordinate)> guassianKeyPairs, Matrix<Grayscale> image, int fastScore)
    {
        _imageId = imageId;
        Coordinate = coordinate;
        FastScore = fastScore;

        // TODO determine a less clunky way to create the brief descriptor. Like don't pass all this into the constructor
        // TODO the brief descriptor should just be requested when needed. Or be a superclass of this like "keypoint with brief". Or myabe some form of metadata.
        BriefDescriptor = GetBriefDescriptor(Coordinate, guassianKeyPairs, image);
        Value = image[(ushort)Coordinate.X, (ushort)Coordinate.Y];
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

            var testValue1 = image[testCoordinate1].K;
            var testValue2 = image[testCoordinate2].K;

            if (testValue1 < testValue2)
            {
                descriptor += 1;
            }
        }

        return descriptor;
    }

    public Grayscale GetValue()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"({Coordinate.X}, {Coordinate.Y})";
    }
}