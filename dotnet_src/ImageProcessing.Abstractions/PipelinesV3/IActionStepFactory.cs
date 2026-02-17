using System.Threading.Tasks.Dataflow;

namespace ImageProcessing.Abstractions.PipelinesV3;

public interface IActionStepFactory<TInput> : IInitializable
{
    public ActionBlock<TInput> GetActionBlock();
    public ActionBlock<TInput> GetAndInitActionBlock();
}