using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines.Items;

public abstract class BaseItem<TInput, TOutput> where TInput : BaseMessage where TOutput : BaseMessage
{
    private readonly BaseItemOptions _options;

    public BaseItem(BaseItemOptions options)
    {
        // TODO need to introduce logging
        _options = options;
    }

    public abstract Task<TOutput> ProcessAsync(TInput input);
}