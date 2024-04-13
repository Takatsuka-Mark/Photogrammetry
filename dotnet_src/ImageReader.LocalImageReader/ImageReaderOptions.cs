namespace ImageReader.LocalImageReader;

public class ImageReaderOptions
{
    public string RootDirectory { get; init; }
    public string RootOutputDirectory { get; init; }

    private const string RootDirectoryKey = "image_reader:root_directory";
    private const string RootOutputDirectoryKey = "image_reader:root_output_directory";
    
    public ImageReaderOptions()
    {
        RootDirectory = Environment.GetEnvironmentVariable(RootDirectoryKey) ?? 
                        throw new ArgumentNullException(RootDirectoryKey);
        RootOutputDirectory = Environment.GetEnvironmentVariable(RootOutputDirectoryKey) ??
                        throw new ArgumentNullException(RootOutputDirectoryKey);
    }
}