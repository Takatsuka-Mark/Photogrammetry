using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageProcessing.PipelinesV3.DTOs;
using ImageReader.LocalImageReader;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using PhotogrammetryStore;

namespace ImageProcessing.PipelinesV3.Factories;

public class SingleImageReaderTransformStepFactory : ITransformStepFactory<string, MetadataStoreRecord>
{
    private readonly MetadataStore _metadataStore;
    private readonly LocalImageReader _localImageReader;

    public SingleImageReaderTransformStepFactory(MetadataStore metadataStore, LocalImageReader localImageReader)
    {
        _metadataStore = metadataStore;
        _localImageReader = localImageReader;
    }

    public void Initialize()
    {
        // TODO could do something like check to see if has perms to input directory?
    }

    public TransformBlock<string, MetadataStoreRecord> GetTransformBlock()
    {
        return new TransformBlock<string, MetadataStoreRecord>(ReadFromDirectory);
    }

    public TransformBlock<string, MetadataStoreRecord> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }

    private MetadataStoreRecord ReadFromDirectory(string filename)
    {
        var matrix = _localImageReader.ReadImageFromDirectoryV2(filename);
        
        // TODO could have this allow us to record.SaveRgba64(matrix)
        var record = _metadataStore.CreateRecord();
        _metadataStore.StoreRgba64(record, matrix);
        return new MetadataStoreRecord(record);
    }
}