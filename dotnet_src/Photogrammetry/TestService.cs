using ImageProcessing;
using ImageProcessing.Options;
using ImageProcessing.Pipelines;
using ImageProcessing.Pipelines.Items;
using ImageProcessing.Pipelines.Messages;
using ImageProcessing.PipelineV2;
using ImageProcessing.PipelineV2.Services;
using ImageReader.LocalImageReader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Photogrammetry;

public class TestService : IHostedService
{
    private readonly DeWarpSequentialPipeline.DeWarpSequentialPipelineBuilder _pipelineBuilder;
    private readonly DeWarpProcessor _deWarpProcessor;
    private readonly LocalImageReader _imageReader;
    private readonly ILogger<TestService>? _logger;

    public TestService(
        DeWarpSequentialPipeline.DeWarpSequentialPipelineBuilder pipelineBuilder,
        DeWarpProcessor deWarpProcessor,
        LocalImageReader imageReader,
        ILogger<TestService>? logger = null)
    {
        _pipelineBuilder = pipelineBuilder;
        _deWarpProcessor = deWarpProcessor;
        _imageReader = imageReader;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Starting Test Service");
        await RunDeWarpPipelineV2(cancellationToken);
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

    public async Task RunDeWarpPipelineV2(CancellationToken cancellationToken)
    {
        // TODO determine why double initialization.
        var pipeline = SequentialPipeline.Create(_deWarpProcessor);
        pipeline.Initialize();
        var image = _imageReader.ReadImageFromDirectoryV2("dewarp/straight_edge_1920x1080.jpg");
        var result = pipeline.Execute(image);
        _imageReader.WriteImageToDirectoryV2(result, "output");
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Stopping Test Service");
        return Task.CompletedTask;
    }
}