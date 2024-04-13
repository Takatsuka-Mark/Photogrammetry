using System.Diagnostics;
using ImageProcessing;
using ImageReader.LocalImageReader;
using Images.Abstractions;

namespace Photogrammetry;

public class Program
{
    public static void Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();
        // var imageReader = new LocalImageReader();
        // var image = imageReader.ReadImageFromDirectory("straight_edge_1920x1080.jpg");
        // imageReader.WriteImageToDirectory(image, "output");

        var deWarp = new DeWarp(new ImageDimensions(1920, 1080));
        var distortionMatrix = deWarp.GetDistortionMatrix(new double[5] {3e-4, 1e-7, 0, 0, 0});
        Console.WriteLine(distortionMatrix[0, 0]);

        Console.WriteLine($"Elapsed: {sw.Elapsed}");
    }
}