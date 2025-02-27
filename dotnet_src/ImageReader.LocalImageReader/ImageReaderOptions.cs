using Framework;
using Microsoft.Extensions.Configuration;

namespace ImageReader.LocalImageReader;

public class ImageReaderOptions : BaseOptions
{
    private const string Section = "image_reader";
    
    public string RootDirectory { get; init; }
    public string RootOutputDirectory { get; init; }
    
    public ImageReaderOptions(IConfiguration configuration) : base(configuration, Section)
    {
        RootDirectory = GetRequired("root_directory");
        RootOutputDirectory = GetRequired("root_output_directory");
    }
}