using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Pipelines.Messages;

public class GrayscaleImage : BaseMessage
{
	public override string GetName() => "Grayscale Image message";
	
	public required Matrix<Grayscale> Image { get; init; } 
}