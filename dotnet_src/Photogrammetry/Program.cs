using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImageProcessing;
using ImageReader.LocalImageReader;
using Images.Abstractions;
using Images.Abstractions.Pixels;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Configuration;
using ScottPlot;
using SixLabors.ImageSharp;

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
        // var image1 = imageReader.ReadImageFromDirectory("15pt_star.png");
        // var image2 = imageReader.ReadImageFromDirectory("15pt_star_shifted_150.png");
        // var image1 = imageReader.ReadImageFromDirectory("lego_space_1_from_left.jpg");
        // var image2 = imageReader.ReadImageFromDirectory("lego_space_1_from_right.jpg");
        // TestKeypointMatching(image1, image2);

        // image1.DrawLine(new Coordinate{X = 10, Y = 10}, new Coordinate{Y = 300, X = 11}, new Rgba{R = 100, A = 255});
        // imageReader.WriteImageToDirectory(image1, "test_line");

        // var image = imageReader.ReadImageFromDirectory("15pt_star.png");
        // var image = imageReader.ReadImageFromDirectory("lego_space_1_from_left.jpg");
        // TestRedundantKeypointElimination(image);
        // System.Console.WriteLine(image.Dimensions.Width);
        // System.Console.WriteLine(image.Dimensions.Height);

        // var image1 = imageReader.ReadImageFromDirectory("15pt_star.png");
        // var image2 = imageReader.ReadImageFromDirectory("15pt_star_shifted_150.png");
        // TestKeypointMatching(image1, image2);

        // var image1 = imageReader.ReadImageFromDirectory("15pt_star.png");
        // var image2 = imageReader.ReadImageFromDirectory("15pt_star_shifted_150.png");
        // var image1 = imageReader.ReadImageFromDirectory("lego_space_1_from_left.jpg");
        // var image2 = imageReader.ReadImageFromDirectory("lego_space_1_from_right.jpg");
        // EstimateCameraPose(image1, image2);

        var configuration = SetupConfiguration();

        Console.WriteLine($"Elapsed: {sw.Elapsed}");
    }

    public static IConfiguration SetupConfiguration(){
        var env = Environment.GetEnvironmentVariable("PHOTOGRAMMETRY_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(env))
            throw new Exception("Environment variable \"PHOTOGRAMMETRY_ENVIRONMENT\" must be set");
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.ToLower()}.json", optional: true, reloadOnChange: true)
            .Build();

        return configuration;
    }

    public static Matrix<Rgba> TestDeWarp(Matrix<Rgba> inputImage)
    {
        var swNoIo = new Stopwatch();
        swNoIo.Start();
        var deWarp = new DeWarp(new MatrixDimensions(1920, 1080));
        var distortionMatrix = deWarp.GetDistortionMatrix(new[] { 3e-4, 1e-7, 0, 0, 0 });
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

    public static void TestRedundantKeypointElimination(Matrix<Rgba> inputImage)
    {
        var swNoIo = new Stopwatch();
        swNoIo.Start();

        var keypointDetector = new KeypointDetection(0.2f, 50, 256);

        var initializeTime = swNoIo.Elapsed;
        swNoIo.Restart();

        // TODO note this conversion ignores `a`
        var bwImage = inputImage.Convert(Grayscale.FromRgba);
        var keypoints = keypointDetector.Detect(bwImage);
        // TODO there are better ways to store these points. Maybe with spatial hashing?

        Console.WriteLine($"Elapsed while detecting keypoints: Initialize: {initializeTime}, Detecting: {swNoIo.Elapsed}");
        Console.WriteLine($"Found: {keypoints.Count} key points");

        // TODO param is totally arbitrary and not research based. Fits for 15pt star.
        var rke = new RedundantKeypointEliminator((int)(inputImage.Dimensions.Width * 0.015D));

        swNoIo.Restart();
        keypoints = rke.EliminateRedundantKeypoints(keypoints);
        Console.WriteLine($"Reduced to {keypoints.Count} key points in {swNoIo.Elapsed}");

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

        var keypointDetector = new KeypointDetection(0.2f, 50, 256);
        var rke = new RedundantKeypointEliminator((int)(inputImage1.Dimensions.Width * 0.015f));

        var bwImage1 = inputImage1.Convert(Grayscale.FromRgba);
        var keypoints1 = rke.EliminateRedundantKeypoints(keypointDetector.Detect(bwImage1));

        var bwImage2 = inputImage2.Convert(Grayscale.FromRgba);
        var keypoints2 = rke.EliminateRedundantKeypoints(keypointDetector.Detect(bwImage2));

        var keypointMatching = new KeypointMatching(100);
        var matchedPairs = keypointMatching.MatchKeypoints(keypoints1, keypoints2);

        // foreach (var keypointPair in matchedPairs)
        // {
        //     Console.WriteLine($"Keypoint1: {keypointPair.Keypoint1}, keypoint2: {keypointPair.Keypoint2}");
        // }

        System.Console.WriteLine($"Found {matchedPairs.Count} matching keypoint pairs");

        var outputImage = new Matrix<Rgba>(new MatrixDimensions(inputImage1.Dimensions.Width * 2, inputImage1.Dimensions.Height));

        // Copy images to output
        for (var x = 0; x < inputImage1.Dimensions.Width; x += 1)
        {
            for (var y = 0; y < inputImage1.Dimensions.Height; y += 1)
            {
                outputImage[x, y] = inputImage1[x, y];
            }
        }

        for (var x = 0; x < inputImage1.Dimensions.Width; x += 1)
        {
            for (var y = 0; y < inputImage1.Dimensions.Height; y += 1)
            {
                outputImage[x + inputImage1.Dimensions.Width, y] = inputImage2[x, y];
            }
        }

        foreach (var keypointPair in matchedPairs)
        {
            var k2Coordinate = new Coordinate
            {
                X = keypointPair.Keypoint2.Coordinate.X + inputImage1.Dimensions.Width,
                Y = keypointPair.Keypoint2.Coordinate.Y
            };
            outputImage.DrawLine(keypointPair.Keypoint1.Coordinate, k2Coordinate, new Rgba { R = 100, A = 255 });
        }

        var imageReader = new LocalImageReader();
        imageReader.WriteImageToDirectory(outputImage, "dotnet_paired_keypoints");
    }

    public static void EstimateCameraPose(Matrix<Rgba> inputImage1, Matrix<Rgba> inputImage2)
    {
        var swNoIo = new Stopwatch();
        swNoIo.Start();

        var keypointDetector = new KeypointDetection(0.2f, 50, 256);
        var rke = new RedundantKeypointEliminator((int)(inputImage1.Dimensions.Width * 0.015f));

        var bwImage1 = inputImage1.Convert(Grayscale.FromRgba);
        var keypoints1 = rke.EliminateRedundantKeypoints(keypointDetector.Detect(bwImage1));

        var bwImage2 = inputImage2.Convert(Grayscale.FromRgba);
        var keypoints2 = rke.EliminateRedundantKeypoints(keypointDetector.Detect(bwImage2));

        System.Console.WriteLine($"Found {keypoints1.Count} keypoints from Image 1");
        System.Console.WriteLine($"Found {keypoints2.Count} keypoints from Image 2");

        var keypointMatching = new KeypointMatching(100);
        var matchedPairs = keypointMatching.MatchKeypoints(keypoints1, keypoints2);

        var cpe = new CameraPoseEstimation();

        var (samples, fundamentalMatrix) = cpe.GetFundamentalMatrix(matchedPairs, 2000, 32, 0.001f);

        var errors = new List<float>();

        foreach (var sample in samples)
        {
            var kp1Mat = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(
                [sample.Keypoint2.Coordinate.X, sample.Keypoint2.Coordinate.Y, 1]);
            var kp2Mat = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(
                [sample.Keypoint1.Coordinate.X, sample.Keypoint1.Coordinate.Y, 1]);

            var result = fundamentalMatrix.Multiply(kp1Mat).DotProduct(kp2Mat);
            // var result = kp1Mat.ToColumnMatrix().Multiply(fundamentalMatrix).Multiply(kp2Mat);

            errors.Add(Math.Abs(result));
        }

        System.Console.WriteLine($"Average Error: {errors.Mean()}");

        cpe.EstimateCameraPose(samples, fundamentalMatrix);
    }
}