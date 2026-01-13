using ImageProcessing.Abstractions;
using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Pipelines.Messages;

public class ImageMessage : BaseMessage
{
    public override string GetName() => "Image message";
    
    // TODO Would be nice to have some validation that each pipeline item will pull form something that exists.
    // Theoretically, this could store a collection of items that each has many attributes.
    // Then, we can pull just the individual item that we want. Could start with just a bunch of variables and an enum for each pipeline step...
    public ImageVariantKey? RgbImageKey { get; init; }
    public ImageVariantKey? GrayscaleImageKey { get; init; }
}