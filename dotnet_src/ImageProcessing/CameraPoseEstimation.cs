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


    public void OutlierRejection(IList<KeypointPair> keypointPairs, int numSamples, int numPairsPerSample){
        if (numPairsPerSample < 8)
            throw new InvalidOperationException("At least 8 keypoint pairs must be included per sample");
        
        if (keypointPairs.Count < numPairsPerSample)
            throw new InvalidOperationException("Must provide at least as many keypoint pairs as pairs required per sample");
        
        var random = new Random();

        var n = 0;
        var includedSample = new List<KeypointPair>();
        for (var sampleIdx = 0; sampleIdx < numSamples; sampleIdx += 1){

            var samples = keypointPairs.OrderBy(_ => random.NextSingle()).Take(numPairsPerSample).ToList();

            var f = EstimateFundamentalMatrix(samples);

            throw new NotImplementedException();
            // https://cmsc426.github.io/sfm/#featmatch for ransac
        }
    }

    public Matrix<float> EstimateFundamentalMatrix(IList<KeypointPair> keypointPairs){
        if (keypointPairs.Count < 8)
            throw new InvalidOperationException("At least 8 keypoint pairs must be provided");
        
        var mat = new DenseMatrix(keypointPairs.Count, 9);
        
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