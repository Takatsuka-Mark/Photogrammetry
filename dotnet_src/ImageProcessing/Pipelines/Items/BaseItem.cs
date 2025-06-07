using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines.Items;

public abstract class BaseItem
{
    private readonly BaseItemOptions _options;

    public BaseItem(BaseItemOptions options)
    {
        // TODO need to introduce logging
        _options = options;
    }

    public abstract Task<BaseMessage> ProcessAsync(BaseMessage input, CancellationToken cancellationToken);

    public abstract Type GetInputType();
    public abstract Type GetOutputType();
}