using System.Threading.Tasks.Dataflow;
using ImageProcessing.PipelinesV3.DTOs;
using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelinesV3.Factories;

public static class Converters
{
    public static TransformBlock<Matrix<Rgba64>, GrayscaleImagePair> GetGrayscaleConverterTransformBlock()
    {
        return new TransformBlock<Matrix<Rgba64>, GrayscaleImagePair>(image =>
            new GrayscaleImagePair(image,
                (Matrix<Grayscale>)image.Convert((_, rgba64) => Grayscale.FromRgba64(rgba64))));
    }
}