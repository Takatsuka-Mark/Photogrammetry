namespace Images.Abstractions.Pixels;

public class Uv
{
    public int U { get; init; }
    public int V { get; init; }

    public override string ToString()
    {
        return $"U: {U}, V: {V}";
    }
}