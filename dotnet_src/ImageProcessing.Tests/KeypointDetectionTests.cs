using ImageProcessing.Options;

namespace ImageProcessing.Tests;

public class KeypointDetectionTests
{
    [Fact]
    public void Test1()
    {
    }

    private KeypointDetectionOptions BuildOptions()
    {
        return new KeypointDetectionOptions
        {
            Threshold = 0.5f,
            GaussianStandardDeviation = 50,
            NumGaussianPairs = 256
        };
    }
}
