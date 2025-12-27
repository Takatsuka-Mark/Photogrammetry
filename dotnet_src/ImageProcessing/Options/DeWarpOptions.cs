using System.ComponentModel.DataAnnotations;
using Images.Abstractions;

namespace ImageProcessing.Options;

public class DeWarpOptions
{
	public const string Section = "DeWarp";
	
	public required MatrixDimensions MatrixDimensions { get; init; }
	public required double[] DistortionCoefficients { get; init; }

	// public void Validate()
	// {
	// 	// TODO probably move into the service options validations.
	// 	// Could be nice to register options like cognitiv does.
	// 	if (DistortionCoefficients.Length != 5)
	// 		throw new Exception("Distortion coefficients must have length 5");
	// }
}