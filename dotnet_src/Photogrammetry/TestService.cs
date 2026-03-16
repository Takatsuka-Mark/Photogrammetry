using System.Threading.Tasks.Dataflow;
using ImageProcessing;
using ImageProcessing.PipelinesV3.DTOs;
using ImageProcessing.PipelinesV3.Factories;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotogrammetryStore;
using SkiaSharp.HarfBuzz;

namespace Photogrammetry;

public class TestService : IHostedService
{
    private readonly MetadataStore _metadataStore;
    private readonly DeWarpTransformStepFactory _deWarpTransformStepFactory;
    private readonly SingleImageReaderTransformStepFactory _imageReaderTransformStepFactory;
    private readonly ImageWriterActionStepFactory _imageWriterActionStepFactory;
    private readonly KeyPointDetectionTransformStepFactory _keyPointDetectionTransformStepFactory;
    private readonly RedundantKeypointEliminatorTransformStepFactory _redundantKeypointEliminatorTransformStepFactory;
    private readonly KeypointMatching _keypointMatching;
    private readonly ILogger<TestService>? _logger;

    private readonly DataflowLinkOptions _linkOptions = new() { PropagateCompletion = true };

    public TestService(
        MetadataStore metadataStore,
        DeWarpTransformStepFactory deWarpTransformStepFactory,
        SingleImageReaderTransformStepFactory imageReaderTransformStepFactory,
        ImageWriterActionStepFactory imageWriterActionStepFactory,
        KeyPointDetectionTransformStepFactory keyPointDetectionTransformStepFactory,
        RedundantKeypointEliminatorTransformStepFactory redundantKeypointEliminatorTransformStepFactory,
        KeypointMatching keypointMatching,
        ILogger<TestService>? logger = null)
    {
        _metadataStore = metadataStore;
        _deWarpTransformStepFactory = deWarpTransformStepFactory;
        _imageReaderTransformStepFactory = imageReaderTransformStepFactory;
        _imageWriterActionStepFactory = imageWriterActionStepFactory;
        _keyPointDetectionTransformStepFactory = keyPointDetectionTransformStepFactory;
        _redundantKeypointEliminatorTransformStepFactory = redundantKeypointEliminatorTransformStepFactory;
        _keypointMatching = keypointMatching;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Starting Test Service");
        // await TestKeypointDetection(cancellationToken, "1_keypoint_detection/15pt_star.png");
        await TestKeypointDetection(cancellationToken, "2_keypoint_matching/15pt_star_shifted_150.png");
        await TestKeypointMatching(cancellationToken, "2_keypoint_matching/15pt_star.png",
            "2_keypoint_matching/15pt_star_shifted_150.png");
        _logger?.LogInformation("Finished Test Service");
        await Task.CompletedTask;
    }

    public async Task TestKeypointDetection(CancellationToken cancellationToken, string filename)
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

        inputItem.Post(filename);
        inputItem.Complete();

        await writer.Completion;
        _logger?.LogInformation("Completed pipeline");
    }

    public async Task TestKeypointMatching(CancellationToken cancellationToken, string image1Filename,
        string image2Filename)
    {
        var (inputItem, denoisedKeypointOutputItem) = BuildKeypointDetectorPipeline();

        inputItem.Post(image1Filename);
        inputItem.Post(image2Filename);
        inputItem.Complete();

        var image1Record = await denoisedKeypointOutputItem.ReceiveAsync(cancellationToken);
        var image2Record = await denoisedKeypointOutputItem.ReceiveAsync(cancellationToken);
        await denoisedKeypointOutputItem.Completion;

        _logger?.LogInformation("Matching Keypoints");
        var kp1 = _metadataStore.FetchDenoisedKeypoints(image1Record.RecordGuid);
        var kp2 = _metadataStore.FetchDenoisedKeypoints(image2Record.RecordGuid);
        var keypointPairs = _keypointMatching.MatchKeypoints(kp1, kp2);
        _logger?.LogInformation("Matched keypoints. Found {Ct} pairs", keypointPairs.Count);

        var image1 = _metadataStore.FetchRgba64(image1Record.RecordGuid);
        var image1Width = image1.GetDimensions().Width;

        var joinedImage = image1.JoinRight(_metadataStore.FetchRgba64(image2Record.RecordGuid));

        ResultBuilders.DrawSquares(joinedImage,
            keypointPairs
                .Select(pair => pair.Keypoint1)
                .Select(k => k.Coordinate)
                .ToList(),
            5,
            new Rgba64 { A = ushort.MaxValue, R = ushort.MaxValue, B = 0, G = 0 });
        ResultBuilders.DrawSquares(
            joinedImage,
            keypointPairs
                .Select(pair => pair.Keypoint2)
                .Select(k => k.Coordinate with { X = k.Coordinate.X + image1Width })
                .ToList(),
            5,
            new Rgba64 { A = ushort.MaxValue, R = 0, B = ushort.MaxValue, G = 0 });

        ResultBuilders.DrawLines(
            joinedImage,
            keypointPairs
                .Select(pair => (pair.Keypoint1.Coordinate, pair.Keypoint2.Coordinate with {X = pair.Keypoint2.Coordinate.X + image1Width}))
                .ToList(),
            new Rgba64 { A = ushort.MaxValue, R = 0, G = ushort.MaxValue, B = 0 }
        );

        // TODO investigate why there is some weird behavior with the placement of keypoints in the joined image.
        //      Drawn lines match, so there is some weird data.
        // TODO investigate double matched keypoints.
        
        var writer = _imageWriterActionStepFactory.GetAndInitActionBlock();
        writer.Post(joinedImage);
        writer.Complete();
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