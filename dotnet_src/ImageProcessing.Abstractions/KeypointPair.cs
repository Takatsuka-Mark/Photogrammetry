namespace ImageProcessing.Abstractions;

public class KeypointPair
{
    public Keypoint Keypoint1 { get; init; } = null!;
    public Keypoint Keypoint2 { get; init; } = null!;
    public int Distance { get; init; }
}