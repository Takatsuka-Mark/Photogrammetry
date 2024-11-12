using ImageProcessing.Abstractions;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace ImageProcessing;

public class CameraPoseEstimation{
    public CameraPoseEstimation(){
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


    public List<KeypointPair> OutlierRejection(IList<KeypointPair> keypointPairs, int numSamples, int numPairsPerSample, float threshold){
        if (numPairsPerSample < 8)
            throw new InvalidOperationException("At least 8 keypoint pairs must be included per sample");
        
        if (keypointPairs.Count < numPairsPerSample)
            throw new InvalidOperationException("Must provide at least as many keypoint pairs as pairs required per sample");
        
        // See https://cmsc426.github.io/sfm/#featmatch for ransac impl
        var random = new Random();
        var bestSample = new List<KeypointPair>();
        for (var sampleIdx = 0; sampleIdx < numSamples; sampleIdx += 1){

            var samples = keypointPairs.OrderBy(_ => random.NextSingle()).Take(numPairsPerSample).ToList();

            var f = EstimateFundamentalMatrix(samples);
            var workingPairs = new List<KeypointPair>();

            foreach (var keypointPair in keypointPairs){
                var kp1Mat = Matrix<float>.Build.DenseOfRowMajor(3, 1, new List<float>() {keypointPair.Keypoint2.Coordinate.X, keypointPair.Keypoint2.Coordinate.Y, 1});    // TODO if needed for perf (and not clarity), just build transposed.
                var kp2Mat = Matrix<float>.Build.DenseOfRowMajor(3, 1, new List<float>() {keypointPair.Keypoint1.Coordinate.X, keypointPair.Keypoint1.Coordinate.Y, 1});
                if (kp2Mat.Transpose().Multiply(f.Multiply(kp1Mat))[0, 0] <= threshold){
                    workingPairs.Add(keypointPair);
                }
            }

            if (workingPairs.Count > bestSample.Count){
                // TODO should this also have an out var for the best fundamental matrix?
                bestSample = workingPairs;
            }
        }

        return bestSample;
    }

    internal Matrix<float> EstimateFundamentalMatrix(IList<KeypointPair> keypointPairs){
        if (keypointPairs.Count < 8)
            throw new InvalidOperationException("At least 8 keypoint pairs must be provided");
        
        var mat = new DenseMatrix(keypointPairs.Count, 9);

        throw new NotImplementedException();
        // TODO should actually normalize these values, from -1 - 1. See 4.1 of https://web.stanford.edu/class/cs231a/course_notes/03-epipolar-geometry.pdf
        
        for (var i = 0; i < keypointPairs.Count; i += 1){
            var kpPair = keypointPairs[i];
            
            var coord1 = kpPair.Keypoint1.Coordinate;
            var coord2 = kpPair.Keypoint2.Coordinate;

            mat[i, 0] = coord1.X * coord2.X;
            mat[i, 1] = coord1.X * coord2.Y;
            mat[i, 2] = coord1.X;
            mat[i, 3] = coord1.Y * coord2.X;
            mat[i, 4] = coord1.Y * coord2.Y;
            mat[i, 5] = coord1.Y;
            mat[i, 6] = coord2.X;
            mat[i, 7] = coord2.Y;
            mat[i, 8] = 1;
        }

        var svd = mat.Svd();

        var lastRow = svd.VT.Row(svd.VT.RowCount - 1);
        return Matrix<float>.Build.DenseOfColumnMajor(3, 3, lastRow);
    }
}