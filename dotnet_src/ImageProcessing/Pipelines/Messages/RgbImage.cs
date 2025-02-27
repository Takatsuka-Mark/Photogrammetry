using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Pipelines.Messages;

public class RgbImage : BaseMessage
{
    public override string GetName() => "RGB Image message";
    
    public required Matrix<Rgb> Image { get; init; }
}