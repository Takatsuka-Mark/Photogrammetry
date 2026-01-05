namespace ImageProcessing.PipelineV2;

public static class SequentialPipeline
{
    // Really just for this static type parameterized create method.
    public static SequentialTerminalPipeline<TInputType, TOutputType> Create<TInputType, TOutputType>(IProcessor<TInputType, TOutputType> processor)
    {
        // TODO I don't really want to have to create it like this.
        //      I would prefer to call create with no parames, then down the line call "execute" 
        return new SequentialTerminalPipeline<TInputType, TOutputType>(processor);
    }
}

public class SequentialPipeline<TInputType, TProcessorInputType, TProcessorOutputType> : BaseSequentialPipeline<TInputType, TProcessorInputType, TProcessorOutputType>
{
    /// <summary>
    /// Thank you to Nico Schertler for this pattern https://stackoverflow.com/a/50664755
    /// </summary>
    
    private readonly ISequentialPipeline<TInputType, TProcessorInputType> _previousPipeline;

    public SequentialPipeline(IProcessor<TProcessorInputType, TProcessorOutputType> processor, ISequentialPipeline<TInputType, TProcessorInputType> previousPipeline) : base(processor)
    {
        // TODO think about how an end item with no return value can be set here.
        _previousPipeline = previousPipeline;
    }

    public override void Initialize()
    {
        _previousPipeline.Initialize();
        Processor.Initialize();
    }

    public override TProcessorOutputType ExecuteSubPipeline(TInputType input)
    {
        var previousPipelineResult = _previousPipeline.Execute(input);
        return Processor.Process(previousPipelineResult);
    }
}