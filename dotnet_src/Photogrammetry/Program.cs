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
        // var image = imageReader.ReadImageFromDirectory("15pt_star.png");
        // TestKeypointDetection(image);

        // Keypoint Mathcing Tests
        var image1 = imageReader.ReadImageFromDirectory("15pt_star.png");
        // var image2 = imageReader.ReadImageFromDirectory("15pt_star_shifted_150.png");
        // TestKeypointMatching(image1, image2);

        image1.DrawLine(new Coordinate{X = 10, Y = 10}, new Coordinate{Y = 300, X = 11}, new Rgba{R = 100, A = 255});
        imageReader.WriteImageToDirectory(image1, "test_line");
        
        
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

    public static void TestKeypointMatching(Matrix<Rgba> inputImage1, Matrix<Rgba> inputImage2)
    {
        var swNoIo = new Stopwatch();
        swNoIo.Start();

        var keypointDetector = new KeypointDetection(0.5f, 50, 256);
        
        var bwImage1 = inputImage1.Convert(Grayscale.FromRgba);
        var keypoints1 = keypointDetector.Detect(bwImage1);

        var bwImage2 = inputImage2.Convert(Grayscale.FromRgba);
        var keypoints2 = keypointDetector.Detect(bwImage2);

        var keypointMatching = new KeypointMatching(100);
        var matchedPairs = keypointMatching.MatchKeypoints(keypoints1, keypoints2);

        foreach (var keypointPair in matchedPairs)
        {
            Console.WriteLine($"Keypoint1: {keypointPair.Keypoint1}, keypoint2: {keypointPair.Keypoint2}");
        }
    }
}