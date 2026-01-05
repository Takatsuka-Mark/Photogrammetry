using System.Numerics;
using Images.Abstractions;
using Images.Abstractions.Pixels;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;


namespace ImageReader.LocalImageReader;

public class LocalImageReader
{
    private readonly ImageReaderOptions _options;

    public LocalImageReader(IOptions<ImageReaderOptions> imageReaderOptions)
    {
        _options = imageReaderOptions.Value;
    }

    public LinearAlgebra.Matrix<Images.Abstractions.Pixels.Rgba64> ReadImageFromDirectoryV2(string filename)
    {
        // TODO check if 0,0 is the top left pixel.
        // TODO fix this whole file path thing.
        var rawImage = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba64>(new DecoderOptions { SkipMetadata = false },
            $"{_options.RootDirectory}/{filename}");
        var matrix = new LinearAlgebra.Matrix<Rgba64>(new LinearAlgebra.MatrixDimensions
            { Height = (ushort)rawImage.Height, Width = (ushort)rawImage.Width });

        for (ushort x = 0; x < rawImage.Width; x++)
        {
            for (ushort y = 0; y < rawImage.Height; y++)
            {
                var originalPixel = rawImage[x, y];
                matrix[x, y] = new Rgba64
                {
                    R = originalPixel.R,
                    G = originalPixel.G,
                    B = originalPixel.B,
                    A = originalPixel.A
                };
            }
        }

        return matrix;
    }

    public Images.Abstractions.Matrix<Rgba> ReadImageFromDirectory(string filename)
    {
        var rawImage = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba64>(new DecoderOptions { SkipMetadata = false },
            $"{_options.RootDirectory}/{filename}");
        var myImage = new Images.Abstractions.Matrix<Rgba>(new MatrixDimensions
            { Width = rawImage.Width, Height = rawImage.Height });

        for (var x = 0; x < rawImage.Width; x++)
        {
            for (var y = 0; y < rawImage.Height; y++)
            {
                var originalPixel = rawImage[x, y].ToVector4();
                myImage[x, y] = new Rgba
                {
                    R = originalPixel.X,
                    G = originalPixel.Y,
                    B = originalPixel.Z,
                    A = originalPixel.W
                };
            }
        }

        // TODO this is a bigtime hack.
        // if (filename.EndsWith(".jpg"))
        //     return myImage.Transpose();

        return myImage;
    }

    public void WriteImageToDirectoryV2(LinearAlgebra.Matrix<Images.Abstractions.Pixels.Rgba64> rawImage, string filename)
    {
        // TODO should probably move to a different class...
        using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba64>((int)rawImage.Dimensions.Width, (int)rawImage.Dimensions.Height))
        {
            for (var x = 0; x < rawImage.Dimensions.Width; x++)
            {
                for (var y = 0; y < rawImage.Dimensions.Height; y++)
                {
                    var originalPixel = rawImage[(ushort)x, (ushort)y];
                    image[x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba64(originalPixel.R, originalPixel.G,
                        originalPixel.B, originalPixel.A);
                }
            }

            var file = new FileInfo($"{_options.RootOutputDirectory}/{filename}.bmp");

            if (!file.Directory?.Exists ?? false)
            {
                Directory.CreateDirectory(file.Directory.FullName);
            }

            image.SaveAsBmp(file.FullName);
        }
    }
    
    public void WriteImageToDirectory(Images.Abstractions.Matrix<Rgba> rawImage, string filename)
    {
        // TODO should probably move to a different class...
        using (var image =
               new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba64>(rawImage.Dimensions.Width, rawImage.Dimensions.Height))
        {
            for (var x = 0; x < rawImage.Dimensions.Width; x++)
            {
                for (var y = 0; y < rawImage.Dimensions.Height; y++)
                {
                    var originalPixel = rawImage[x, y];
                    image[x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba64(new Vector4(originalPixel.R, originalPixel.G, originalPixel.B,
                        originalPixel.A));
                }
            }

            var file = new FileInfo($"{_options.RootOutputDirectory}/{filename}.bmp");

            if (!file.Directory?.Exists ?? false)
            {
                Directory.CreateDirectory(file.Directory.FullName);
            }

            image.SaveAsBmp(file.FullName);
        }
    }
}