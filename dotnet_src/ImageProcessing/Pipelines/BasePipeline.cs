using ImageProcessing.Pipelines.Items;
using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines;

public abstract class BasePipeline
{
    // TODO not a huge fan of null suppression, but it might have to happen. sadly.
    protected BasePipelineOptions Options { get; private init; } = null!;
    protected List<BaseItem> PipelineItems { get; private init; } = null!;

    public abstract void ValidatePipeline();

    public void StartPipeline()
    {
        ValidatePipeline(); // TODO potentially double validating. Should set _isValidated flag.
    }
    
    // TODO maybe have some notion of "run pipeline with input?" and then track that item thorough the process?

    public abstract Task RunPipelineAsync(CancellationToken cancellationToken);

    public abstract class Builder<TPipeline> where TPipeline : BasePipeline, new()
    {
        private readonly BasePipelineOptions _options;
        private readonly List<BaseItem> _items = [];
        // TODO may want to introduce terminal items - like one that reads. Tbh the read location should exist though.
        // TODO also may want to make outputs nullable since the pipeline might fail but not want to stop functioning.

        public Builder(BasePipelineOptions options)
        {
            _options = options;
        }

        public Builder<TPipeline> AddItem(BaseItem item)
        {
            if (_items.Count <= 0)
            {
                _items.Add(item);
                return this;
            }

            var previousOutput = _items[^1].GetOutputType();
            var input = item.GetInputType();

            if (previousOutput != input)
                throw new Exception($"Cannot add item. Input type ({input.FullName}) does not match previous item's output type ({previousOutput.FullName})");

            _items.Add(item);
            return this;
        }

        internal TPipeline Build()
        {
            if (_items.Count <= 0)
                throw new Exception("Failed to build pipeline. There must be at least one item");

            return new TPipeline
            {
                // Not really sure if this pattern is best but trying it out.
                Options = _options,
                PipelineItems = _items
            };
        }
    }
}