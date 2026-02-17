using ImageProcessing.Options;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using Microsoft.Extensions.Options;

namespace ImageProcessing.Tests;

public class KeypointDetectionTests
{
    [Fact]
    public void TestCandidateDetection_DimCenterIsDetected()
    {
        var keypointDetection = BuildDetector();

        // TODO is there a way to keep these methods internal?
        var candidateMatrix =
            new Matrix<Grayscale>(new MatrixDimensions { Height = 7, Width = 7 }, new Grayscale { K = 0 })
            {
                [(ushort)3, (ushort)3] = new Grayscale { K = 0 },
                [(ushort)3, (ushort)0] = new Grayscale { K = 1 },
                [(ushort)0, (ushort)3] = new Grayscale { K = 1 },
                [(ushort)3, (ushort)6] = new Grayscale { K = 1 },
                [(ushort)6, (ushort)3] = new Grayscale { K = 1 }
            };

        Assert.True(keypointDetection.IsPotentialKeypoint(candidateMatrix, 0, 3, 3));
    }

    [Fact]
    public void TestCandidateDetection_ConsistentBrightnessIsNotDetected()
    {
        var keypointDetection = BuildDetector();

        // TODO is there a way to keep these methods internal?
        var candidateMatrix =
            new Matrix<Grayscale>(new MatrixDimensions { Height = 7, Width = 7 }, new Grayscale { K = 0.5f });

        Assert.False(keypointDetection.IsPotentialKeypoint(candidateMatrix, 0.5f, 3, 3));
    }

    [Fact]
    public void TestGetIntensityValueIfKeypoint_ConstantBrightnessIsNotDetected()
    {
        var keypointDetection = BuildDetector();

        var candidateMatrix =
            new Matrix<Grayscale>(new MatrixDimensions { Height = 7, Width = 7 }, new Grayscale { K = 0.5f });

        Assert.False(keypointDetection.GetIntensityValueIfKeypoint(candidateMatrix, 3, 3).HasValue);
    }

    private static KeypointDetection BuildDetector(KeypointDetectionOptions? keypointDetectionOptions = null)
    {
        return new KeypointDetection(
            new OptionsWrapper<KeypointDetectionOptions>(keypointDetectionOptions ?? BuildOptions()));
    }

    private static KeypointDetectionOptions BuildOptions(float? threshold = null,
        ushort? gaussianStandardDeviation = null, ushort? numGaussianPairs = null)
    {
        return new KeypointDetectionOptions
        {
            Threshold = threshold ?? 0.5f,
            GaussianStandardDeviation = gaussianStandardDeviation ?? 50,
            NumGaussianPairs = numGaussianPairs ?? 256
        };
    }
}