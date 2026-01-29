using System.Threading.Tasks.Dataflow;
using ImageProcessing.PipelinesV3.DTOs;
using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelinesV3.Factories;

public static class ResultBuilders
{
    public static TransformBlock<DetectedKeypoints, Matrix<Rgba64>> DetectedKeypointDrawerTransformBlock(ushort radius, Rgba64 dataToDraw)
    {
        // TODO Is making this generic necessary? If it is it's very possible
        return new TransformBlock<DetectedKeypoints, Matrix<Rgba64>>(keypointDetection =>
        {
            var keypoints = keypointDetection.Keypoints;
            // TODO make a deep copy method.
            var resultImage = (Matrix<Rgba64>)keypointDetection.ImagePair.RgbaImage.Convert((_, rgba64) => rgba64);

            foreach (var keypoint in keypoints)
            {
                DrawSquare(resultImage, keypoint.Coordinate, radius, dataToDraw);
            }

            return resultImage;
        });
    }
    
    public static void DrawSquare<TData>(Matrix<TData> matrix, Coordinate coordinate, ushort radius, TData dataToDraw)
    {
        for (var u = coordinate.X - radius; u < coordinate.X + radius; u += 1)
        {
            for (var v = coordinate.Y - radius; v < coordinate.Y + radius; v += 1)
            {
                if (!matrix.InBounds(u, v))
                {
                    continue;
                }

                matrix[(ushort)u, (ushort)v] = dataToDraw;
            }
        }
    }
}