using System.Net;
using ImageProcessing.Abstractions;
using Images.Abstractions;
using MathNet.Numerics.LinearAlgebra.Single;

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
        var badRankMatricies = 0;
        MathNet.Numerics.LinearAlgebra.Matrix<float>? bestF = null;
        for (var sampleIdx = 0; sampleIdx < numSamples; sampleIdx += 1)
        {

            var samples = keypointPairs.OrderBy(_ => random.NextSingle()).Take(numPairsPerSample).ToList();

            var f = EstimateFundamentalMatrix(samples);

            if (f.Rank() != 2){
                // TODO Should also do something when the rank is 3
                // sampleIdx -= 1;
                badRankMatricies += 1;
                continue;
            }

            var workingPairs = new List<KeypointPair>();

            foreach (var keypointPair in keypointPairs)
            {
                // var kp1Mat = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(
                //     3, 1, new List<float>() { keypointPair.Keypoint2.Coordinate.X, keypointPair.Keypoint2.Coordinate.Y, 1 });    // TODO if needed for perf (and not clarity), just build transposed.
                // var kp2Mat = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(
                //     3, 1, new List<float>() { keypointPair.Keypoint1.Coordinate.X, keypointPair.Keypoint1.Coordinate.Y, 1 });
                // var result = f.Multiply(kp1Mat);
                // var result2 = kp2Mat.Transpose().Multiply(result);
                // if (result2[0, 0] <= threshold) // TODO validate multiplication order
                // {
                //     workingPairs.Add(keypointPair);
                // }
                var kp1Mat = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(
                    [keypointPair.Keypoint2.Coordinate.X, keypointPair.Keypoint2.Coordinate.Y, 1]);
                var kp2Mat = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(
                    [keypointPair.Keypoint1.Coordinate.X, keypointPair.Keypoint1.Coordinate.Y, 1]);

                var result = f.Multiply(kp1Mat).DotProduct(kp2Mat);

                if (result <= threshold)
                {
                    workingPairs.Add(keypointPair);
                }
            }

            if (workingPairs.Count > bestSample.Count)
            {
                // TODO should this also have an out var for the best fundamental matrix?
                bestSample = workingPairs;
                bestF = f;
            }
        }

        if (bestF is null)
            throw new Exception("Failed computing the best fundamental matrix");
        System.Console.WriteLine($"Rank: {bestF.Rank()}. {(float)badRankMatricies / numSamples * 100}% were not 1");

        // TODO can introduce short circuit
        return (bestSample, bestF); // TODO: Is there some translation that needs to be done? See papaer.
    }

    public void EstimateCameraPose(IList<KeypointPair> keypointPairs, MathNet.Numerics.LinearAlgebra.Matrix<float> FundamentalMatrix)
    {
        var intrinsicMatrix = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(
            3, 3, new List<float>() { 1000, 0, 1500, 0, 1000, 2000, 0, 0, 1 }); // TODO needs to be computed per-image.
        var kInv = intrinsicMatrix.Inverse();

        var essentialMatrix = intrinsicMatrix.Transpose().Multiply(FundamentalMatrix).Multiply(intrinsicMatrix);
        System.Console.WriteLine($"Essential matrix rank: {essentialMatrix.Rank()}");

        var svdResults = essentialMatrix.Svd();
        System.Console.WriteLine($"Singular values {svdResults.S}");    // TODO might need to do something here
        var W = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(
            3, 3, new List<float>() { 0, -1, 0, 1, 0, 0, 0, 0, 1 });

        // TODO: Need to do some rank 2 stuff.
        var R1 = svdResults.U.Multiply(W).Multiply(svdResults.VT);
        var R2 = svdResults.U.Multiply(W.Transpose()).Multiply(svdResults.VT);

        var r1ScalingFactor = R1.Determinant() > 0 ? 1 : -1;
        var r2ScalingFactor = R2.Determinant() > 0 ? 1 : -1;

        var U1 = svdResults.U.Column(2);
        var U2 = U1 * -1;
        var candidates = new List<(MathNet.Numerics.LinearAlgebra.Vector<float> U, MathNet.Numerics.LinearAlgebra.Matrix<float> R)>(){
            (U1 * r1ScalingFactor, R1 * r1ScalingFactor),
            (U2 * r1ScalingFactor, R1 * r1ScalingFactor),
            (U1 * r2ScalingFactor, R2 * r2ScalingFactor),
            (U2 * r2ScalingFactor, R2 * r2ScalingFactor)
        };

        System.Console.WriteLine($"Found {keypointPairs.Count} keypoint pairs");

        // var mostPositiveDepths = 0;
        var numPositiveDepths = new int[4];
        var pointLists = new List<MathNet.Numerics.LinearAlgebra.Vector<float>>[4];

        var i = 0;
        foreach (var (U, R) in candidates)
        {

            var pointList = new List<MathNet.Numerics.LinearAlgebra.Vector<float>>();

            // var r3 = R.Row(2).ToColumnMatrix();

            var plt = new ScottPlot.Plot();
            var P1 = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseIdentity(3).InsertColumn(3, MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.Dense(3, 1).Column(0));
            var P2 = R.InsertColumn(3, U);

            System.Console.WriteLine(U.L2Norm());

            foreach (var keypointPair in keypointPairs)
            {
                // U ~ C ~ Camera Center
                // R ~ t ~ Rotation matrix

                // TODO chatGPT says this works. Does this make sense?
                var normalizedCoord1 = kInv * MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray([keypointPair.Keypoint1.Coordinate.X, keypointPair.Keypoint1.Coordinate.Y, 1]);
                var normalizedCoord2 = kInv * MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray([keypointPair.Keypoint2.Coordinate.X, keypointPair.Keypoint2.Coordinate.Y, 1]);
                var D = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(
                            4, 4, new List<List<float>>(){
                                (P1.Row(0) - P1.Row(2).Multiply(normalizedCoord1[0])).ToList(),
                                (P1.Row(2).Multiply(normalizedCoord1[1]) - P1.Row(1)).ToList(),
                                (P2.Row(0) - P2.Row(2).Multiply(normalizedCoord2[0])).ToList(),
                                (P2.Row(2).Multiply(normalizedCoord2[1]) - P2.Row(1)).ToList(),
                            }.SelectMany(a => a).ToList());


                // var D = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build
                //         .DenseOfRowMajor(4, 4, new List<List<float>>(){
                //             (P1.Row(0) - P1.Row(2).Multiply(keypointPair.Keypoint1.Coordinate.X)).ToList(),
                //             (P1.Row(2).Multiply(keypointPair.Keypoint1.Coordinate.Y) - P1.Row(1)).ToList(),
                //             (P2.Row(0) - P2.Row(2).Multiply(keypointPair.Keypoint2.Coordinate.X)).ToList(),
                //             (P2.Row(2).Multiply(keypointPair.Keypoint2.Coordinate.Y) - P2.Row(1)).ToList(),
                //         }.SelectMany(a => a).ToList());

                var newSvdResults = D.Svd();
                var X = newSvdResults.VT.Transpose().Column(3);

                var scaledX = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfEnumerable(X.Take(3).Select(v => v / X[3]).ToList());
                var point3d = R.Multiply(scaledX) + U;

                plt.Add.ScatterPoints(new List<float>() { point3d[0] }, [point3d[2]], ScottPlot.Colors.Blue);
                plt.Add.ScatterPoints(new List<float>() { point3d[1] }, [point3d[2]], ScottPlot.Colors.Red);

                pointList.Add(point3d);

                if (point3d[2] >= 0)
                {
                    numPositiveDepths[i] += 1;
                }
            }

            pointLists[i] = pointList;


            // System.Console.WriteLine(R);
            // System.Console.WriteLine(U);
            plt.SavePng($"Test{i}.png", 800, 600);
            i += 1;
        }

        System.Console.WriteLine(String.Join(", ", numPositiveDepths));

        var bestPositiveDepthIdx = numPositiveDepths.ToList().IndexOf(numPositiveDepths.Max());
        System.Console.WriteLine(bestPositiveDepthIdx);
        Utils.CreatePointCloud("test.ply", pointLists[bestPositiveDepthIdx]);
    }

    internal MathNet.Numerics.LinearAlgebra.Matrix<float> EstimateFundamentalMatrix(IList<KeypointPair> keypointPairs)
    {
        if (keypointPairs.Count < 8)
            throw new InvalidOperationException("At least 8 keypoint pairs must be provided");

        var mat = new DenseMatrix(keypointPairs.Count, 9);

        // TODO These matricies should translate and scale. Then, we can multiply them by the coordinates. This doesn't entirely make sense to 
        var transformationMatrix1 = CalculateTransformationMatrix(keypointPairs.Select(kpp => kpp.Keypoint1.Coordinate).ToList());
        var transformationMatrix2 = CalculateTransformationMatrix(keypointPairs.Select(kpp => kpp.Keypoint2.Coordinate).ToList());

        for (var i = 0; i < keypointPairs.Count; i += 1)
        {
            var kpPair = keypointPairs[i];
            var coord1 = transformationMatrix1.Multiply(MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(
                [kpPair.Keypoint1.Coordinate.X, kpPair.Keypoint1.Coordinate.Y, 1 ]));
            var coord2 = transformationMatrix2.Multiply(MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(
                [kpPair.Keypoint2.Coordinate.X, kpPair.Keypoint2.Coordinate.Y, 1 ]));

            mat[i, 0] = (float)(coord1[0] * coord2[0]);
            mat[i, 1] = (float)(coord1[0] * coord2[1]);
            mat[i, 2] = (float)coord1[0];
            mat[i, 3] = (float)(coord1[1] * coord2[0]);
            mat[i, 4] = (float)(coord1[1] * coord2[1]);
            mat[i, 5] = (float)coord1[1];
            mat[i, 6] = (float)coord2[0];
            mat[i, 7] = (float)coord2[1];
            mat[i, 8] = 1;
        }

        var svd = mat.Svd();

        var lastRow = svd.VT.Row(svd.VT.RowCount - 1);
        var fundamentalMatrix = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfColumnMajor(3, 3, lastRow);
        // TODO is the third value supposed to be 1?
        // var transformationMatrix1New = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfColumnMajor(
        //     3, 1, new List<float>() { (float)transformationMatrix1.X, (float)transformationMatrix1.Y, 1 });
        // var transformationMatrix2New = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfColumnMajor(
        //     3, 1, new List<float>() { (float)transformationMatrix2.X, (float)transformationMatrix2.Y, 1 });

        // System.Console.WriteLine($"Fundamental matrix dimensions: {fundamentalMatrix.RowCount} x {fundamentalMatrix.ColumnCount}");
        // System.Console.WriteLine($"TransformationMatrix 1 matrix dimensions: {transformationMatrix1New.RowCount} x {transformationMatrix1New.ColumnCount}");
        // System.Console.WriteLine($"TransformationMatrix 2 matrix dimensions: {transformationMatrix2New.RowCount} x {transformationMatrix2New.ColumnCount}");

        // return transformationMatrix2New.Transpose().Multiply(fundamentalMatrix).Multiply(transformationMatrix2New);
        return transformationMatrix2.Transpose().Multiply(fundamentalMatrix).Multiply(transformationMatrix1);
    }

    internal MathNet.Numerics.LinearAlgebra.Matrix<float> CalculateTransformationMatrix(List<Coordinate> coordinates)
    {
        (double X, double Y) translationMatrix = (X: 0, Y: 0);

        var centroid = CalculateCentroid(coordinates);

        foreach (var coordinate in coordinates)
        {
            translationMatrix.X += Math.Pow(coordinate.X - centroid.X, 2);
            translationMatrix.Y += Math.Pow(coordinate.Y - centroid.Y, 2);
        }

        var meanSquaredDistance = (translationMatrix.X + translationMatrix.Y) / coordinates.Count;

        var scale = Math.Pow(2 / meanSquaredDistance, 1 / 2);

        var translationMat = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(
            3, 3, [1, 0, -(float)centroid.X, 0, 1, -(float)centroid.Y, 0, 0, 1]);
        var scalingMat = MathNet.Numerics.LinearAlgebra.Matrix<float>.Build.DenseOfRowMajor(
            3, 3, [(float)scale, 0, 0, 0, (float)scale, 0, 0, 0, 1]);

        return translationMat.Multiply(scalingMat);
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