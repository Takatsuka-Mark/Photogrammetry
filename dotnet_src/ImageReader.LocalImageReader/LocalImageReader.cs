using System.Numerics;
using Images.Abstractions;
using SixLabors.ImageSharp.PixelFormats;
using Images.Abstractions.Pixels;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;


namespace ImageReader.LocalImageReader;

public class LocalImageReader
{
    private readonly ImageReaderOptions _options;

    public LocalImageReader(IConfiguration configuration)
    {
        _options = new ImageReaderOptions(configuration);
    }

    public Images.Abstractions.Matrix<Rgba> ReadImageFromDirectory(string filename)
    {

        var rawImage = Image.Load<Rgba64>(new DecoderOptions{SkipMetadata = false}, $"{_options.RootDirectory}/{filename}");
        var myImage = new Images.Abstractions.Matrix<Rgba>(new MatrixDimensions(rawImage.Width, rawImage.Height));
        
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
        if (filename.EndsWith(".jpg"))
            return myImage.Transpose();

        return myImage;
    }

    public void WriteImageToDirectory(Images.Abstractions.Matrix<Rgba> rawImage, string filename)
    {

        using (var image = new SixLabors.ImageSharp.Image<Rgba64>(rawImage.Dimensions.Width, rawImage.Dimensions.Height))
        {
            for (var x = 0; x < rawImage.Dimensions.Width; x++)
            {
                for (var y = 0; y < rawImage.Dimensions.Height; y++)
                {
                    var originalPixel = rawImage[x, y];
                    image[x, y] = new Rgba64(new Vector4(originalPixel.R, originalPixel.G, originalPixel.B, originalPixel.A));
                }
            }

            image.SaveAsBmp($"{_options.RootOutputDirectory}/{filename}.bmp");
        }
    }
}