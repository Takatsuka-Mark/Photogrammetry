using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using Images.Abstractions.Pixels;
using SixLabors.ImageSharp;


namespace ImageReader.LocalImageReader;

public class LocalImageReader
{
    private readonly ImageReaderOptions _options;

    public LocalImageReader()
    {
        _options = new ImageReaderOptions();
    }

    public Images.Abstractions.Image<Rgba> ReadImageFromDirectory(string filename)
    {

        var rawImage = Image.Load<Rgba64>($"{_options.RootDirectory}/{filename}");
        var myImage = new Images.Abstractions.Image<Rgba>(rawImage.Width, rawImage.Height);
        
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

        return myImage;
    }

    public void WriteImageToDirectory(Images.Abstractions.Image<Rgba> rawImage, string filename)
    {
        using (var image = new Image<Rgba64>(rawImage.Width, rawImage.Height))
        {
            for (var x = 0; x < rawImage.Width; x++)
            {
                for (var y = 0; y < rawImage.Height; y++)
                {
                    var originalPixel = rawImage[x, y];
                    image[x, y] = new Rgba64(new Vector4(originalPixel.R, originalPixel.G, originalPixel.B, originalPixel.A));
                }
            }

            image.SaveAsBmp($"{_options.RootOutputDirectory}/{filename}.bmp");
        }
    }
}