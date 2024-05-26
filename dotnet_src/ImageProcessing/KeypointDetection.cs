using ImageProcessing.Abstractions;
using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing;


// Using FAST
public class KeypointDetection
{
    private readonly float _threshold;

    // TODO determine if there's a better way to store this
    // TODO dynamically create these?
    private readonly Matrix<int> _bresenhamCircle3 = Matrix<int>.FromArray(new[,]{{-3, 0}, {-3, 1}, {-2, 2}, {-1, 3}, {0, 3}, {1, 3}, {2, 2}, {3, 1}, {3, 0}, {3, -1}, {2, -2}, {1, -3}, {0, -3}, {-1, -3}, {-2, -2}, {-3, 1}});
    private readonly Matrix<int> _miniBresenhamCircle3 = Matrix<int>.FromArray(new[,] { { -3, 0 }, {0 , 3}, {3,0}, {0, -3} });
    private readonly Matrix<int> _bresenhamCircle3T;
    private readonly Matrix<int> _miniBresenhamCircle3T;

    public KeypointDetection(float threshold)
    {
        // Since grayscale is a float from 0 to 1, setting the threshold to a float. Could convert to a int [0, 255] instead (like original)
        _threshold = threshold; // The difference between point's intensity for it to be considered a keypoint
        _bresenhamCircle3T = _bresenhamCircle3.Transpose();
        _miniBresenhamCircle3T = _miniBresenhamCircle3.Transpose();
    }

    public List<Keypoint> Detect(Matrix<Grayscale> image)
    {
        var keypoints = new List<Keypoint>();
        for (var y = 3; y < image.Dimensions.Height - 3; y++)
        {
            for (var x = 3; x < image.Dimensions.Width - 3; x++)
            {
                if (IsKeypoint(image, x, y))
                {
                    keypoints.Add(new Keypoint(0, x, y)); // TODO once images are IDed, start passing this value
                }
            }    
        }
        return keypoints;
    }
    
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
