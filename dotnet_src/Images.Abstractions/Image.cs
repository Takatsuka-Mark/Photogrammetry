namespace Images.Abstractions;

public class Image<TPixel>
{
    public int Width { get; }
    public int Height { get; }
    
    private readonly TPixel[,] _pixels;

    public Image(int width, int height)
    {
        Width = width;
        Height = height;
        _pixels = new TPixel[width, height];
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
        if (!(x >= 0 && y >= 0 && x < Width && y < Height))
            throw new ArgumentOutOfRangeException($"Received X:{x}, Y:{y} for image of Height:{Height}, Width:{Width}");
    }
}