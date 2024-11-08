using System.Reflection.Metadata;
using ImageProcessing.Abstractions;
using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing;


// Using FAST
public class KeypointDetection
{
    private readonly float _threshold;

    // TODO determine if there's a better way to store this
    private readonly Matrix<int> _bresenhamCircle3 = Matrix<int>.FromRowMajorArray(new[,]
    {
        { -3, 0 }, { -3, 1 }, { -2, 2 }, { -1, 3 }, { 0, 3 }, { 1, 3 }, { 2, 2 }, { 3, 1 }, { 3, 0 }, { 3, -1 },
        { 2, -2 }, { 1, -3 }, { 0, -3 }, { -1, -3 }, { -2, -2 }, { -3, 1 }
    });

    private readonly Matrix<int> _miniBresenhamCircle3 =
        Matrix<int>.FromRowMajorArray(new[,] { { -3, 0 }, { 0, 3 }, { 3, 0 }, { 0, -3 } });
    private readonly Matrix<int> _bresenhamCircle3T;
    private readonly Matrix<int> _miniBresenhamCircle3T;
    private readonly List<(Coordinate, Coordinate)> _gaussianKeypairs;

    public KeypointDetection(float threshold, int gaussianStdev, int numGaussians)
    {
        // FAST (Features from Accelerated Segment Test) keypoint detector
        // Since grayscale is a float from 0 to 1, setting the threshold to a float. Could convert to a int [0, 255] instead (like original)
        _threshold = threshold; // The difference between point's intensity for it to be considered a keypoint
        _bresenhamCircle3T = _bresenhamCircle3.Transpose();
        _miniBresenhamCircle3T = _miniBresenhamCircle3.Transpose();
        var utils = new Utils();

        _gaussianKeypairs = Enumerable.Range(0, numGaussians)
            .Select(_ => utils.NextGaussianPair(gaussianStdev)).ToList();
    }

    public List<Keypoint> Detect(Matrix<Grayscale> image)
    {
        var keypoints = new List<Keypoint>();
        for (var y = 3; y < image.Dimensions.Height - 3; y++)
        {
            for (var x = 3; x < image.Dimensions.Width - 3; x++)
            {
                var intensity = GetIntensityValueIfKeypoint(image, x, y);
                if (intensity.HasValue)
                {
                    keypoints.Add(new Keypoint(0, new Coordinate { X = x, Y = y }, _gaussianKeypairs, image, intensity.Value));
                }

                // if (IsKeypoint(image, x, y))
                // {
                //     keypoints.Add(new Keypoint(0, new Coordinate { X = x, Y = y }, _gaussianKeypairs, image)); // TODO once images are IDed, start passing this value
                // }
            }
        }
        return keypoints;
    }

    internal int? GetIntensityValueIfKeypoint(Matrix<Grayscale> image, int x, int y)
    {
        var intensity = image[x, y].K;

        if (!IsPotentialKeypoint(image, intensity, x, y))
            return null;

        var isBeginningConsec = true;
        var numBeginningConsec = 0;
        var longestConsec = 0;
        var currentConsec = 0;
        var numFail = 0;

        // TODO don't hardcode this value
        // TODO I don't like this loop, but I guess it works. Can we speed it up?
        for (var idx = 0; idx < 16; idx++)
        {
            // Fetch intensity at bresenham index 1
            if (InThreshold(intensity,
                    image[_bresenhamCircle3T[idx, 0] + x, _bresenhamCircle3T[idx, 1] + y].K))
            {
                isBeginningConsec = false;

                longestConsec = Math.Max(longestConsec, currentConsec);
                currentConsec = 0;

                if (numFail >= 4)
                    return null;

                numFail += 1;
            }
            else
            {
                currentConsec += 1;
                if (isBeginningConsec)
                {
                    numBeginningConsec += 1;
                }
            }
        }

        if (!isBeginningConsec)
        {
            // If we have a break in the ring, add the start to the final consecutive value.
            currentConsec += numBeginningConsec;
        }

        longestConsec = Math.Max(longestConsec, currentConsec);
        return longestConsec < 12 ? null : longestConsec;
    }

    [Obsolete("Use GetIntensityValueIfKeypoint instead. This does not properly handle the case where it is still the beginning of a consecutive loop")]
    internal bool IsKeypoint(Matrix<Grayscale> image, int x, int y)
    {
        var intensity = image[x, y].K;

        if (!IsPotentialKeypoint(image, intensity, x, y))
            return false;

        var isBeginningConsec = true;
        var numBeginningConsec = 0;
        var numConsec = 0;
        var numFail = 0;

        // TODO don't hardcode this value
        for (var idx = 0; idx < 16; idx++)
        {
            // Fetch intensity at bresenham index 1
            if (InThreshold(intensity,
                    image[_bresenhamCircle3T[idx, 0] + x, _bresenhamCircle3T[idx, 1] + y].K))
            {
                isBeginningConsec = false;
                numConsec = 0;

                if (numFail > 3)
                    return false;

                numFail += 1;
            }
            else
            {
                numConsec += 1;
                if (isBeginningConsec)
                {
                    numBeginningConsec += 1;
                }

                if (numConsec >= 12)
                    return true;
            }
        }

        if (isBeginningConsec)
            System.Console.WriteLine("HELP");

        numConsec += numBeginningConsec;
        return numConsec >= 12;
    }

    internal bool IsPotentialKeypoint(Matrix<Grayscale> image, float intensity, int x, int y)
    {
        var numInsideThreshold = 0;

        for (var idx = 0; idx < 4; idx++)
        {
            if (!InThreshold(intensity,
                    image[_miniBresenhamCircle3T[idx, 0] + x, _miniBresenhamCircle3T[idx, 1] + y].K))
                continue;

            if (numInsideThreshold > 0)
                return false;

            numInsideThreshold += 1;
        }

        return true;
    }

    internal bool InThreshold(float intensity, float testIntensity)
    {
        return testIntensity > intensity - _threshold && testIntensity < intensity + _threshold;
    }
}
