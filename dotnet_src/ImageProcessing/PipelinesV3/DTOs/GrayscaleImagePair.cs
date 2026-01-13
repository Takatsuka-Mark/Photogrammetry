using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelinesV3.DTOs;

public record GrayscaleImagePair(Matrix<Rgba> RgbaImage, Matrix<Grayscale> GrayscaleImage);