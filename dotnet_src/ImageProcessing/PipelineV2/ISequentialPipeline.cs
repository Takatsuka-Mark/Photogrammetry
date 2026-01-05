namespace ImageProcessing.PipelineV2;

public interface ISequentialPipeline<TInputType, TOutputType>
{
    public void Initialize();
    public TOutputType Execute(TInputType input);
    // TODO Think about way to restrict calling this.
    public TOutputType ExecuteSubPipeline(TInputType input);
}