using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines.Items;

public class GrayscaleConverterItem : BaseItem
{
    private readonly ImageStore _imageStore;

    public GrayscaleConverterItem(ImageStore imageStore) : base(new EmptyOptions())
    {
        _imageStore = imageStore;
    }

    public override Task<BaseMessage> ProcessAsync(BaseMessage input, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Type GetInputType() => typeof(ImageMessage);

    public override Type GetOutputType() => typeof(ImageMessage);
}