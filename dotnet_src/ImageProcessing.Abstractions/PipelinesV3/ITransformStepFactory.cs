using System.Threading.Tasks.Dataflow;

namespace ImageProcessing.Abstractions.PipelinesV3;

public interface ITransformStepFactory<TInput, TOutput> : IInitializable
{
    public TransformBlock<TInput, TOutput> GetTransformBlock();
    public TransformBlock<TInput, TOutput> GetAndInitTransformBlock();
}