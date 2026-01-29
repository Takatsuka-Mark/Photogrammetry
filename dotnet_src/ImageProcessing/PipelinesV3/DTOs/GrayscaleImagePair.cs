using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelinesV3.DTOs;

public record GrayscaleImagePair(Matrix<Rgba64> RgbaImage, Matrix<Grayscale> GrayscaleImage);