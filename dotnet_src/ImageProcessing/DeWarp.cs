using ImageProcessing.Abstractions;
using Images.Abstractions;
using Images.Abstractions.Pixels;
using MathNet.Numerics.RootFinding;

namespace ImageProcessing;

public class DeWarp
{
    private readonly MatrixDimensions _dimensions;
    private readonly Dictionary<int, double> _rd2ToRootCache = new();

    public DeWarp(MatrixDimensions dimensions)
    {
        _dimensions = dimensions;
        
    }

    public static Matrix<TPixel> ApplyDistortionMat<TPixel>(Matrix<TPixel> image, DistortionMatrix distortionMatrix)
    {
        // TODO could move somewhere like on the distortion mat itself...
        if (!Equals(image.Dimensions, distortionMatrix.Dimensions))
            throw new ArgumentException("Dimensions of image and distortion matrix must be equal.");

        var resultImage = new Matrix<TPixel>(image.Dimensions);

        for (var x = 0; x < image.Dimensions.Width; x++)
        {
            for (int y = 0; y < image.Dimensions.Height; y++)
            {
                var newPixel = distortionMatrix[x, y];
                resultImage[x, y] = image[newPixel.U, newPixel.V];
            }
        }
        
        return resultImage;
    }
    
    public DistortionMatrix GetDistortionMatrix(double[] distortionCoefficients)
    {
        // TODO we may just want to make this static
        // TODO implement caching for the matrix.
        if (distortionCoefficients.Length != 5)
             // TODO is there a way we can force this through params, without requiring 5 params? Perhaps it's own class...
            throw new ArgumentException("You must pass exactly 5 distortion coefficients");
        
        var distortionMatrix = new DistortionMatrix(_dimensions);

        // TODO think about how these are being cast
        var x0 = _dimensions.Width / 2d;
        var y0 = _dimensions.Height / 2d;
        
        for (var u = 0; u < _dimensions.Width; u ++)
        {
            for (var v = 0; v < _dimensions.Height; v++)
            {
                var x = (int)(u - x0);
                var y = (int)(v - y0);

                var rd2 = x * x + y * y;

                if (!_rd2ToRootCache.TryGetValue(rd2, out var root))
                {
                    var rd = Math.Sqrt(rd2);
                
                    var b = (rd * distortionCoefficients[3] - distortionCoefficients[0]) /
                            (rd * distortionCoefficients[4] - distortionCoefficients[1]);
                    var c = (rd * distortionCoefficients[2] - 1) /
                            (rd * distortionCoefficients[4] - distortionCoefficients[1]);
                    var d = rd / (rd * distortionCoefficients[4] - distortionCoefficients[1]);
                
                    // TODO write own program to calculate roots
                    var (root1, root2, root3) = Cubic.RealRoots(d, c, b); // TODO what order should these be entered?

                    var sortedRoots = (new List<double> { root1, root2, root3 }).Where(r => !double.IsNaN(r)).ToList();
                    sortedRoots.Sort();
                
                    // TODO this might be necessary with different coefficients, but with {3e-4, 1e-7, 0, 0, 0} it is always 1 root.
                    var calculatedRoot = sortedRoots.Count == 3 ? sortedRoots[1] : sortedRoots[0];

                    root = calculatedRoot;
                    _rd2ToRootCache.Add(rd2, root);
                }

                // var root = sortedRoots[0];
                // if (u == 0 && v == 0)
                // {
                //     Console.WriteLine($"x:{x} y:{y} rd:{rd} b:{b} c:{c} d:{d} root:{root}");
                // }
                var theta = Math.Atan2(y, x);

                var xd = root * Math.Cos(theta);
                var yd = root * Math.Sin(theta);

                distortionMatrix[u, v] = new Uv
                {
                    U = (int)(xd + x0),
                    V = (int)(yd + y0)
                };
            }
        }
        
        return distortionMatrix;
    }
}