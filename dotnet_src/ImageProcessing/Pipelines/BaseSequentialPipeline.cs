using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines;

// TODO don't really want to have these input and output types, because some input forms will have no input/output.
public abstract class BaseSequentialPipeline<TInputMessage, TOutputMessage> : BasePipeline where TInputMessage : BaseMessage where TOutputMessage : BaseMessage
{
    protected new SequentialPipelineOptions Options { get; private init; } = null!;

    public async Task<TOutputMessage> RunPipelineWithInput(TInputMessage baseMessage, CancellationToken cancellationToken)
    {
        BaseMessage lastMessage = baseMessage;

        foreach (var pipelineItem in PipelineItems)
        {
            lastMessage = await pipelineItem.ProcessAsync(lastMessage, cancellationToken);
        }

        return (TOutputMessage)lastMessage;
    }
}