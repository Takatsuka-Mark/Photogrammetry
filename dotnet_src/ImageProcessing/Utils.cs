using ImageProcessing.Abstractions;
using Images.Abstractions;

namespace ImageProcessing;

public class Utils
{
    private readonly Random _random;

    public Utils()
    {
        _random = new Random();
    }

    public (Coordinate, Coordinate) NextGaussianPair(int standardDeviation)
    {
        return (NextGaussianCoordinate(standardDeviation), NextGaussianCoordinate(standardDeviation));
    }
    
    public Coordinate NextGaussianCoordinate(int standardDeviation)
    {
        // Using Marsaglia Polar Method
        // See https://people.maths.ox.ac.uk/gilesm/mc/mc/lec1.pdf slide 14

        double y1;
        double y2;
        double rSquared;

        do
        {
            y1 = _random.NextDouble();
            y2 = _random.NextDouble();

            rSquared = y1 * y1 + y2 * y2;
        } while (rSquared >= 1);

        var s = Math.Sqrt(-2 * Math.Log(rSquared) / rSquared);
        return new Coordinate { X = (int)(s * y1 * standardDeviation), Y = (int)(s * y2 * standardDeviation) };
    }

    public static Coordinate ClampCoordinate(MatrixDimensions clampDimensions, Coordinate coordinate)
    {
        return new Coordinate
        {
            X = Math.Max(Math.Min(coordinate.X, clampDimensions.Width - 1), 0),
            Y = Math.Max(Math.Min(coordinate.Y, clampDimensions.Height - 1), 0)
        };
    }
}