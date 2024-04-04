using System.Drawing;
using System.Net;

namespace SambaReader;

public class SambaReader
{
    private readonly SambaReaderOptions _options;
    private readonly NetworkCredential _credential;

    public SambaReader()
    {
        _options = new SambaReaderOptions();
        _credential = new NetworkCredential(_options.Username, _options.Password, _options.RootDirectory);
    }

    public void ReadImageFromDirectory(string filename)
    {
        // TODO this is oversimplified...
        // var cred = _credential.GetCredential(new Uri(_options.RootDirectory), "Basic"))
        using (var fileStream = new FileStream($@"{_options.RootDirectory}{filename}", FileMode.Open, FileAccess.Read))
        {
            var image = Image.FromStream(fileStream);

            Console.WriteLine($"Image width: {image.Width}, Image Height: {image.Height}");
        }
    }
}