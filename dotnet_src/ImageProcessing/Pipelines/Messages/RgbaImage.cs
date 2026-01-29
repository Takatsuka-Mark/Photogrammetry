using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Pipelines.Messages;

public class RgbaImage : BaseMessage
{
    public override string GetName() => "RGB Image message";
    
    public required MatrixV1<Rgba> Image { get; init; }
}