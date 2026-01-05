namespace ImageProcessing.PipelineV2;

public interface ISequentialPipeline<TInputType, TOutputType>
{
    public void Initialize();
    public TOutputType Execute(TInputType input);
    public TOutputType ExecuteSubPipeline(TInputType input);
}