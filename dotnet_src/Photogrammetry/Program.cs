using ImageReader.LocalImageReader;

namespace Photogrammetry;

public class Program
{
    public static void Main(string[] args)
    {
        var imageReader = new LocalImageReader();
        var image = imageReader.ReadImageFromDirectory("straight_edge_1920x1080.jpg");
        imageReader.WriteImageToDirectory(image, "output.jpg");
    }
}