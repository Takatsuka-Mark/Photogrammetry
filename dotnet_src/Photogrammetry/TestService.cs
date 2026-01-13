using System.Threading.Tasks.Dataflow;
using ImageProcessing;
using ImageProcessing.Options;
using ImageProcessing.Pipelines;
using ImageProcessing.Pipelines.Items;
using ImageProcessing.Pipelines.Messages;
using ImageProcessing.PipelinesV3.Factories;
using ImageReader.LocalImageReader;
using MathNet.Numerics.Integration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Photogrammetry;

public class TestService : IHostedService
{
    private readonly DeWarpSequentialPipeline.DeWarpSequentialPipelineBuilder _pipelineBuilder;
    private readonly LocalImageReader _imageReader;
    private readonly DeWarpTransformStepFactory _deWarpTransformStepFactory;
    private readonly SingleImageReaderTransformStepFactory _imageReaderTransformStepFactory;
    private readonly ImageWriterActionStepFactory _imageWriterActionStepFactory;
    private readonly KeyPointDetectionTransformStepFactory _keyPointDetectionTransformStepFactory;
    private readonly ILogger<TestService>? _logger;

    public TestService(
        DeWarpSequentialPipeline.DeWarpSequentialPipelineBuilder pipelineBuilder,
        LocalImageReader imageReader,
        DeWarpTransformStepFactory deWarpTransformStepFactory,
        SingleImageReaderTransformStepFactory imageReaderTransformStepFactory,
        ImageWriterActionStepFactory imageWriterActionStepFactory,
        KeyPointDetectionTransformStepFactory keyPointDetectionTransformStepFactory,
        ILogger<TestService>? logger = null)
    {
        _pipelineBuilder = pipelineBuilder;
        _imageReader = imageReader;
        _deWarpTransformStepFactory = deWarpTransformStepFactory;
        _imageReaderTransformStepFactory = imageReaderTransformStepFactory;
        _imageWriterActionStepFactory = imageWriterActionStepFactory;
        _keyPointDetectionTransformStepFactory = keyPointDetectionTransformStepFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Starting Test Service");
        await RunDeWarpPipelineV3(cancellationToken);
        _logger?.LogInformation("Finished Test Service");
        await Task.CompletedTask;
    }

    public async Task RunDeWarpPipelineV1(CancellationToken cancellationToken)
    {
        // TODO do this in an initialization step?
        var pipeline = _pipelineBuilder.BuildPipeline();
        var image = _imageReader.ReadImageFromDirectory("dewarp/straight_edge_1920x1080.jpg");
        var result = await pipeline.RunPipelineWithInput(new RgbaImage { Image = image }, cancellationToken);
        // TODO image reading pipeline item?
        _imageReader.WriteImageToDirectory(result.Image, "output");
    }

    public async Task RunDeWarpPipelineV3(CancellationToken cancellationToken)
    {
        // TODO this is rather clunky but I am not sure of a better system at the moment.
        //      Some way is needed to track everything that is linked, then build the transformation around them
        //      in a repeatable manner. i'm also unsure how to handle the settings for these. On restart seems, bad.
        var imageReader = _imageReaderTransformStepFactory.GetAndInitTransformBlock();
        var deWarper = _deWarpTransformStepFactory.GetAndInitTransformBlock();
        var writer = _imageWriterActionStepFactory.GetAndInitActionBlock();

        imageReader.LinkTo(deWarper);
        deWarper.LinkTo(writer);

        imageReader.Post("dewarp/straight_edge_1920x1080.jpg");
        _logger?.LogInformation("Completed pipeline");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Stopping Test Service");
        return Task.CompletedTask;
    }
}