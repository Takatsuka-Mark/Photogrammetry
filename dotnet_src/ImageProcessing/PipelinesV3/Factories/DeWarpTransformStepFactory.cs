using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageProcessing.PipelinesV3.DTOs;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using Microsoft.Extensions.Logging;
using PhotogrammetryStore;

namespace ImageProcessing.PipelinesV3.Factories;

public class DeWarpTransformStepFactory : ITransformStepFactory<MetadataStoreRecord, MetadataStoreRecord>
{
    private readonly MetadataStore _metadataStore;
    private readonly ILogger<DeWarpTransformStepFactory>? _logger;
    private readonly Lazy<Matrix<Uv>> _distortionMatrix;

    public DeWarpTransformStepFactory(MetadataStore metadataStore, DeWarp deWarp, ILogger<DeWarpTransformStepFactory>? logger = null)
    {
        _metadataStore = metadataStore;
        _logger = logger;
        // TODO should possibly have some way to change configuration live.

        _distortionMatrix = new Lazy<Matrix<Uv>>(deWarp.GetDistortionMatrix);
    }

    public void Initialize()
    {
        if (!_distortionMatrix.IsValueCreated)
        {
            _ = _distortionMatrix.Value;
        }
        else
        {
            // TODO do logger.
            Console.WriteLine("WARN: It appears the DeWarp service has been doubly initialed");
        }
    }

    public TransformBlock<MetadataStoreRecord, MetadataStoreRecord> GetTransformBlock()
    {
        // TODO OOPness of this is kinda weird.
        return new TransformBlock<MetadataStoreRecord, MetadataStoreRecord>(DeWarpRecord);
    }

    public TransformBlock<MetadataStoreRecord, MetadataStoreRecord> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }

    private MetadataStoreRecord DeWarpRecord(MetadataStoreRecord record)
    {
        // TODO log category. also downgrade to debug
        // TODO determine how we can make this know which variant it should be pulling.
        // TODO could have some sense of tags that translate into all downstream variants. Like ("dewarped").
        _logger?.LogInformation("Dewarping image");

        var inputMatrix = _metadataStore.FetchRgba64(record.RecordGuid);
        var result = DeWarp.ApplyDistortionMat(inputMatrix, _distortionMatrix.Value);
        _metadataStore.StoreDeWarpedRgba64(record.RecordGuid, result);
        return record;
    }
}
