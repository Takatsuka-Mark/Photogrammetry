using System.Threading.Tasks.Dataflow;
using ImageProcessing.Abstractions.PipelinesV3;
using ImageReader.LocalImageReader;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using Microsoft.Extensions.Logging;

namespace ImageProcessing.PipelinesV3.Factories;

public class ImageWriterActionStepFactory : IActionStepFactory<Matrix<Rgba64>>
{
    private readonly LocalImageReader _localImageReader;
    private readonly ILogger<ImageWriterActionStepFactory>? _logger;

    public ImageWriterActionStepFactory(LocalImageReader localImageReader, ILogger<ImageWriterActionStepFactory>? logger)
    {
        _localImageReader = localImageReader;
        _logger = logger;
    }

    public ActionBlock<Matrix<Rgba64>> GetActionBlock()
    {
        // TODO need some way to specify the output filename?
        return new ActionBlock<Matrix<Rgba64>>(matrix =>
        {
            var filename = $"output_{Guid.NewGuid()}";
            _localImageReader.WriteImageToDirectoryV2(matrix, filename);
            
            // TODO would prefer to use a different logger that has the proper category name.
           _logger?.LogInformation("Output file written to {filename}", filename);
        });
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