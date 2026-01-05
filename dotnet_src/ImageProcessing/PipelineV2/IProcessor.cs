namespace ImageProcessing.PipelineV2;

public interface IProcessor<TInputItem, TOutputItem>
{
    public void Initialize();
    public TOutputItem Process(TInputItem inputItem);
}