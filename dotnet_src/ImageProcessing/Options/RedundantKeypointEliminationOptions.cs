namespace ImageProcessing.Options;

public class RedundantKeypointEliminationOptions
{
    public const string Section = "RedundantKeypointElimination";

    public int SuppressionRadius { get; init; }
}