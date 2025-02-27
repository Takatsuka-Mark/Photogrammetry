using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines;

public abstract class SequentialPipeline : BasePipeline
{
    protected new SequentialPipelineOptions Options { get; private init; } = null!;


    public new abstract class Builder<TPipeline>(SequentialPipelineOptions options)
        : BasePipeline.Builder<TPipeline>(options)
        where TPipeline : SequentialPipeline, new();
}