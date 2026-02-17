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
    
    // public void DrawLine(Coordinate p1, Coordinate p2, TData dataToDraw)
    // {
    //     // Using bresehnam's line drawer
    //     ValidateCoords(p1.X, p1.Y);
    //     ValidateCoords(p2.X, p2.Y);
    //
    //     var dx = Math.Abs(p2.X - p1.X);
    //     var dy = Math.Abs(p2.Y - p1.Y);
    //     var sx = p1.X < p2.X ? 1 : -1;
    //     var sy = p1.Y < p2.Y ? 1 : -1;
    //     var err = dx - dy;
    //
    //     var x = p1.X;
    //     var y = p1.Y;
    //
    //     // TODO Kinda scary to have a while(true)
    //     while (true){
    //         _storage.SetOrDoNothing(x, y, dataToDraw);
    //
    //         if (x == p2.X && y == p2.Y)
    //             break;
    //
    //         var err2 = 2 * err;
    //
    //         if (err2 >= -dy){
    //             err -= dy;
    //             x += sx;
    //         }
    //         if (err2 <= dx){
    //             err += dx;
    //             y += sy;
    //         }
    //     }
    // }
}