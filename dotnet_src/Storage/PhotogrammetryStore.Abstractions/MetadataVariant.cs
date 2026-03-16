namespace PhotogrammetryStore.Abstractions;

public enum MetadataVariant
{
	Rgba64 = 0,
	Greyscale = 1,
	Keypoints = 2,
	DeWarpedRgba64 = 3,
	DeWarpedGrayscale = 4,
	DeNoisedKeypoints = 5,
}
