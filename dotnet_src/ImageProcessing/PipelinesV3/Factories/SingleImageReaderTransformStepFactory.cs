using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageReader.LocalImageReader;
using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelinesV3.Factories;

public class SingleImageReaderTransformStepFactory : ITransformStepFactory<string, Matrix<Rgba64>>
{
    private readonly LocalImageReader _localImageReader;

    public SingleImageReaderTransformStepFactory(LocalImageReader localImageReader)
    {
        _localImageReader = localImageReader;
    }

    public void Initialize()
    {
        // TODO could do something like check to see if has perms to input directory?
    }

    public TransformBlock<string, Matrix<Rgba64>> GetTransformBlock()
    {
        return new TransformBlock<string, Matrix<Rgba64>>(filename =>
            _localImageReader.ReadImageFromDirectoryV2(filename));
    }

    public TransformBlock<string, Matrix<Rgba64>> GetAndInitTransformBlock()
    {
        Initialize();
        return GetTransformBlock();
    }
}