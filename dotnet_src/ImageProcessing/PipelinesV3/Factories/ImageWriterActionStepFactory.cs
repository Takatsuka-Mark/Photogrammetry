using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageReader.LocalImageReader;
using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.PipelinesV3.Factories;

public class ImageWriterActionStepFactory : IActionStepFactory<Matrix<Rgba64>>
{
    private readonly LocalImageReader _localImageReader;

    public ImageWriterActionStepFactory(LocalImageReader localImageReader)
    {
        _localImageReader = localImageReader;
    }

    public ActionBlock<Matrix<Rgba64>> GetActionBlock()
    {
        // TODO need some way to specify the output filename?
        return new ActionBlock<Matrix<Rgba64>>(matrix =>
            _localImageReader.WriteImageToDirectoryV2(matrix, $"output_{Guid.NewGuid()}"));
    }

    public ActionBlock<Matrix<Rgba64>> GetAndInitActionBlock()
    {
        Initialize();
        return GetActionBlock();
    }

    public void Initialize()
    {
        // TODO could verify that the location is writable.
    }
}