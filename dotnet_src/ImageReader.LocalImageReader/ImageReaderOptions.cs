using Framework;
using Microsoft.Extensions.Configuration;

namespace ImageReader.LocalImageReader;

public class ImageReaderOptions : BaseOptions
{
    private const string Section = "image_reader";
    
    public string RootDirectory { get; init; }
    public string RootOutputDirectory { get; init; }
    
    public ImageReaderOptions(IConfiguration configuration)
    {
        RootDirectory = GetRequired(configuration, Section, "root_directory");
        RootOutputDirectory = GetRequired(configuration, Section, "root_output_directory");
    }
}