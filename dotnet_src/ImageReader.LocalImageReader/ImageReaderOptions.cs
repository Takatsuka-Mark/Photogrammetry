using Framework.BaseOptions;

namespace ImageReader.LocalImageReader;

public class ImageReaderOptions : BaseOptions
{
    public string RootDirectory { get; init; }
    public string RootOutputDirectory { get; init; }

    private const string RootDirectoryKey = "image_reader:root_directory";
    private const string RootOutputDirectoryKey = "image_reader:root_output_directory";
    
    public ImageReaderOptions()
    {
        RootDirectory = GetRequired(RootDirectoryKey);
        RootOutputDirectory = GetRequired(RootOutputDirectoryKey);
    }
}