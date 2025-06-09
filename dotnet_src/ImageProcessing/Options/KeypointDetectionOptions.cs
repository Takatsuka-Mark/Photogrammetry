namespace ImageProcessing.Options;

public class KeypointDetectionOptions
{
	public const string Section = "KeypointDetection";
	
	// Since grayscale is a float from 0 to 1, setting the threshold to a float. Could convert to a int [0, 255] instead (like original)
	// The difference between point's intensity for it to be considered a keypoint
	public float Threshold { get; init; }	// TODO naming?	
	public int GaussianStandardDeviation { get; init; }
	public int NumGaussianPairs { get; init; }
}