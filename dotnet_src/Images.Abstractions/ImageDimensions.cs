namespace Images.Abstractions;

public class ImageDimensions(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
}