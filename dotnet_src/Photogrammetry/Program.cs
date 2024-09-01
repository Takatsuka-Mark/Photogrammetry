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
        // Dewarping tests
        // var image = imageReader.ReadImageFromDirectory("straight_edge_1920x1080.jpg");
        // var result = TestDeWarp(image);
        // imageReader.WriteImageToDirectory(result, "output");

        // Keypoint Detection Tests
        var image = imageReader.ReadImageFromDirectory("15pt_star.png");
        TestKeypointDetection(image);

        
        Console.WriteLine($"Elapsed: {sw.Elapsed}");
    }

    public static Matrix<Rgba> TestDeWarp(Matrix<Rgba> inputImage)
    {
        var swNoIo = new Stopwatch();
        swNoIo.Start();
        var deWarp = new DeWarp(new MatrixDimensions(1920, 1080));
        var distortionMatrix = deWarp.GetDistortionMatrix(new[] {3e-4, 1e-7, 0, 0, 0});
        var distMatTime = swNoIo.Elapsed;
        swNoIo.Restart();
        
        var result = DeWarp.ApplyDistortionMat(inputImage, distortionMatrix);
        
        Console.WriteLine($"Elapsed while de-warping: Generating Distortion Map: {distMatTime}, Applying: {swNoIo.Elapsed}");
        return result;
    }

    public static void TestKeypointDetection(Matrix<Rgba> inputImage)
    {
        var swNoIo = new Stopwatch();
        swNoIo.Start();
        
        var keypointDetector = new KeypointDetection(0.5f, 50, 256);

        var initializeTime = swNoIo.Elapsed;
        swNoIo.Restart();
        
        // TODO note this conversion ignores `a`
        var bwImage = inputImage.Convert(Grayscale.FromRgba);
        var keypoints = keypointDetector.Detect(bwImage);
        // TODO there are better ways to store these points. Maybe with spatial hashing?

        Console.WriteLine($"Elapsed while detecting keypoints: Initialize: {initializeTime}, Detecting: {swNoIo.Elapsed}");
        Console.WriteLine($"Found: {keypoints.Count} key points");

        foreach (var keypoint in keypoints)
        {
            inputImage.DrawSquare(keypoint.Coordinate.X, keypoint.Coordinate.Y,
                5, new Rgba { A = 127, B = 255, G = 0, R = 0 });
        }

        // TODO probably change the name of this image reader...
        var imageReader = new LocalImageReader();
        imageReader.WriteImageToDirectory(inputImage, "dotnet_keypoints");
    }
}