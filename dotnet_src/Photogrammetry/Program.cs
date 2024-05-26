using System.Diagnostics;
using ImageProcessing;
using ImageReader.LocalImageReader;
using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace Photogrammetry;

public class Program
{
    public static void Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();
        var imageReader = new LocalImageReader();
        var image = imageReader.ReadImageFromDirectory("straight_edge_1920x1080.jpg");

        var result = TestDeWarp(image);
        
        imageReader.WriteImageToDirectory(result, "output");
        Console.WriteLine($"Elapsed: {sw.Elapsed}");
    }

    public static Image<Rgba> TestDeWarp(Image<Rgba> inputImage)
    {
        var swNoIo = new Stopwatch();
        swNoIo.Start();
        var deWarp = new DeWarp(new ImageDimensions(1920, 1080));
        var distortionMatrix = deWarp.GetDistortionMatrix(new[] {3e-4, 1e-7, 0, 0, 0});
        var distMatTime = swNoIo.Elapsed;
        swNoIo.Restart();
        
        var result = DeWarp.ApplyDistortionMat(inputImage, distortionMatrix);
        
        Console.WriteLine($"Elapsed while de-warping: Generating Distortion Map: {distMatTime}, Applying: {swNoIo.Elapsed}");
        return result;
    }
}