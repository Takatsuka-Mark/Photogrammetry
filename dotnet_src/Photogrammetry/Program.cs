using SambaReader;

namespace Photogrammetry;

public class Program
{
    public static void Main(string[] args)
    {
        var sambaReader = new SambaReader.SambaReader();
        sambaReader.ReadImageFromDirectory("straight_edge_1920x1080.jpg");
    }
}