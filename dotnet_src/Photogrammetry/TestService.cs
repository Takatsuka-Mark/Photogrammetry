using System.Threading.Tasks.Dataflow;
using ImageProcessing.PipelinesV3.DTOs;
using ImageProcessing.PipelinesV3.Factories;
using ImageReader.LocalImageReader;
using Images.Abstractions.Pixels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotogrammetryStore;

namespace Photogrammetry;

public class TestService : IHostedService
{
    private readonly MetadataStore _metadataStore;
    private readonly DeWarpTransformStepFactory _deWarpTransformStepFactory;
    private readonly SingleImageReaderTransformStepFactory _imageReaderTransformStepFactory;
    private readonly ImageWriterActionStepFactory _imageWriterActionStepFactory;
    private readonly KeyPointDetectionTransformStepFactory _keyPointDetectionTransformStepFactory;
    private readonly RedundantKeypointEliminatorTransformStepFactory _redundantKeypointEliminatorTransformStepFactory;
    private readonly ILogger<TestService>? _logger;
    
    private readonly DataflowLinkOptions _linkOptions = new() { PropagateCompletion = true };

    public TestService(
        MetadataStore metadataStore,
        DeWarpTransformStepFactory deWarpTransformStepFactory,
        SingleImageReaderTransformStepFactory imageReaderTransformStepFactory,
        ImageWriterActionStepFactory imageWriterActionStepFactory,
        KeyPointDetectionTransformStepFactory keyPointDetectionTransformStepFactory,
        RedundantKeypointEliminatorTransformStepFactory redundantKeypointEliminatorTransformStepFactory,
        ILogger<TestService>? logger = null)
    {
        _metadataStore = metadataStore;
        _deWarpTransformStepFactory = deWarpTransformStepFactory;
        _imageReaderTransformStepFactory = imageReaderTransformStepFactory;
        _imageWriterActionStepFactory = imageWriterActionStepFactory;
        _keyPointDetectionTransformStepFactory = keyPointDetectionTransformStepFactory;
        _redundantKeypointEliminatorTransformStepFactory = redundantKeypointEliminatorTransformStepFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Starting Test Service");
        await RunPipeline(cancellationToken);
        _logger?.LogInformation("Finished Test Service");
        await Task.CompletedTask;
    }

    public async Task RunPipeline(CancellationToken cancellationToken)
    {
        // TODO this is rather clunky but I am not sure of a better system at the moment.
        //      Some way is needed to track everything that is linked, then build the transformation around them
        //      in a repeatable manner. i'm also unsure how to handle the settings for these. On restart seems, bad.
        
        // TODO could make each emit a list of variants that exist. Then, the consumer can determine if the list is ok.
        var (inputItem, outputItem) = BuildKeypointDetectorPipeline();
        var keypointDrawer = ResultBuilders.DetectedKeypointDrawerTransformBlock(_metadataStore, 5,
            new Rgba64 { A = ushort.MaxValue, R = ushort.MaxValue, B = 0, G = 0 });
        var writer = _imageWriterActionStepFactory.GetAndInitActionBlock();

        outputItem.LinkTo(keypointDrawer, _linkOptions);
        keypointDrawer.LinkTo(writer, _linkOptions);

        inputItem.Post("dewarp/straight_edge_1920x1080.jpg");
        inputItem.Complete();

        await writer.Completion;
        _logger?.LogInformation("Completed pipeline");
    }

    public (TransformBlock<string, MetadataStoreRecord> input, TransformBlock<MetadataStoreRecord, MetadataStoreRecord>
        output) BuildKeypointDetectorPipeline()
    {
        var imageReader = _imageReaderTransformStepFactory.GetAndInitTransformBlock();
        var deWarper = _deWarpTransformStepFactory.GetAndInitTransformBlock();
        var grayscaleConverter = Converters.GetGrayscaleConverterTransformBlock(_metadataStore);
        var keypointDetector = _keyPointDetectionTransformStepFactory.GetAndInitTransformBlock();
        var redundantKeypointDetector = _redundantKeypointEliminatorTransformStepFactory.GetAndInitTransformBlock();
        
        imageReader.LinkTo(deWarper, _linkOptions);
        deWarper.LinkTo(grayscaleConverter, _linkOptions);
        grayscaleConverter.LinkTo(keypointDetector, _linkOptions);
        keypointDetector.LinkTo(redundantKeypointDetector, _linkOptions);
        
        return (imageReader, redundantKeypointDetector);
    }
    

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Stopping Test Service");
        return Task.CompletedTask;
    }
}