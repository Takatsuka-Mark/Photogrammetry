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

    public static double CoordinateDistance(Coordinate c1, Coordinate c2){
        return Math.Sqrt(Math.Pow(c2.X - c1.X, 2) + Math.Pow(c2.Y - c1.Y, 2));
    }

    public static void CreatePointCloud(string filename, List<MathNet.Numerics.LinearAlgebra.Vector<float>> points){
        // TODO add some validation
        using var fs = File.OpenWrite(filename);
        using var sw = new StreamWriter(fs);
        
        sw.WriteLine("ply");
        sw.WriteLine("format ascii 1.0");
        sw.WriteLine($"element vertex {points.Count}");
        sw.WriteLine($"property float x");
        sw.WriteLine($"property float y");
        sw.WriteLine($"property float z");
        sw.WriteLine("end_header");
        
        foreach(var point in points){
            sw.WriteLine($"{point[0]} {point[1]} {point[2]}");
        }
    }
}