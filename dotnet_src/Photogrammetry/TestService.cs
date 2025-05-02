using ImageReader.LocalImageReader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Photogrammetry;

public class TestService : IHostedService
{
    private readonly IOptions<ImageReaderOptions> imageReaderOptions;

    public TestService(IOptions<ImageReaderOptions> imageReaderOptions){
        this.imageReaderOptions = imageReaderOptions;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"Hey look mom I made it. Options: {imageReaderOptions.Value.RootDirectory}");
        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine("I'm stopping so hard right now");
        throw new NotImplementedException();
    }
}