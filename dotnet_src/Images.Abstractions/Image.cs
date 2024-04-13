namespace Images.Abstractions;

public class Image<TPixel>
{
    public ImageDimensions Dimensions { get; }

    private readonly TPixel[,] _pixels;

    public Image(ImageDimensions dimensions)
    {
        Dimensions = dimensions;
        _pixels = new TPixel[dimensions.Width, dimensions.Height];
    }

    public TPixel this[int x, int y]
    {
        get
        {
            ValidateCoords(x, y);
            return _pixels[x, y];
        }
        set
        {
            ValidateCoords(x, y);
            _pixels[x, y] = value;
        }
    }

    public void ValidateCoords(int x, int y)
    {
        if (!(x >= 0 && y >= 0 && x < Dimensions.Width && y < Dimensions.Height))
            throw new ArgumentOutOfRangeException($"Received X:{x}, Y:{y} for image of Height:{Dimensions.Width}, Width:{Dimensions.Height}");
    }
}