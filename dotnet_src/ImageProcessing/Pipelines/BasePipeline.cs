using ImageProcessing.Pipelines.Items;
using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines;

public abstract class BasePipeline
{
    // TODO not a huge fan of null suppression, but it might have to happen. sadly.
    protected BasePipelineOptions Options { get; private init; } = null!;
    protected List<BaseItem<BaseMessage, BaseMessage>> PipelineItems { get; private init; } = null!;

    public abstract void ValidatePipeline();

    public void StartPipeline()
    {
        ValidatePipeline(); // TODO potentially double validating. Should set _isValidated flag.
    }

    public abstract Task RunPipelineAsync();

    public abstract class Builder<TPipeline> where TPipeline : BasePipeline, new()
    {
        private readonly BasePipelineOptions _options;
        private readonly List<BaseItem<BaseMessage, BaseMessage>> _items;
        // TODO may want to introduce terminal items - like one that reads. Tbh the read location should exist though.
        // TODO also may want to make outputs nullable since the pipeline might fail but not want to stop functioning.

        public Builder(BasePipelineOptions options)
        {
            _options = options;
            _items = new List<BaseItem<BaseMessage, BaseMessage>>();
        }

        public Builder<TPipeline> AddItem(BaseItem<BaseMessage, BaseMessage> item)
        {
            if (_items.Count <= 0)
            {
                _items.Add(item);
                return this;
            }

            var previousOutput = GetOutputMessageType(_items[^1]);
            var input = GetInputMessageType(item);

            if (previousOutput != input)
                throw new Exception($"Cannot add item. Input type ({input.FullName}) does not match previous item's output type ({previousOutput.FullName})");

            _items.Add(item);
            return this;
        }

        private Type GetInputMessageType<TInput, TOutput>(BaseItem<TInput, TOutput> _)
            where TInput : BaseMessage
            where TOutput : BaseMessage => typeof(TInput);

        private Type GetOutputMessageType<TInput, TOutput>(BaseItem<TInput, TOutput> _)
                    where TInput : BaseMessage
                    where TOutput : BaseMessage => typeof(TOutput);

        public TPipeline Build()
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