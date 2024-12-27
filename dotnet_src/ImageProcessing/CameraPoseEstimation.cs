using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using ImageProcessing.Abstractions;
using Images.Abstractions;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using Microsoft.VisualBasic;

namespace ImageProcessing;

public class CameraPoseEstimation
{
    public CameraPoseEstimation()
    {
        /// Essential matrix
        /// Let M and M' be the camera projection matricies
        /// Let R be the rotation, and T be the translation
        /// M = K [I, 0], M' = K' [R^T, - R^T T]
        /// ^ Applicable for "Canonical cameras" meaning they are parallel
        /// 
        /// Need more general. Introducing, the Fundamental Matrix
        /// p sub c = (K^-1 * p) and p' sub c = (K'^-1) * p'
        /// Defined as projections of P to the corresponding cameras if the cameras were canonical. Then we get some cursed value. From standford.edu
        /// F = K'^(-T) [T sub x] R K'^(-1)
        /// K and K' are the camera matricies.
        /// 7 DOF
    }


    public (List<KeypointPair> Samples, MathNet.Numerics.LinearAlgebra.Matrix<float> FundamentalMatrix) GetFundamentalMatrix(IList<KeypointPair> keypointPairs, int numSamples, int numPairsPerSample, float threshold)
    {
        if (numPairsPerSample < 8)
            throw new InvalidOperationException("At least 8 keypoint pairs must be included per sample");

        if (keypointPairs.Count < numPairsPerSample)
            throw new InvalidOperationException("Must provide at least as many keypoint pairs as pairs required per sample");

        // See https://cmsc426.github.io/sfm/#featmatch for ransac impl
        var random = new Random();
        var bestSample = new List<KeypointPair>();
        MathNet.Numerics.LinearAlgebra.Matrix<float>? bestF = null;
        for (var sampleIdx = 0; sampleIdx < numSamples; sampleIdx += 1)
        {

            var samples = keypointPairs.OrderBy(_ => random.NextSingle()).Take(numPairsPerSample).ToList();

            var f = EstimateFundamentalMatrix(samples);
            var workingPairs = new List<KeypointPair>();

            foreach (var keypointPair in keypointPairs)
            {
                var kp1Mat = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(3, 1, new List<float>() { keypointPair.Keypoint2.Coordinate.X, keypointPair.Keypoint2.Coordinate.Y, 1 });    // TODO if needed for perf (and not clarity), just build transposed.
                var kp2Mat = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(3, 1, new List<float>() { keypointPair.Keypoint1.Coordinate.X, keypointPair.Keypoint1.Coordinate.Y, 1 });
                var result = f.Multiply(kp1Mat);
                var result2 = kp2Mat.Transpose().Multiply(result);
                if (result2[0, 0] <= threshold) // TODO validate multiplication order
                {
                    workingPairs.Add(keypointPair);
                }
            }

            if (workingPairs.Count > bestSample.Count)
            {
                // TODO should this also have an out var for the best fundamental matrix?
                bestSample = workingPairs;
            }
        }

        if (bestF is null)
            throw new Exception("Failed computing the best fundamental matrix");
        return (bestSample, bestF);
    }

    internal MathNet.Numerics.LinearAlgebra.Matrix<float> EstimateFundamentalMatrix(IList<KeypointPair> keypointPairs)
    {
        if (keypointPairs.Count < 8)
            throw new InvalidOperationException("At least 8 keypoint pairs must be provided");

        var mat = new DenseMatrix(keypointPairs.Count, 9);

        var transformationMatrix1 = CalculateTransformationMatrix(keypointPairs.Select(kpp => kpp.Keypoint1.Coordinate).ToList());
        var transformationMatrix2 = CalculateTransformationMatrix(keypointPairs.Select(kpp => kpp.Keypoint2.Coordinate).ToList());
        // TODO should actually normalize these values, from -1 - 1. See 4.1 of https://web.stanford.edu/class/cs231a/course_notes/03-epipolar-geometry.pdf

        for (var i = 0; i < keypointPairs.Count; i += 1)
        {
            var kpPair = keypointPairs[i];

            (double X, double Y) coord1 = (transformationMatrix1.X * kpPair.Keypoint1.Coordinate.X, transformationMatrix1.Y * kpPair.Keypoint1.Coordinate.Y);
            (double X, double Y) coord2 = (transformationMatrix2.X * kpPair.Keypoint2.Coordinate.X, transformationMatrix2.Y * kpPair.Keypoint2.Coordinate.Y);

            mat[i, 0] = (float)(coord1.X * coord2.X);
            mat[i, 1] = (float)(coord1.X * coord2.Y);
            mat[i, 2] = (float)coord1.X;
            mat[i, 3] = (float)(coord1.Y * coord2.X);
            mat[i, 4] = (float)(coord1.Y * coord2.Y);
            mat[i, 5] = (float)coord1.Y;
            mat[i, 6] = (float)coord2.X;
            mat[i, 7] = (float)coord2.Y;
            mat[i, 8] = 1;
        }

        var svd = mat.Svd();

        var lastRow = svd.VT.Row(svd.VT.RowCount - 1);
        var fundamentalMatrix = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfColumnMajor(3, 3, lastRow);
        // TODO is the third value supposed to be 1?
        var transformationMatrix1New = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfColumnMajor(3, 1, new List<float>() { (float)transformationMatrix1.X, (float)transformationMatrix1.Y, 1 });
        var transformationMatrix2New = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfColumnMajor(3, 1, new List<float>() { (float)transformationMatrix2.X, (float)transformationMatrix2.Y, 1 });

        return transformationMatrix2New.Transpose().Multiply(fundamentalMatrix).Multiply(transformationMatrix2New);
    }

    internal (double X, double Y) CalculateTransformationMatrix(List<Coordinate> coordinates)
    {
        (double X, double Y) translationMatrix = (X: 0, Y: 0);

        var centroid = CalculateCentroid(coordinates);

        foreach (var coordinate in coordinates)
        {
            translationMatrix.X += Math.Pow(coordinate.X - centroid.X, 2);
            translationMatrix.Y += Math.Pow(coordinate.Y - centroid.Y, 2);
        }

        translationMatrix.X = 2 * coordinates.Count / translationMatrix.X;
        translationMatrix.Y = 2 * coordinates.Count / translationMatrix.Y;
        translationMatrix.X = Math.Pow(translationMatrix.X, 1 / 2);
        translationMatrix.Y = Math.Pow(translationMatrix.Y, 1 / 2);

        return translationMatrix;
    }

    internal (double X, double Y) CalculateCentroid(List<Coordinate> coordinates)
    {
        // TODO should really just make a good matrix representation.
        (double X, double Y) centroid = (X: 0, Y: 0);
        foreach (var coordinate in coordinates)
        {
            centroid.X += coordinate.X;
            centroid.Y += coordinate.Y;
        }
        centroid.X /= coordinates.Count;
        centroid.Y /= coordinates.Count;
        return centroid;
    }
}