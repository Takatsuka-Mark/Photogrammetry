namespace ImageProcessing.PipelineV2;

public class SequentialTerminalPipeline<TInputType, TOutputType>: BaseSequentialPipeline<TInputType, TInputType, TOutputType>
{
    private readonly IProcessor<TInputType, TOutputType> _processor;

    public SequentialTerminalPipeline(IProcessor<TInputType, TOutputType> processor) : base(processor)
    {
        _processor = processor;
    }

    public override void Initialize() => _processor.Initialize();

    public override TOutputType ExecuteSubPipeline(TInputType input) => _processor.Process(input);
}