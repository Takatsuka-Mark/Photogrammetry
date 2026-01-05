namespace ImageProcessing.PipelineV2;

public abstract class
    BaseSequentialPipeline<TInputType, TProcessorInputType, TProcessorOutputType> : ISequentialPipeline<TInputType, TProcessorOutputType>
{
    protected readonly IProcessor<TProcessorInputType, TProcessorOutputType> Processor;

    public BaseSequentialPipeline(IProcessor<TProcessorInputType, TProcessorOutputType> processor)
    {
        Processor = processor;
    }

    public BaseSequentialPipeline<TInputType, TProcessorOutputType, TOutputType> AppendProcessor<TOutputType>(
        IProcessor<TProcessorOutputType, TOutputType> newProcessor)
    {
        return new SequentialPipeline<TInputType, TProcessorOutputType, TOutputType>(newProcessor, this);
    }

    public abstract void Initialize();

    public TProcessorOutputType Execute(TInputType input)
    {
        // TODO add some context to this;
        return ExecuteSubPipeline(input);
    }

    public abstract TProcessorOutputType ExecuteSubPipeline(TInputType input);
}