using ImageProcessing.Options;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using MathNet.Numerics.RootFinding;
using Microsoft.Extensions.Options;

namespace ImageProcessing;

public class DeWarp
{
    private readonly DeWarpOptions _deWarpOptions;
    private readonly Dictionary<int, double> _rd2ToRootCache = new();

    public DeWarp(IOptions<DeWarpOptions> deWarpOptions)
    {
        _deWarpOptions = deWarpOptions.Value;
    }

    public static Matrix<TPixel> ApplyDistortionMat<TPixel>(Matrix<TPixel> image, Matrix<Uv> distortionMatrix)
    {
        // TODO could move somewhere like on the distortion mat itself...
        if (!Equals(image.Dimensions, distortionMatrix.Dimensions))
            throw new ArgumentException("Dimensions of image and distortion matrix must be equal.");

        var resultImage = new Matrix<TPixel>(image.Dimensions);

        for (ushort x = 0; x < image.Dimensions.Cols; x++)
        {
            for (ushort y = 0; y < image.Dimensions.Rows; y++)
            {
                var newPixel = distortionMatrix[x, y];
                resultImage[x, y] = image[(ushort)newPixel.U, (ushort)newPixel.V];
            }
        }
        
        return resultImage;
    }
    
    public Matrix<Uv> GetDistortionMatrix()
    {
        // TODO could just store the matrix in a Lazy<T>.
        // But, note that the matrix might already be stored somewhere
        
        // TODO we may just want to make this static
        // TODO implement caching for the matrix.
        if (_deWarpOptions.DistortionCoefficients.Length != 5)
             // TODO is there a way we can force this through params, without requiring 5 params? Perhaps it's own class...
            throw new ArgumentException("You must pass exactly 5 distortion coefficients");
        
        var distortionMatrix = new Matrix<Uv>(_deWarpOptions.MatrixDimensions);

        // TODO think about how these are being cast
        var x0 = _deWarpOptions.MatrixDimensions.Cols / 2d;
        var y0 = _deWarpOptions.MatrixDimensions.Rows / 2d;
        
        for (ushort u = 0; u < _deWarpOptions.MatrixDimensions.Cols; u ++)
        {
            for (ushort v = 0; v < _deWarpOptions.MatrixDimensions.Rows; v++)
            {
                var x = (int)(u - x0);
                var y = (int)(v - y0);

                var rd2 = x * x + y * y;

                if (!_rd2ToRootCache.TryGetValue(rd2, out var root))
                {
                    var rd = Math.Sqrt(rd2);
                
                    var b = (rd * _deWarpOptions.DistortionCoefficients[3] - _deWarpOptions.DistortionCoefficients[0]) /
                            (rd * _deWarpOptions.DistortionCoefficients[4] - _deWarpOptions.DistortionCoefficients[1]);
                    var c = (rd * _deWarpOptions.DistortionCoefficients[2] - 1) /
                            (rd * _deWarpOptions.DistortionCoefficients[4] - _deWarpOptions.DistortionCoefficients[1]);
                    var d = rd / (rd * _deWarpOptions.DistortionCoefficients[4] - _deWarpOptions.DistortionCoefficients[1]);
                
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