namespace Images.Abstractions.Pixels;

public class Grayscale
{
    // TODO should probably add some values
    public float K { get; init; }

    public static Grayscale FromRgb(Rgb input)
    {
        return new() { K = (input.R + input.B + input.G) / 3 };
    }
}